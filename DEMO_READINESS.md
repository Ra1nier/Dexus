# Demo readiness — 2026-09-16

Bungie sign-in is restored. Complete server credentials enable it without a
feature flag. This document supersedes the earlier disabled-preview status.

## Completed

- Core uses injected scoped dependencies and no constructor-started asynchronous
  work or private logger factory. Application service wiring lives in DexusServices.
- `/auth/bungie/start` and `/callback` implement OAuth on the server. The existing
  registered callback path is preserved; sessionStorage validation is removed.
- OAuth state has 256 bits of randomness, is hashed server-side, browser-bound,
  expires after ten minutes, and is atomically consumed before exchanging a code.
- Callback success/failure redirects to the account page without retaining the
  authorization code in the destination URL. Provider errors are not echoed.
- HttpOnly, Secure, SameSite=Lax session cookies contain a protected random session
  identifier, not tokens. Each session has isolated server-memory credentials.
- New circuits resolve the authenticated session before rendering account pages;
  redirects, reloads, and tabs retain account access for up to eight hours.
- POST logout validates antiforgery, clears the cookie, and revokes token access
  from existing bound circuits. Revoked sessions cannot be revived by a late refresh.
- Concurrent Core token refreshes share a session lock. Authenticated request
  methods reject absent/expired sessions, and action methods reject a mismatched
  membership type. API requests use only the server-bound session's token.
- Linked membership selection supports accounts without cross-save as well as
  prioritizing cross-save primary profiles.
- Credentials remain in server configuration or `/run/secrets`; browser credential
  entry and shared plaintext credential/token files remain removed.
- Managed HTTP clients have explicit timeouts. Manifest downloads stream to disk,
  and manifest initialization is serialized across circuits in this process.
- Circuit layout polling stops on disposal. Obsolete credential Docker volumes
  remain removed; existing operator data is not automatically deleted.

## Verified

- Release application build passed; supporting-project warnings remain.
- `dotnet run --project tests/Dexus.SecurityChecks -c Release` passed.
- Tests exercise real HTTP OAuth endpoints and the actual rendered account page
  with a fake Bungie provider: visible sign-in, successful callback, authenticated
  account rendering, separate visitor identities, reloads, browser binding,
  state replay/expiry, provider denial, cookie flags, logout antiforgery, active
  circuit revocation, session expiry, and non-cross-save membership selection.
- No real Bungie credentials or item transfers were used in these checks.

## Deployment setup

1. Set the three `BungieAPIKeys__...` settings in `.env` or mounted secrets.
2. Register `https://YOUR_HOST/callback` in Bungie's confidential application.
3. Set the exact AllowedHosts and trusted reverse-proxy IP; forward HTTPS scheme
   and support WebSockets. Keep secure cookies and HTTPS enabled.
4. Deploy one instance. Session memory is bounded and expires after eight hours;
   restarting the app requires visitors to sign in again.
5. Rehearse with real accounts before announcing the demo.

The former `Dexus__EnableLiveBungie` setting is no longer used. Sign-in appears
when credentials are configured, without requiring another opt-in setting.

## Remaining work

- Real Bungie/browser rehearsal: two independent accounts, inventory loading,
  refresh, logout, and a consented test transfer.
- Full local item/character ownership enforcement, transfer concurrency and
  invalid-input tests, and refresh/reconnect stress tests.
- Durable encrypted user-keyed storage if sessions need to survive restarts;
  distributed session coordination before adding multiple application replicas.
- Atomic manifest cache replacement, corrupt-download recovery, and further
  cancellation/timeout coverage for streamed downloads.
- CI is configured; Git-history secret scanning, container vulnerability
  scans, Docker rebuild, proxy/deployment verification, and warning cleanup.
- Retire old credential files/volumes and rotate any credentials previously exposed.

This file records the current behavior and verification limits.
