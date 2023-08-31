using System.Text.Json;

namespace RAIT.Core;

public class RaitConfig
{
    public static JsonSerializerOptions? SerializationOptions { get; set; }
}