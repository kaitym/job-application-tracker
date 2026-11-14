using System.Text.Json;
using System.Text.Json.Serialization;

namespace JobTrackerApi.Tests;

// Read and write JSON the same way the API does
public static class TestJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
