# AGENTS.md

## Project Structure
- This is a Vue 3 + TypeScript + Vite project
- Main entrypoint: `src/main.ts`
- Component files in `src/components/`
- Views in `src/views/`
- API clients generated from swagger.json

## Key Commands
- `npm install` - Install dependencies
- `npm run dev` - Run development server (port 30457)
- `npm run build` - Build for production
- `npm run lint` - Lint code with oxlint and eslint
- `npm run type-check` - Type check with vue-tsc
- `npm run api:sync` - Regenerate API clients from swagger.json

## Development Setup
- Uses Volar for Vue TypeScript support
- Requires Node.js version ^22.18.0 || >=24.12.0
- Development server runs on port 30457 (defined in vite.config.ts)

## Architecture Notes
- Uses Vite for fast development and builds
- TypeScript with strict checking (noUncheckedIndexedAccess)
- Path aliases configured (@/ for src/)
- API clients auto-generated from OpenAPI spec
- ESLint + oxlint for code quality