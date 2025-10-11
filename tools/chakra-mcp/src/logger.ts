import { appendFileSync } from "fs";
import { join } from "path";

export class Logger {
  private logFilePath: string;

  constructor(logFileName: string = "log.txt") {
    this.logFilePath = join(__dirname, "../", logFileName);
  }

  log(message: string): void {
    const timestamp = new Date().toISOString();
    const logEntry = `[${timestamp}] ${message}`;
    appendFileSync(this.logFilePath, logEntry + "\n");
  }

  logObject(obj: any): void {
    this.log(JSON.stringify(obj, null, 2));
  }

  logRequest(input: string): void {
    this.log(`REQUEST: ${input}`);
  }

  logResponse(response: any): void {
    this.log(`RESPONSE: ${JSON.stringify(response)}`);
  }
}
