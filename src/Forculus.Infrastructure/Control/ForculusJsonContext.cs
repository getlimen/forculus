using System.Text.Json.Serialization;
using Limen.Contracts.ForculusHttp;

namespace Forculus.Infrastructure.Control;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ConfigSnapshot))]
[JsonSerializable(typeof(IReadOnlyList<PeerSpec>))]
internal sealed partial class ForculusJsonContext : JsonSerializerContext;
