import { Logger } from "../../logger";
import { buildTmpPath } from "./index";

export const getCharts = {
  name: "getCharts",
  description: "Get the names of available charts",
  inputSchema: { type: "object", properties: {} },
  execute: async () => {
    const fs = await import("fs/promises");
    const chartsDir = buildTmpPath("charts");
    let chartNames: string[] = [];
    const logger = new Logger();
    try {
      const files = await fs.readdir(chartsDir);
      chartNames = files
        .filter((file: string) => file.endsWith(".md"))
        .map((file: string) => file.replace(/\.md$/, ""));
    } catch (e) {
      logger.log(`Charts from: ${chartsDir}`);
      logger.log(`Error reading charts directory: ${e}`);
      chartNames = ["Could not read charts directory."];
    }
    return {
      content: [
        {
          type: "text",
          text: JSON.stringify({ names: chartNames }),
        },
      ],
    };
  },
};

export const getChart = {
  name: "getChart",
  description: "Get the content of a specific chart documentation by name",
  inputSchema: {
    type: "object",
    properties: {
      name: { type: "string", description: "Chart name" },
    },
    required: ["name"],
  },
  execute: async (args: object) => {
    const { name } = args as { name: string };
    const fs = await import("fs/promises");
    const logger = new Logger();
    const filePath = buildTmpPath("charts", name + ".md");
    let fileContent = "";
    try {
      fileContent = await fs.readFile(filePath, "utf-8");
    } catch (e) {
      logger.log(`Chart from: ${filePath}`);
      logger.log(`Error reading chart: ${e}`);
      fileContent = "Could not read chart documentation.";
    }
    return {
      content: [
        {
          type: "text",
          text: fileContent,
        },
      ],
    };
  },
};
