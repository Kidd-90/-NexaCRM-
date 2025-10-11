FROM oven/bun:alpine

WORKDIR /app

COPY . .

RUN bun install && bun run update

CMD ["bun", "run", "src/server.ts"]