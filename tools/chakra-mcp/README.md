# Chakra UI MCP Server

Model Context Protocol server dla dokumentacji Chakra UI.

## Instalacja

```bash
pnpm install
```

## Aktualizacja dokumentacji

```bash
pnpm run update
```

## Uruchomienie servera

#### Krok 1: Zbuduj projekt

```bash
bun src/server.ts
```

#### Krok 2: Uruchom MCP Inspector

```bash
bunx @modelcontextprotocol/inspector
```

## Korzystanie z kontenera Docker

### Pobranie obrazu

```bash
docker pull szymii/chakra-mcp
```

### Uruchomienie serwera

```bash
docker run -it szymii/chakra-mcp
```

### Aktualizacja dokumentacji

Jeśli chcesz zaktualizować dokumentację, uruchom kontener z odpowiednim poleceniem:

```bash
docker run -it twoja-nazwa/chakra-mcp update
```

**W MCP Inspector użyj:**

- Command: `bun`
- Arguments: `run src/server.ts`

## Struktura projektu

- `src/server.ts` - główny plik MCP servera
- `src/update.ts` - skrypt do pobierania dokumentacji Chakra UI
- `src/tmp/` - katalog z pobraną dokumentacją

## Dostępne skrypty

- `pnpm start` - uruchamia server
- `pnpm run dev` - uruchamia server w trybie dev (TypeScript)
- `pnpm run update` - pobiera najnowszą wersję dokumentacji Chakra UI
