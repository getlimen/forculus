using Forculus.Application.Common.Interfaces;
using Limen.Contracts.ForculusHttp;
using Mediator;

namespace Forculus.Application.Commands.Peers;

public sealed record ApplyFullConfigCommand(ConfigSnapshot Snapshot) : ICommand<Unit>;

internal sealed class ApplyFullConfigCommandHandler : ICommandHandler<ApplyFullConfigCommand, Unit>
{
    private readonly IWireGuardDriver _wg;
    public ApplyFullConfigCommandHandler(IWireGuardDriver wg) { _wg = wg; }

    public async ValueTask<Unit> Handle(ApplyFullConfigCommand cmd, CancellationToken ct)
    {
        await _wg.ApplyConfigAsync(cmd.Snapshot, ct);
        return Unit.Value;
    }
}
