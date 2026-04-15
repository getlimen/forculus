using Forculus.Application.Common.Interfaces;
using Limen.Contracts.ForculusHttp;
using Mediator;

namespace Forculus.Application.Commands.Peers;

public sealed record UpsertPeerCommand(PeerSpec Peer) : ICommand<Unit>;

internal sealed class UpsertPeerCommandHandler : ICommandHandler<UpsertPeerCommand, Unit>
{
    private readonly IWireGuardDriver _wg;
    public UpsertPeerCommandHandler(IWireGuardDriver wg) { _wg = wg; }

    public async ValueTask<Unit> Handle(UpsertPeerCommand cmd, CancellationToken ct)
    {
        await _wg.UpsertPeerAsync(cmd.Peer, ct);
        return Unit.Value;
    }
}
