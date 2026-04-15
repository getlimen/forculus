using Limen.Contracts.ForculusHttp;

namespace Forculus.Application.Common.Interfaces;

public interface IWireGuardDriver
{
    Task ApplyConfigAsync(ConfigSnapshot snapshot, CancellationToken ct);
    Task UpsertPeerAsync(PeerSpec peer, CancellationToken ct);
    Task RemovePeerAsync(string publicKey, CancellationToken ct);
}
