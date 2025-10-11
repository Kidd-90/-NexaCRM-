const serverInfo = {
  name: "Chakra UI MCP Server",
  version: "1.0.0",
  description: "Documentation for Chakra UI v3",
};

export function initialize() {
  return {
    protocolVersion: "2025-03-26",
    capabilities: {
      tools: {
        listChanged: true,
      },
    },
    serverInfo,
  };
}
