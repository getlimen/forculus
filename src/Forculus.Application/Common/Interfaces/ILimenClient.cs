using Limen.Contracts.ForculusHttp;

namespace Forculus.Application.Common.Interfaces;

public interface ILimenClient
{
    Task<ConfigSnapshot> FetchConfigAsync(CancellationToken ct);
}
