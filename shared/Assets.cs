using System;
using System.IO;
using Newtonsoft.Json;

namespace CarApp.Assets;

public interface IAssetContext
{
    public string DataPath { get; set; }
}

public static class DataHelper
{
    public static string[] ReadFileStrings(this IAssetContext assets, string path)
    {
        string filePath = Path.Join(assets.DataPath, path);
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"No data file {path} found.");
            return null;
        }
        return File.ReadAllLines(filePath);
    }

    public static bool TryReadJson<T>(this IAssetContext assets, string path, JsonSerializer deserializer, out T result)
    {
        result = default;

        string filePath = Path.Join(assets.DataPath, path);
        if (!File.Exists(filePath))
            return false;

        try
        {
            using var fileStream = new StreamReader(filePath);
            using var jsonReader = new JsonTextReader(fileStream);
            result = deserializer.Deserialize<T>(jsonReader);
            return true;
        }
        catch (JsonException e)
        {
            Console.WriteLine($"Could not read {path} as {typeof(T).Name}: {e.ToString()}");
            return false;
        }
    }

    public static void WriteJson<T>(this IAssetContext assets, string path, JsonSerializer serializer, T value)
    {
        string filePath = Path.Join(assets.DataPath, path);
        using var fileStream = new StreamWriter(filePath);
        using var jsonWriter = new JsonTextWriter(fileStream);
        serializer.Serialize(jsonWriter, value);
    }
}