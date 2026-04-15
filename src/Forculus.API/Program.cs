using Forculus.Application.Commands.Peers;
using Forculus.Application.Common.Interfaces;
using Forculus.Application.Services;
using Forculus.Infrastructure.Control;
using Forculus.Infrastructure.WireGuard;
using Limen.Contracts.ForculusHttp;
using Mediator;

var builder = WebApplication.CreateBuilder(args);

#region Configure Services
builder.Services.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Scoped);

builder.Services.AddSingleton<IWireGuardDriver>(sp =>
    new WgCliDriver(sp.GetRequiredService<ILogger<WgCliDriver>>()));

builder.Services.AddHttpClient<ILimenClient, LimenHttpClient>(c =>
    c.BaseAddress = new Uri(builder.Configuration["Limen:BaseUrl"] ?? "http://limen:8080"));

builder.Services.AddHostedService<PeerReconciler>();
#endregion

var app = builder.Build();

#region Configure HTTP Pipeline
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.MapPost("/peers", async (PeerSpec peer, IMediator m, CancellationToken ct) =>
{
    await m.Send(new UpsertPeerCommand(peer), ct);
    return Results.Ok();
});

app.MapDelete("/peers/{pubkey}", async (string pubkey, IMediator m, CancellationToken ct) =>
{
    await m.Send(new RemovePeerCommand(pubkey), ct);
    return Results.Ok();
});

app.MapGet("/config", async (ILimenClient limen, CancellationToken ct) =>
    Results.Ok(await limen.FetchConfigAsync(ct)));
#endregion

app.Run();
