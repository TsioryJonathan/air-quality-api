# 0003. API keys with two roles

**Status:** accepted

## Context
Access rights must be managed per route: read access for consumers (Grafana, scripts), rule writes reserved for administration.

## Decision
Authentication through the `X-Api-Key` header, two roles (`reader`, `admin`) and two authorization policies (`CanRead`, `CanManageRules`). Keys come from configuration and are compared in constant time.

## Consequences
- Simple to deploy and test; no users, no expiry.
- For real accounts, the next step is OpenID Connect: the policies would stay as they are, only the authentication scheme would be replaced.
