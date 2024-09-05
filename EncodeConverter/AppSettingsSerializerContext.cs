using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EncodeConverter;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(List<int>))]
[JsonSerializable(typeof(string))]
public partial class AppSettingsSerializerContext : JsonSerializerContext;
