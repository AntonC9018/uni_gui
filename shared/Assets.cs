using System;
using System.IO;
using Newtonsoft.Json;

namespace CarApp.Assets;

public class AssetContext
{
    public string DataPath { get; set; }
}

public static class DataHelper
{
    public static AssetContext CreateAssetContext(string dataPath)
    {
        if (!Directory.Exists(dataPath))
        {
            Console.WriteLine("No data folder found. Make sure you're running the program in the right directory.");
            return null;
        }

        var assets = new AssetContext
        {
            DataPath = dataPath,
        };
        return assets;
    }

    public static string[] ReadFileStrings(this AssetContext assets, string path)
    {
        string filePath = Path.Join(assets.DataPath, path);
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"No data file {path} found.");
            return null;
        }
        return File.ReadAllLines(filePath);
    }

    public static bool TryReadJson<T>(this AssetContext assets, string path, JsonSerializer deserializer, out T result)
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

    public static void WriteJson<T>(this AssetContext assets, string path, JsonSerializer serializer, T value)
    {
        string filePath = Path.Join(assets.DataPath, path);
        using var fileStream = new StreamWriter(filePath);
        using var jsonWriter = new JsonTextWriter(fileStream);
        serializer.Serialize(jsonWriter, value);
    }
}