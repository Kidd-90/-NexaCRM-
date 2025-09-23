#!/usr/bin/env python3
"""Launch the Blazor WebClient, visit key routes with Playwright, and fail on console errors."""
from __future__ import annotations

import asyncio
import os
import signal
import subprocess
import sys
import threading
import time
from pathlib import Path
from typing import Iterable, List
from urllib.error import HTTPError, URLError
from urllib.parse import urljoin
from urllib.request import Request, urlopen

PLAYWRIGHT_IMPORT_ERROR = (
    "Playwright for Python is required. Install it with `pip install playwright` and `playwright install chromium`."
)


def stream_output(pipe, prefix: str) -> None:
    try:
        for raw_line in iter(pipe.readline, ""):
            line = raw_line.rstrip()
            if line:
                print(f"[{prefix}] {line}")
    finally:
        pipe.close()


def wait_for_server(base_url: str, timeout: float = 60.0) -> bool:
    target = base_url if base_url.endswith("/") else f"{base_url}/"
    deadline = time.time() + timeout
    while time.time() < deadline:
        try:
            with urlopen(Request(target, method="GET"), timeout=5) as response:  # nosec: B310 - internal URL
                if 200 <= response.status < 500:
                    return True
        except HTTPError as err:
            # Treat 4xx as ready (static Blazor returns 404 for unmatched paths)
            if 400 <= err.code < 500:
                return True
        except URLError:
            pass
        time.sleep(0.5)
    return False


def launch_server(base_url: str) -> subprocess.Popen:
    project = os.environ.get("NEXACRM_WEBCLIENT_PROJECT", "src/Web/NexaCRM.WebClient")
    command = [
        "dotnet",
        "run",
        "--project",
        project,
        "--configuration",
        os.environ.get("NEXACRM_DOTNET_CONFIGURATION", "Release"),
        "--no-build",
        "--urls",
        base_url,
    ]
    env = os.environ.copy()
    dotnet_root = env.get("DOTNET_ROOT", str(Path.home() / ".dotnet"))
    env["DOTNET_ROOT"] = dotnet_root
    env["PATH"] = f"{dotnet_root}:{env.get('PATH', '')}"
    process = subprocess.Popen(
        command,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        text=True,
        env=env,
    )
    assert process.stdout is not None
    thread = threading.Thread(target=stream_output, args=(process.stdout, "web"), daemon=True)
    thread.start()
    return process


def shutdown(process: subprocess.Popen) -> None:
    if process.poll() is not None:
        return
    process.send_signal(signal.SIGINT)
    try:
        process.wait(timeout=10)
        return
    except subprocess.TimeoutExpired:
        process.terminate()
        try:
            process.wait(timeout=5)
            return
        except subprocess.TimeoutExpired:
            process.kill()
            process.wait(timeout=5)


async def probe_pages(base_url: str, paths: Iterable[str]) -> List[str]:
    try:
        from playwright.async_api import async_playwright
    except ModuleNotFoundError as exc:  # pragma: no cover - import guard
        raise RuntimeError(PLAYWRIGHT_IMPORT_ERROR) from exc

    errors: List[str] = []
    async with async_playwright() as playwright:
        browser = await playwright.chromium.launch()
        context = await browser.new_context()
        for path in paths:
            page_url = urljoin(base_url if base_url.endswith("/") else f"{base_url}/", path.lstrip("/"))
            page = await context.new_page()
            page_errors: set[str] = set()

            def handle_console(msg) -> None:
                if msg.type == "error":
                    page_errors.add(f"console error on {page_url}: {msg.text}")

            def handle_page_error(err) -> None:
                page_errors.add(f"page error on {page_url}: {err}")

            def handle_request_failed(request) -> None:
                failure = request.failure
                error_text = getattr(failure, "error_text", "") if failure else ""
                page_errors.add(
                    f"request failed on {page_url}: {request.url}"
                    + (f" ({error_text})" if error_text else "")
                )

            page.on("console", handle_console)
            page.on("pageerror", handle_page_error)
            page.on("requestfailed", handle_request_failed)
            try:
                await page.goto(page_url, wait_until="networkidle")
                await page.wait_for_timeout(1000)
            except Exception as exc:  # pragma: no cover - surface navigation failure
                page_errors.add(f"navigation failed for {page_url}: {exc}")
            finally:
                if page_errors:
                    errors.extend(sorted(page_errors))
                await page.close()
        await browser.close()
    return errors


def main() -> int:
    base_url = os.environ.get("NEXACRM_BASE_URL", "http://127.0.0.1:5188")
    paths_to_probe = ["/", "/statistics/dashboard", "/sales-pipeline-page"]

    print(f"[quality-check] Launching web client at {base_url}...")
    process = launch_server(base_url)
    try:
        if not wait_for_server(base_url):
            print("[quality-check] Web client failed to start within timeout.", file=sys.stderr)
            return 1
        if process.poll() is not None:
            print(f"[quality-check] Web client exited early with code {process.returncode}.", file=sys.stderr)
            return process.returncode or 1
        errors = asyncio.run(probe_pages(base_url, paths_to_probe))
        if errors:
            print("[quality-check] JavaScript console issues detected:", file=sys.stderr)
            for item in errors:
                print(f"  - {item}", file=sys.stderr)
            return 1
        print("[quality-check] Console check passed with no errors.")
        return 0
    finally:
        shutdown(process)


if __name__ == "__main__":
    sys.exit(main())
