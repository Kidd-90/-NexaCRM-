import { Logger } from "../../logger";
import { buildTmpPath } from "./index";

export const getTheming = {
  name: "getTheming",
  description: "Get chapters of theming documentation",
  inputSchema: { type: "object", properties: {} },
  execute: async () => {
    const fs = await import("fs/promises");
    const themingDir = buildTmpPath("theming");

    let themingNames: string[] = [];
    const logger = new Logger();

    try {
      const files = await fs.readdir(themingDir);
      themingNames = files
        .filter((file: string) => file.endsWith(".md"))
        .map((file: string) => file.replace(/\.md$/, ""));
    } catch (e) {
      logger.log(`Theming from: ${themingDir}`);
      logger.log(`Error reading theming directory: ${e}`);
      themingNames = ["Could not read theming directory."];
    }
    return {
      content: [
        {
          type: "text",
          text: JSON.stringify({ names: themingNames }),
        },
      ],
    };
  },
};

export const getThemingChapter = {
  name: "getThemingChapter",
  description:
    "Get the content of a specific theming documentation chapter by name",
  inputSchema: {
    type: "object",
    properties: {
      name: { type: "string", description: "Theming chapter name" },
    },
    required: ["name"],
  },
  execute: async (args: object) => {
    const { name } = args as { name: string };
    const fs = await import("fs/promises");
    const { buildTmpPath } = require("./index");
    const filePath = buildTmpPath("theming", name + ".md");

    const logger = new Logger();
    let fileContent = "";

    try {
      fileContent = await fs.readFile(filePath, "utf-8");
    } catch (e) {
      logger.log(`Theming chapter from: ${filePath}`);
      logger.log(`Error reading theming chapter: ${e}`);
      fileContent = "Could not read theming chapter.";
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
