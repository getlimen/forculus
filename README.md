# Forculus

> *Roman god of the **door panel*** — invoked in Roman religious practice as one of the deities guarding specific parts of a door.

Forculus is the **WireGuard hub** component of [Limen](https://github.com/getlimen/limen). All managed nodes' agents connect their tunnels into Forculus. Peer management is driven by Limen via a small HTTP API.

## Role in the Limen platform

- **Listens on:** UDP 51820 (WireGuard) + HTTP 3004 (control API)
- **Control API:** REST — `POST /peers`, `DELETE /peers/{publicKey}`, `GET /config`
- **WireGuard backend:** kernel module via `wg` CLI subprocess (`wg syncconf`)
- **State:** stateless — full config pulled from Limen on boot + every 60s as reconcile
- **Deployed on:** the `control`-role node (alongside Limen and Postgres) in v1

## How it's installed

Part of the Limen compose bundle. When you `docker compose up` Limen on the control node, Forculus comes up too.

## Tech stack

.NET 10 / NativeAOT • `wg` / `wg-quick` CLI subprocess • `System.Net.Http` • ASP.NET Core Minimal APIs

## Status

In active development. See [`limen/docs/superpowers/plans/2026-04-14-plan-03-wireguard-forculus.md`](https://github.com/getlimen/limen/blob/main/docs/superpowers/plans/2026-04-14-plan-03-wireguard-forculus.md).

## Development

Forculus needs the `wg`/`wg-quick` tools (Linux only). Local dev on Windows is limited to non-WG code paths (HTTP endpoints, limen connectivity). Full local testing runs under Docker.

### Sync contracts from limen

Limen.Contracts source is checked in locally under `src/Limen.Contracts/`. When it changes upstream, run:

```bash
bash scripts/sync-contracts.sh
```

### Build the Docker image

```bash
docker build -t ghcr.io/getlimen/forculus:dev -f src/Forculus.API/Dockerfile .
```

## License

[Apache 2.0](LICENSE)
