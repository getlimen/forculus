using System.Net.Http.Json;
using Forculus.Application.Common.Interfaces;
using Limen.Contracts.ForculusHttp;

namespace Forculus.Infrastructure.Control;

public sealed class LimenHttpClient : ILimenClient
{
    private readonly HttpClient _http;
    public LimenHttpClient(HttpClient http) { _http = http; }

    public async Task<ConfigSnapshot> FetchConfigAsync(CancellationToken ct)
        => await _http.GetFromJsonAsync("/api/forculus/config", ForculusJsonContext.Default.ConfigSnapshot, ct)
            ?? throw new InvalidOperationException("limen returned empty ConfigSnapshot");
}
