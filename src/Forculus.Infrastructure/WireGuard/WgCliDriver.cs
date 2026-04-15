using System.Diagnostics;
using System.Text;
using Forculus.Application.Common.Interfaces;
using Limen.Contracts.ForculusHttp;
using Microsoft.Extensions.Logging;

namespace Forculus.Infrastructure.WireGuard;

public sealed class WgCliDriver : IWireGuardDriver
{
    private readonly ILogger<WgCliDriver> _log;
    private readonly string _iface;

    public WgCliDriver(ILogger<WgCliDriver> log, string iface = "wg0") { _log = log; _iface = iface; }

    public async Task ApplyConfigAsync(ConfigSnapshot snapshot, CancellationToken ct)
    {
        var configPath = "/etc/wireguard/wg0.conf";
        var strippedPath = "/tmp/wg0.stripped";

        var content = RenderConfig(snapshot);
        await File.WriteAllTextAsync(configPath, content, ct);

        if (!await InterfaceExistsAsync(ct))
        {
            await RunAsync("wg-quick", $"up {_iface}", ct);
            _log.LogInformation("Interface {Iface} brought up from scratch", _iface);
            return;
        }

        await RunShellAsync($"wg-quick strip {configPath} > {strippedPath}", ct);
        await RunAsync("wg", $"syncconf {_iface} {strippedPath}", ct);
        _log.LogInformation("Applied config with {N} peers via syncconf", snapshot.Peers.Count);
    }

    public Task UpsertPeerAsync(PeerSpec peer, CancellationToken ct)
        => RunAsync("wg", $"set {_iface} peer {peer.PublicKey} allowed-ips {peer.AllowedIps}", ct);

    public Task RemovePeerAsync(string publicKey, CancellationToken ct)
        => RunAsync("wg", $"set {_iface} peer {publicKey} remove", ct);

    private async Task<bool> InterfaceExistsAsync(CancellationToken ct)
    {
        try
        {
            var psi = new ProcessStartInfo("wg", $"show {_iface}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            using var p = Process.Start(psi)!;
            await p.WaitForExitAsync(ct);
            return p.ExitCode == 0;
        }
        catch { return false; }
    }

    private static string RenderConfig(ConfigSnapshot s)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[Interface]");
        sb.AppendLine($"Address = {s.InterfaceAddress}");
        sb.AppendLine($"ListenPort = {s.ListenPort}");
        sb.AppendLine($"PrivateKey = {s.ServerPrivateKey}");
        foreach (var p in s.Peers)
        {
            sb.AppendLine();
            sb.AppendLine("[Peer]");
            sb.AppendLine($"PublicKey = {p.PublicKey}");
            sb.AppendLine($"AllowedIPs = {p.AllowedIps}");
            if (p.PresharedKey is not null)
            {
                sb.AppendLine($"PresharedKey = {p.PresharedKey}");
            }
        }
        return sb.ToString();
    }

    private async Task RunAsync(string cmd, string args, CancellationToken ct)
    {
        var psi = new ProcessStartInfo(cmd, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        using var p = Process.Start(psi) ?? throw new InvalidOperationException($"Could not start {cmd}");
        var stderrTask = p.StandardError.ReadToEndAsync(ct);
        await p.WaitForExitAsync(ct);
        if (p.ExitCode != 0)
        {
            var err = await stderrTask;
            throw new InvalidOperationException($"`{cmd} {args}` failed with exit code {p.ExitCode}: {err}");
        }
    }

    private async Task RunShellAsync(string cmdline, CancellationToken ct)
    {
        var psi = new ProcessStartInfo("/bin/sh", $"-c \"{cmdline.Replace("\"", "\\\"")}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        using var p = Process.Start(psi) ?? throw new InvalidOperationException($"Could not start shell");
        var stderrTask = p.StandardError.ReadToEndAsync(ct);
        await p.WaitForExitAsync(ct);
        if (p.ExitCode != 0)
        {
            var err = await stderrTask;
            throw new InvalidOperationException($"shell `{cmdline}` failed: {err}");
        }
    }
}
