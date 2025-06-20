# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Totum is a full-stack microservices application built with .NET Aspire orchestration. It consists of an Identity Server (Arachne), a REST API backend (Cellarium Backend), and a React frontend (Cellarium Frontend), all managed through a unified development environment.

## Tech Stack

- **Backend**: ASP.NET Core 9.0 with Entity Framework Core
- **Authentication**: Duende IdentityServer 6 (OAuth2/OIDC)
- **Frontend**: React 18 + TypeScript + Vite
- **Orchestration**: Microsoft .NET Aspire
- **Package Management**: PNPM with workspaces
- **API Client**: Auto-generated from OpenAPI specs using Hey API

## Common Development Commands

### Start the entire application stack
```bash
dotnet run --project projects/RootHost
```

### Build and test
```bash
# Build solution
dotnet build

# Run all tests
dotnet test

# Run specific project tests
dotnet test projects/cellarium/cellarium-backend.Tests
```

### Frontend development
```bash
cd projects/cellarium/cellarium-frontend

# Install dependencies
pnpm install

# Start dev server
pnpm dev

# Generate API client from OpenAPI spec
pnpm gen-api
# or
just generate-api

# Build for production
pnpm build

# Lint and format
pnpm lint
```

### Backend development
```bash
cd projects/cellarium/cellarium-backend

# Run API directly
dotnet run

# Run with HTTPS profile
dotnet run --launch-profile https
```

## Architecture

### Service Structure
- **RootHost** (`projects/RootHost/`): Aspire orchestrator - manages all services and service discovery
- **Arachne** (`projects/Arachne/`): Identity Server providing OAuth2/OIDC authentication
- **Cellarium Backend** (`projects/cellarium/cellarium-backend/`): Main REST API with shopping list functionality
- **Cellarium Frontend** (`projects/cellarium/cellarium-frontend/`): React SPA with OIDC authentication

### Development URLs
- Aspire Dashboard: `https://localhost:17035`
- Identity Server: `https://localhost:5001`
- Backend API: `https://localhost:5002`
- Frontend: `http://localhost:6001`
- API Documentation: `https://localhost:5002/scalar/v1`

### Authentication Flow
1. Frontend redirects to Arachne Identity Server for authentication
2. User authenticates and is redirected back with authorization code
3. Frontend exchanges code for JWT tokens
4. Backend validates JWT tokens against Arachne
5. Client ID: `cellarium-client`, Scopes: `openid profile cellarium`

### API Development Pattern
1. Backend exposes OpenAPI specification at `/openapi/v1.json`
2. Frontend auto-generates TypeScript client using `pnpm gen-api`
3. Generated client is in `src/api/` directory
4. Always regenerate client after backend API changes

### Database
- Development: In-Memory database for rapid development
- Entity Framework Core with support for SQLite/SQL Server
- Migrations located in `projects/Arachne/Migrations/`

## Key Development Notes

### Frontend Code Organization
- Components use SCSS modules for styling
- Authentication state managed via React OIDC Context
- Protected routes require authentication
- API calls use auto-generated client from `src/api/`

### Backend Code Organization
- Controllers in `Controllers/` directory
- Entity models and DbContext for data access
- JWT Bearer authentication configured
- OpenAPI documentation automatically generated

### Running Individual Services
Each service can run independently for debugging:
- Identity Server: `dotnet run --project projects/Arachne`
- Backend API: `dotnet run --project projects/cellarium/cellarium-backend`
- Frontend: `cd projects/cellarium/cellarium-frontend && pnpm dev`

However, the recommended approach is using the Aspire orchestrator which handles service discovery and configuration.