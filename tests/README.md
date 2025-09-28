Playwright E2E for NexaCRM layout checks

Setup:

1. Start the WebClient in watch mode (in your workspace root):

   dotnet watch run --project src/NexaCRM.WebClient/NexaCRM.WebClient.csproj

2. In another terminal, install dev deps and run tests:

   npm install
   npm run test:e2e

Notes:
- Tests use BASE_URL environment variable if provided, otherwise http://localhost:60743.
- Tests are headless by default; set headless=false in tests/playwright.config.js for debugging.
