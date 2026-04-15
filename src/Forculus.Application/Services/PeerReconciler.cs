using Forculus.Application.Commands.Peers;
using Forculus.Application.Common.Interfaces;
using Mediator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Forculus.Application.Services;

public sealed class PeerReconciler : BackgroundService
{
    private readonly ILimenClient _limen;
    private readonly IMediator _mediator;
    private readonly ILogger<PeerReconciler> _log;

    public PeerReconciler(ILimenClient limen, IMediator mediator, ILogger<PeerReconciler> log)
    {
        _limen = limen; _mediator = mediator; _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await ApplyWithRetryAsync(ct);

        while (!ct.IsCancellationRequested)
        {
            try { await Task.Delay(TimeSpan.FromSeconds(60), ct); }
            catch (OperationCanceledException) { return; }

            try
            {
                var snap = await _limen.FetchConfigAsync(ct);
                await _mediator.Send(new ApplyFullConfigCommand(snap), ct);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Reconcile failed; will retry in 60s");
            }
        }
    }

    private async Task ApplyWithRetryAsync(CancellationToken ct)
    {
        var delay = TimeSpan.FromSeconds(5);
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var snap = await _limen.FetchConfigAsync(ct);
                await _mediator.Send(new ApplyFullConfigCommand(snap), ct);
                _log.LogInformation("Initial config applied with {N} peers", snap.Peers.Count);
                return;
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                _log.LogWarning(ex, "Boot config fetch failed; retrying in {Delay}", delay);
                try { await Task.Delay(delay, ct); } catch (OperationCanceledException) { return; }
                delay = TimeSpan.FromSeconds(Math.Min(60, delay.TotalSeconds * 2));
            }
        }
    }
}
