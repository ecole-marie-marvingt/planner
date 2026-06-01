# Planner – École Marie Marvingt

Application de réservation de créneaux sportifs (piscine).

## Structure du projet

```
planner/
├── frontend/          # Application React + Vite + Redux
│   ├── src/
│   ├── index.html
│   ├── package.json
│   └── vite.config.ts
│
├── backend/           # Solution .NET 10
│   ├── Planner.sln
│   ├── Planner.AppHost/       # Aspire AppHost (orchestration)
│   ├── Planner.ServiceDefaults/   # Defaults Aspire (OTel, health checks)
│   └── Planner.Api/           # ASP.NET Core Minimal API + Dapper + Npgsql
│
└── db/
    └── init/
        ├── 01_create_tables.sql   # Schéma PostgreSQL
        └── 02_seed_data.sql       # Données de démonstration
```

## Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) + [Yarn](https://yarnpkg.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (pour Aspire + Postgres)
- [.NET Aspire workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling) :
  ```bash
  dotnet workload install aspire
  ```

## Démarrage rapide

### Backend (API + Postgres via Aspire)

```bash
cd backend
dotnet run --project Planner.AppHost
```

Le dashboard Aspire s'ouvre automatiquement. L'API sera disponible sur `https://localhost:{port}/api/slots`.

### Frontend (React)

```bash
cd frontend
yarn install
yarn dev
```

L'application est accessible sur `http://localhost:5173/planner/`.

## API REST

| Méthode | Route | Description |
|---------|-------|-------------|
| `GET` | `/api/slots?startDate=YYYY-MM-DD&endDate=YYYY-MM-DD&email=...` | Lister les créneaux |
| `GET` | `/api/slots/{id}?email=...` | Détail d'un créneau |
| `POST` | `/api/slots/{id}/book` | Réserver (`{userName, email}`) |
| `DELETE` | `/api/slots/{id}/book/{bookingId}` | Annuler une réservation |

La documentation OpenAPI est disponible sur `/openapi/v1.json` en mode développement.

## Variables d'environnement

### Frontend (`frontend/.env.local`)

```
VITE_API_BASE_URL=https://localhost:PORT/api
```

### Backend

La chaîne de connexion PostgreSQL est injectée automatiquement par Aspire via la variable `ConnectionStrings__plannerdb`.


Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the ESLint configuration

If you are developing a production application, we recommend updating the configuration to enable type-aware lint rules:

```js
export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...

      // Remove tseslint.configs.recommended and replace with this
      tseslint.configs.recommendedTypeChecked,
      // Alternatively, use this for stricter rules
      tseslint.configs.strictTypeChecked,
      // Optionally, add this for stylistic rules
      tseslint.configs.stylisticTypeChecked,

      // Other configs...
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])
```

You can also install [eslint-plugin-react-x](https://github.com/Rel1cx/eslint-react/tree/main/packages/plugins/eslint-plugin-react-x) and [eslint-plugin-react-dom](https://github.com/Rel1cx/eslint-react/tree/main/packages/plugins/eslint-plugin-react-dom) for React-specific lint rules:

```js
// eslint.config.js
import reactX from 'eslint-plugin-react-x'
import reactDom from 'eslint-plugin-react-dom'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...
      // Enable lint rules for React
      reactX.configs['recommended-typescript'],
      // Enable lint rules for React DOM
      reactDom.configs.recommended,
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])
```
