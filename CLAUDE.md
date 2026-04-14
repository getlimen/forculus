# CLAUDE.md — forculus (WireGuard hub)

> **Project**: Forculus — component of [Limen](https://github.com/getlimen/limen)
> **Role**: Server-side WireGuard endpoint. All agents (Limentinus instances) connect their tunnels into Forculus. Forculus subprocesses `wg`/`wg-quick` to apply peer config pushed from Limen.

**For full project context, read [`limen/docs/HANDOFF.md`](https://github.com/getlimen/limen/blob/main/docs/HANDOFF.md).**

## Etymology
*Forculus* — Roman god of the **door panel**. Ovid and Augustine both record him as one of the deities Romans invoked for specific parts of a door. Forculus guards the solid structure of the gate itself — the thing between inside and outside.

## Tech Stack

- .NET 10 / ASP.NET Core with **NativeAOT**
- **`wg` CLI subprocess** (via `wg syncconf`) for kernel WireGuard config (same approach as Pangolin's Gerbil, but implemented in C#)
- HTTP REST endpoints (NOT WebSocket — Forculus is a "dumb relay")
- No database — state pulled from Limen on boot + every 60s reconcile
- Single-binary AOT container image, runs alongside Limen on `control`-role node

## Endpoints

```
POST   /peers                   — upsert a peer (from Limen)
DELETE /peers/{publicKey}       — remove a peer (from Limen)
GET    /config                  — full current config (for debugging)
GET    /stats                   — tunnel statistics
GET    /healthz                 — liveness probe
```

Forculus ALSO calls Limen:
```
GET    <limen>/api/forculus/config   — boot-time + reconcile fetch of full config
```

## Clean architecture

Same strict rules as `limen`. Domain layer is effectively empty for Forculus since there's no local DB. Put the `ConfigSnapshot` and `PeerSpec` types in `Limen.Contracts` (shared with Limen).

## What Forculus does NOT do

- Store peers persistently (no DB — rebuilds from Limen on boot)
- Make routing decisions (Limen decides what peers exist)
- Run the reverse proxy (that's Ostiarius)
- Run any app-level logic
- Accept commands via WebSocket — it's HTTP-only by design

## Lifecycle

1. **Boot**: HTTP GET `/api/forculus/config` from Limen with retry (5s → 60s backoff until success)
2. **Apply**: write `wg0.conf` + `wg syncconf wg0 wg0.conf`
3. **Runtime**: accept POST/DELETE `/peers` from Limen for incremental changes
4. **Reconcile**: every 60s, pull full config, diff, apply (fixes drift; covers Limen restart case that Gerbil doesn't)

## Conventions

Same as `limen`: English-only, Apache 2.0, conventional commits, **no AI attribution in commits**.
