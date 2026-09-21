# Security Policy

## Supported Deployment

Dexus is currently maintained as a portfolio/demo application. The Docker setup is intended for controlled demo deployments and local testing.

## Secrets

Do not commit Bungie API credentials, OAuth tokens, certificates, `.env` files, or generated profile data. Runtime credentials should be supplied through environment variables, Docker secrets, or the hosting provider's secret manager.

Expected application credential keys:

```bash
BungieAPIKeys__API_Key
BungieAPIKeys__Client_ID
BungieAPIKeys__Client_Secret
```

The production container does not receive a TLS certificate. HTTPS terminates at
Cloudflare, and cloudflared reaches the backend over private HTTP. The backend binds to loopback by default; bridge-mode deployments must restrict the published port with the NAS firewall.
Configure the public host and the proxy address exactly as observed by Dexus:

```bash
AllowedHosts=dexus.example.com
Dexus__KnownProxies__0=::ffff:172.17.0.1
```

The supplied run script rejects placeholder credentials and environment files
that are readable by group or other users.

## Known Security Limitation

Bungie OAuth is enabled by server-configured application credentials. Authorization
starts and completes in server endpoints; no tokens, secrets, or authorization
codes are passed to interactive components. State is random, hashed server-side,
browser-bound, expires after ten minutes, and is consumed before code exchange.
The callback redirects to a clean account URL on both success and failure.

Authentication cookies contain only a protected random session identifier.
Profiles live in a bounded server-memory session store for up to eight hours.
Circuits bind to a profile through the authenticated principal, and logout
revokes the session across its tabs and rejects late token refresh results.
Logout is a POST with antiforgery validation. Tokens are never persisted to disk.

This store supports one process. Restarts invalidate sessions. A durable encrypted
store is needed if logins must survive restarts or run across multiple replicas.
Full local item/character ownership and transfer concurrency tests, dependency
scans, and real-provider rehearsal remain in [DEMO_READINESS.md](DEMO_READINESS.md).

Existing credential/token files and old Docker volumes are not automatically
deleted. Operators must retire them and rotate any credentials previously exposed.
No historical Git secret scan has been completed in this pass.

Do not expose the backend port directly, mount the Docker socket, or mount NAS
system/storage paths. Restrict the container network from reaching NAS management
interfaces with host firewall rules.

## Reporting Issues

If you find a security issue in this demo, contact the repository owner privately before opening a public issue.
