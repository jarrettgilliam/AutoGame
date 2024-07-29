namespace AutoGame.Core.Models;

using System.Text.Json;
using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(WriteIndented = true, ReadCommentHandling = JsonCommentHandling.Skip)]
[JsonSerializable(typeof(Config))]
public partial class ConfigJsonSerializerContext : JsonSerializerContext;