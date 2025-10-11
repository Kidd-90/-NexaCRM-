import { Logger } from "../../logger";
import { buildTmpPath } from "./index";

export const getComponents = {
  name: "getComponents",
  description: "Get the names of available components",
  inputSchema: { type: "object", properties: {} },
  execute: async () => {
    const fs = await import("fs/promises");
    const componentsDir = buildTmpPath("components");
    let componentNames: string[] = [];
    const logger = new Logger();
    try {
      const files = await fs.readdir(componentsDir);
      componentNames = files
        .filter((file: string) => file.endsWith(".md"))
        .map((file: string) => file.replace(/\.md$/, ""));
    } catch (e) {
      logger.log(`Components from: ${componentsDir}`);
      logger.log(`Error reading components directory: ${e}`);
      componentNames = ["Could not read components directory."];
    }
    return {
      content: [
        {
          type: "text",
          text: JSON.stringify({ names: componentNames }),
        },
      ],
    };
  },
};

export const getComponent = {
  name: "getComponent",
  description: "Get the content of a specific component documentation by name",
  inputSchema: {
    type: "object",
    properties: {
      name: { type: "string", description: "Component name" },
    },
    required: ["name"],
  },
  execute: async (args: object) => {
    const { name } = args as { name: string };
    const fs = await import("fs/promises");
    const logger = new Logger();
    const filePath = buildTmpPath("components", name + ".md");
    let fileContent = "";
    try {
      fileContent = await fs.readFile(filePath, "utf-8");
    } catch (e) {
      logger.log(`Component from: ${filePath}`);
      logger.log(`Error reading component: ${e}`);
      fileContent = "Could not read component documentation.";
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
