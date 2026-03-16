# Project Architecture

<!-- ADAPT: Replace this entire document with your project's actual architecture -->

## Overview

<!-- ADAPT: Brief project description (1-2 sentences) -->

## Project Structure

<!-- ADAPT: Describe the directory layout and key entry points -->

```
<!-- ADAPT: Replace with actual project tree -->
src/
├── Api/           — Backend API
├── Web/           — Frontend SPA
└── Shared/        — Shared types/contracts
```

## Architectural Methodology

<!-- ADAPT: Describe your chosen methodology -->

This project follows the **IDesign** layered service architecture. For full methodology reference, see:
- [IDesign Reference Guide](architecture/idesign-reference.md)
- [IDesign Implementation Guide](architecture/idesign-implementation.md)

### Layer Summary

| Layer | Responsibility | Naming Convention |
|-------|---------------|-------------------|
| Clients | Presentation, HTTP routing, request/response mapping | `*Controller`, `*Client` |
| Managers | Use-case orchestration (nouns); near-expendable | `*Manager` |
| Engines | Reusable business rules (verbs); rare | `*Engine` |
| Resource Access | Data/resource access (business verbs, NOT CRUDs) | `*Accessor` |
| Utilities | Cross-cutting concerns | `*Helper`, `*Extensions` |

## Tech Stack

<!-- ADAPT: Replace with actual technologies -->

| Layer | Technology |
|-------|-----------|
| Backend | <!-- ADAPT --> |
| Frontend | <!-- ADAPT --> |
| Data | <!-- ADAPT --> |
| Infrastructure | <!-- ADAPT --> |
| Auth | <!-- ADAPT --> |

## Key Patterns

<!-- ADAPT: Document key architectural decisions and patterns used in this project -->
