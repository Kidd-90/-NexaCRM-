import { getCharts, getChart } from "./charts";
import { getComponents, getComponent } from "./components";
import { getStyling, getStylingChapter } from "./styling";
import { getTheming, getThemingChapter } from "./theming";
import { getMigratingGuideToV3 } from "./migrating-to-v3";
import { join } from "node:path";

interface Tool {
  name: string;
  description: string;
  inputSchema: object;
  execute: (args: object) => Promise<object>;
}

export const tools: Tool[] = [
  getCharts,
  getChart,
  getComponents,
  getComponent,
  getStyling,
  getStylingChapter,
  getTheming,
  getThemingChapter,
  getMigratingGuideToV3,
];

export function buildTmpPath(folder: string, file?: string) {
  if (file) {
    return join(__dirname, "../../tmp", folder, file);
  }
  return join(__dirname, "../../tmp", folder);
}
