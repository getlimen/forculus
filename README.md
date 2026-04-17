# Forculus

> *Latin: "door panel"* — Roman deity of the solid door structure, the barrier between inside and outside.

**Forculus** is the WireGuard hub component of [Limen](https://github.com/getlimen/limen). It runs alongside Limen on the control node and manages the server-side WireGuard interface via `wg syncconf`.

## Not a standalone tool

Forculus is deployed as part of the Limen control node stack. It has no database — peer config is pulled from Limen's API and reconciled every 60 seconds.

## Features

- **Kernel WireGuard** via `wg` CLI subprocess
- **HTTP API** for peer management (POST/DELETE /peers, GET /config, GET /stats)
- **Periodic reconciliation** — pulls full config from Limen, diffs, applies
- **Boot recovery** — fetches config from Limen on startup with retry

## Tech stack

.NET 10 / ASP.NET Core • NativeAOT • `wg`/`wg-quick` subprocess

## Architecture

See the [Limen design spec](https://github.com/getlimen/limen/blob/main/docs/superpowers/specs/2026-04-14-limen-design.md).

## License

[Apache 2.0](LICENSE)
