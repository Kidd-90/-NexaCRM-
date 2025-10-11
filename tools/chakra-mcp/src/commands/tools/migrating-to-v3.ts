import { Logger } from "../../logger";
import { buildTmpPath } from "./index";

export const getMigratingGuideToV3 = {
  name: "getMigratingGuideToV3",
  description: "Get migration guides to v3",
  inputSchema: { type: "object", properties: {} },
  execute: async () => {
    const fs = await import("fs/promises");
    const logger = new Logger();
    const filePath = buildTmpPath("migrating-to-v3", "migration-to-v3.md");
    let fileContent = "";
    try {
      fileContent = await fs.readFile(filePath, "utf-8");
    } catch (e) {
      logger.log(`Migration guide from: ${filePath}`);
      logger.log(`Error reading migration guide: ${e}`);
      fileContent = "Could not read migration guide.";
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
