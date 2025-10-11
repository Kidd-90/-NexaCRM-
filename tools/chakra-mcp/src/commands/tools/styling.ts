import { Logger } from "../../logger";
import { buildTmpPath } from "./index";

export const getStyling = {
  name: "getStyling",
  description: "Get chapters of styling documentation",
  inputSchema: { type: "object", properties: {} },
  execute: async () => {
    const fs = await import("fs/promises");
    const stylingDir = buildTmpPath("styling");
    let stylingNames: string[] = [];
    const logger = new Logger();
    try {
      const files = await fs.readdir(stylingDir);
      stylingNames = files
        .filter((file: string) => file.endsWith(".md"))
        .map((file: string) => file.replace(/\.md$/, ""));
    } catch (e) {
      logger.log(`Styling from: ${stylingDir}`);
      logger.log(`Error reading styling directory: ${e}`);
      stylingNames = ["Could not read styling directory."];
    }
    return {
      content: [
        {
          type: "text",
          text: JSON.stringify({ names: stylingNames }),
        },
      ],
    };
  },
};

export const getStylingChapter = {
  name: "getStylingChapter",
  description:
    "Get the content of a specific styling documentation chapter by name",
  inputSchema: {
    type: "object",
    properties: {
      name: { type: "string", description: "Styling chapter name" },
    },
    required: ["name"],
  },
  execute: async (args: object) => {
    const { name } = args as { name: string };
    const fs = await import("fs/promises");
    const logger = new Logger();
    const filePath = buildTmpPath("styling", name + ".md");
    let fileContent = "";
    try {
      fileContent = await fs.readFile(filePath, "utf-8");
    } catch (e) {
      logger.log(`Styling chapter from: ${filePath}`);
      logger.log(`Error reading styling chapter: ${e}`);
      fileContent = "Could not read styling chapter.";
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
