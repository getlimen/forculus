using Forculus.Application.Common.Interfaces;
using Mediator;

namespace Forculus.Application.Commands.Peers;

public sealed record RemovePeerCommand(string PublicKey) : ICommand<Unit>;

internal sealed class RemovePeerCommandHandler : ICommandHandler<RemovePeerCommand, Unit>
{
    private readonly IWireGuardDriver _wg;
    public RemovePeerCommandHandler(IWireGuardDriver wg) { _wg = wg; }

    public async ValueTask<Unit> Handle(RemovePeerCommand cmd, CancellationToken ct)
    {
        await _wg.RemovePeerAsync(cmd.PublicKey, ct);
        return Unit.Value;
    }
}
