using System;
using System.Text.Json;

namespace Newtonsoft
{
    // Minimal shim to satisfy compile-time references to Newtonsoft.Json.JsonConvert
    public static class JsonConvert
    {
        public static string SerializeObject(object obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        public static T DeserializeObject<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }

        public static object DeserializeObject(string json, Type type)
        {
            return JsonSerializer.Deserialize(json, type);
        }
    }
}
