# Cellarium Frontend

React + TypeScript frontend for the Cellarium shopping list management system.

## Tech Stack

- **React 18** + **TypeScript** for type-safe component development
- **Vite** for fast development and build tooling
- **SCSS Modules** for component-scoped styling
- **OIDC Client** for authentication with Arachne Identity Server
- **Auto-generated API Client** from OpenAPI specifications

## Development

```bash
# Install dependencies
pnpm install

# Start development server
pnpm dev

# Generate API client from backend OpenAPI spec
pnpm gen-api

# Build for production
pnpm build

# Lint and format code
pnpm lint
```

## URLs

- **Development**: http://localhost:6001
- **Backend API**: https://localhost:5002
- **API Documentation**: https://localhost:5002/scalar/v1

## Authentication

The frontend uses OIDC authentication flow:
- **Identity Provider**: Arachne Identity Server (https://localhost:5001)
- **Client ID**: `cellarium-client`
- **Scopes**: `openid profile cellarium`

## API Integration

The frontend uses an auto-generated TypeScript client from the backend's OpenAPI specification. After making backend API changes, regenerate the client:

```bash
pnpm gen-api
```

Generated client files are located in `src/api/` directory.
