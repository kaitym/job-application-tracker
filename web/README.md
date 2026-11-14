# Sun & Waves (website)

The coffee-shop front end for the job tracker: React + TypeScript + Tailwind, built with Vite.

```sh
npm install
npm run dev
```

Open http://localhost:5173. The API needs to be running too (`dotnet run --project src/JobTrackerApi` from the repo root). In development, Vite forwards `/api/*` to it on port 5181. In production, set `VITE_API_URL` to the API's address.

## Layout

```
src/
  App.tsx              logged in → dashboard, otherwise login
  api/                 API client and response types
  lib/stages.ts        coffee names for each status
  lib/session.ts       remembers the login token
  components/          screens and pieces (job card, level bar, ...)
```

The theme (colors and fonts) lives in `src/index.css`.
