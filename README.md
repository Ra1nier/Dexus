# Dexus

Dexus is a .NET 9 and Blazor application for managing Destiny 2 inventory data through Bungie's API. The current application focuses on OAuth sign-in, manifest download/caching, inventory visualization, and item transfer workflows between characters and the vault.

## Features

- Bungie OAuth sign-in with server-side application credentials.
- Destiny 2 profile, character, equipment, inventory, and vault loading.
- Local Destiny manifest download and SQLite-backed definition lookup.
- Item grouping by character, equipment state, and vault location.
- Drag-and-drop item transfer workflow through the Bungie API.
- Docker deployment with HTTPS support for OAuth redirect requirements.

## Tech Stack

- .NET 9
- Blazor interactive server/client components
- MudBlazor UI components
- Bungie.net OAuth and Destiny 2 Platform APIs
- SQLite manifest querying with `Microsoft.Data.Sqlite`
- Docker

## Configuration

Dexus expects Bungie application credentials from ASP.NET configuration. For local Docker runs, place values in a local `.env` file based on `.env.example`:

```bash
BungieAPIKeys__API_Key=replace-with-your-bungie-api-key
BungieAPIKeys__Client_ID=replace-with-your-bungie-client-id
BungieAPIKeys__Client_Secret=replace-with-your-bungie-client-secret
ASPNETCORE_Kestrel__Certificates__Default__Password=replace-with-your-local-dev-cert-password
```

Do not commit `.env`, certificates, OAuth token files, or generated manifest data.

## HTTPS

Bungie OAuth redirect URLs require HTTPS. The Docker image expects a mounted certificate at `/https/dexus.pfx`.

For local testing:

```bash
mkdir -p https
dotnet dev-certs https -ep ./https/dexus.pfx -p "dev-password"
dotnet dev-certs https --trust
```

Then set the matching certificate password in `.env`.

## Docker

Build the image:

```bash
docker build -t dexus .
```

Run the container:

```bash
docker run --env-file .env -p 5001:5001 -v "$PWD/https:/https:ro" dexus
```

Open:

```text
https://localhost:5001
```

## Security Notes

- Bungie application credentials are loaded from environment/configuration, not committed source files.
- Local OAuth tokens and generated profile data are ignored by Git and Docker build context rules.
- The current token/profile persistence layer is file-backed. For a public multi-user deployment, replace this with isolated per-user storage or deploy the app as a private/single-user demo.
- Avoid publishing a Docker image that contains local build output, certificates, manifest caches, or token files.

## Current Limitations

- File-backed user token/profile storage is not suitable for a multi-user public service.
- Automated test coverage has not been added yet.
- Some compiler and StyleCop warnings remain in supporting model/request projects.
- The hosted demo should be treated as a portfolio preview unless the persistence model is hardened.

## Release Checklist

- Build with `dotnet build Dexus/Dexus/Dexus.csproj -c Release`.
- Build the Docker image from a clean working tree.
- Verify `.env`, `https/`, `.idea/`, `bin/`, `obj/`, and generated manifest/profile data are not included.
- Confirm the Bungie application redirect URL matches the hosted HTTPS URL.
- Test sign-in, manifest download, inventory loading, and one safe transfer workflow.
