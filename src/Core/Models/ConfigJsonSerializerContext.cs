namespace AutoGame.Core.Models;

using System.Text.Json;
using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(WriteIndented = true, ReadCommentHandling = JsonCommentHandling.Skip)]
[JsonSerializable(typeof(Config))]
internal partial class ConfigJsonSerializerContext : JsonSerializerContext;