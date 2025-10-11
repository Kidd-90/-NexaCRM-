#!/usr/bin/env node

import * as readline from "readline";
import { stdin, stdout } from "node:process";
import { Logger } from "./logger";
import { initialize } from "./commands/initialize";
import { tools } from "./commands/tools";

class ChakraMCPServer {
  private rl: readline.Interface;
  private logger: Logger;

  constructor() {
    this.rl = readline.createInterface({
      input: stdin,
      output: stdout,
    });
    this.logger = new Logger();
  }

  sendResponse(id: string | number, result: object) {
    const response = {
      jsonrpc: "2.0",
      id: id,
      result: result,
    };

    this.logger.logResponse(response);
    console.log(JSON.stringify(response));
  }

  start() {
    this.rl.on("line", async (line) => {
      try {
        const input = line.trim();
        const json = JSON.parse(input);
        this.logger.logRequest(input);

        if (json.jsonrpc !== "2.0")
          throw new Error("Incompatible JSON-RPC version");

        if ((json.method as string).includes("notification")) return;

        switch (json.method) {
          case "initialize":
            this.sendResponse(json.id, initialize());
            break;
          case "tools/list":
            this.sendResponse(json.id, {
              tools: tools.map((tool) => ({
                name: tool.name,
                description: tool.description,
                inputSchema: tool.inputSchema,
              })),
            });
            break;
          case "tools/call":
            const tool = tools.find((t) => t.name === json.params.name);
            if (!tool) {
              this.sendResponse(json.id, {
                error: {
                  code: -32602,
                  message: `MCP error -32602: Tool ${json.params.name} not found`,
                },
              });
              return;
            }

            const toolResponse = await tool.execute(json.params.arguments);
            this.sendResponse(json.id, toolResponse);
            break;
          case "ping":
            this.sendResponse(json.id, {});
            break;
          default:
            throw new Error(`Unknown method: ${json.method}`);
        }
      } catch (error) {
        console.error("Invalid JSON input:", error);
      }
    });

    this.rl.on("close", () => {
      console.log("Server shutting down...");
      process.exit(0);
    });
  }
}

const server = new ChakraMCPServer();
server.start();
