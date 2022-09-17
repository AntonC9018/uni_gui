using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public struct PersonNames
{
    public string FirstName;
    public string LastName;
}

public enum EngineKind
{
    Diesel,
    Bio,
    Gas,
    Electric,
    Count,
}

public struct RGBColor
{
    public uint HexValue;

    public RGBColor(uint hexValue)
    {
        HexValue = hexValue;
    }

    public RGBColor(int r, int g, int b, int a = 0xFF)
    {
        Debug.Assert(r > 0 && r <= 0xFF);
        Debug.Assert(g > 0 && g <= 0xFF);
        Debug.Assert(b > 0 && b <= 0xFF);
        Debug.Assert(a > 0 && a <= 0xFF);
        HexValue = unchecked(
            ((uint) r << 0)
            | ((uint) g << 8)
            | ((uint) b << 16)
            | ((uint) a << 24));
    }

    public int Red
    {
        readonly get => unchecked((int) ((HexValue >> 0) & 0xFF));
        set
        {
            Debug.Assert(value > 0 && value <= 0xFF);
            HexValue = unchecked((uint) (HexValue & ~(0xFF << 0)) | ((uint) value << 0));
        }
    }
    public int Green
    {
        readonly get => unchecked((int) ((HexValue >> 8) & 0xFF));
        set
        {
            Debug.Assert(value > 0 && value <= 0xFF);
            HexValue = unchecked((uint) (HexValue & ~(0xFF << 8)) | ((uint) value << 8));
        }
    }
    public int Blue
    {
        readonly get => unchecked((int) ((HexValue >> 16) & 0xFF));
        set
        {
            Debug.Assert(value > 0 && value <= 0xFF);
            HexValue = unchecked((uint) (HexValue & ~(0xFF << 16)) | ((uint) value << 16));
        }
    }
    public int Alpha
    {
        readonly get => unchecked((int) ((HexValue >> 24) & 0xFF));
        set
        {
            Debug.Assert(value > 0 && value <= 0xFF);
            HexValue = unchecked((uint) (HexValue & ~(0xFF << 24)) | ((uint) value << 24));
        }
    }
}

public enum CurrencyKind
{
    USDollar,
    Euro,
    Count,
}

public struct Currency
{
    public decimal Value;
    public CurrencyKind Kind;
}

public class CarModel
{
    public string NumberplateText;
    public DateTime ManufacturedDate;
    public int ManufacturerId;
    public Currency Price;
    public int CountryId;
    public PersonNames? Owner;
    public EngineKind EngineKind;
    public float KilometersTravelled;
    public float MassInKilograms;
    public int NumWheels;
    public int NumDoors;
    public RGBColor Color;

    public bool IsOwned => Owner.HasValue;
}

public class AssetContext
{
    public string DataPath { get; set; }
}

public static class DataHelper
{
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

public static class RandomHelper
{
    public static char NextRandomCapitalLetter(this Random rng)
    {
        int v = rng.Next();
        int offset = v % ('A' - 'Z' + 1);
        return (char) (offset + 'A');
    }

    public static float Range(this Random rng, float min, float max)
    {
        return rng.NextSingle() * (max - min) + min;
    }

    public static int Range(this Random rng, int min, int maxInclusive)
    {
        return (rng.Next() % (maxInclusive - min + 1)) + min;
    }

    public static int NextInt(this Random rng, int maxExclusive)
    {
        return rng.Next() % maxExclusive;
    }
}

public static class NumberHelper
{
    public static float TruncateToDecimalDigits(float value, int digits)
    {
        float mult = MathF.Pow(10.0f, digits);
        float result = MathF.Truncate(mult * value) / mult;
        return result;
    }
}

public class CarDependenciesRegistry
{
    private const string _ManufacturersFileName = "car_manufacturers.txt";
    public string[] Manufacturers { get; private set; }

    private const string _CountriesFileName = "countries.txt";
    public string[] Countries { get; private set; }

    public bool Initialize(AssetContext assets)
    {
        Manufacturers = assets.ReadFileStrings(_ManufacturersFileName);
        if (Manufacturers is null)
            return false;

        Countries = assets.ReadFileStrings(_CountriesFileName);
        if (Countries is null)
            return false;

        return true;
    }
}

public class OnlyFieldsResolver : DefaultContractResolver
{
    public static readonly OnlyFieldsResolver Instance = new OnlyFieldsResolver();

    public OnlyFieldsResolver()
    {
        this.IgnoreSerializableAttribute = true;
        this.IgnoreSerializableInterface = true;
        this.IgnoreShouldSerializeMembers = true;
        this.SerializeCompilerGeneratedMembers = true;
    }

    protected override JsonContract CreateContract(Type objectType)
    {
        return base.CreateContract(objectType);
    }

    protected override List<MemberInfo> GetSerializableMembers(Type objectType)
    {
        return objectType
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Cast<MemberInfo>()
            .ToList();
    }
}

public sealed class HexStringJsonConverter : JsonConverter
{
    public static readonly HexStringJsonConverter Instance = new HexStringJsonConverter();

    public override bool CanConvert(Type objectType)
    {
        return typeof(uint).Equals(objectType);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue($"0x{value:X}");
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var value = reader.Value;
        if (value is not string str || !str.StartsWith("0x"))
            throw new JsonSerializationException("Expected a hex string.");
        return uint.Parse(str.AsSpan("0x".Length), System.Globalization.NumberStyles.HexNumber);
    }
}

public sealed class CurrencyKindConverter : JsonConverter
{
    public static readonly CurrencyKindConverter Instance = new CurrencyKindConverter();

    public override bool CanConvert(Type objectType)
    {
        return typeof(CurrencyKind).Equals(objectType);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        switch ((CurrencyKind) value)
        {
            case CurrencyKind.USDollar:
            {
                writer.WriteValue("$");
                break;
            }
            case CurrencyKind.Euro:
            {
                writer.WriteValue("€");
                break;
            }
            default:
            {
                throw new JsonSerializationException("Invalid currency kind");
            }
        }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var value = reader.Value;
        if (value is not string str)
            throw new JsonSerializationException("Expected a currency string.");
        switch (str)
        {
            case "$": return CurrencyKind.USDollar;
            case "€": return CurrencyKind.Euro;
            default: throw new JsonSerializationException("Unknown currency type.");
        }
    }
}

public sealed class EnumAsStringsConverter<T> : JsonConverter where T : Enum
{
    public static readonly EnumAsStringsConverter<T> Instance = new EnumAsStringsConverter<T>();
    private static readonly Dictionary<T, string> ValueToName;
    private static readonly Dictionary<string, T> NameToValue;

    static EnumAsStringsConverter()
    {
        var values = typeof(T).GetEnumValues().Cast<T>();
        NameToValue = values.ToDictionary(v => Enum.GetName(typeof(T), v));
        ValueToName = NameToValue.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(T).Equals(objectType);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (!ValueToName.TryGetValue((T) value, out var name))
            throw new JsonSerializationException($"Invalid enum value: {value}.");
        writer.WriteValue(name);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var name = reader.Value;
        if (name is not string str)
            throw new JsonSerializationException("Expected an enum string.");

        if (!NameToValue.TryGetValue(str, out T enumValue))
            throw new JsonSerializationException($"Invalid enum value.");

        return enumValue;
    }
}

public class Program
{
    public static int Main(string[] args)
    {
        var basePath = (args.Length > 1) ? args[1] : Directory.GetCurrentDirectory();
        var dataPath = Path.Join(basePath, "data");
        if (!Directory.Exists(dataPath))
        {
            Console.WriteLine("No data folder found. Make sure you're running the program in the right directory.");
            return 1;
        }

        var assets = new AssetContext
        {
            DataPath = dataPath,
        };

        var carRegistry = new CarDependenciesRegistry();
        if (!carRegistry.Initialize(assets))
            return 1;

        var jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = OnlyFieldsResolver.Instance,
            Converters = new JsonConverter[]
            {
                HexStringJsonConverter.Instance,
                CurrencyKindConverter.Instance,
                EnumAsStringsConverter<EngineKind>.Instance,
            },
        };
        var jsonSerializer = JsonSerializer.Create(jsonSettings);

        const string carsFileName = "cars.json";
        if (!assets.TryReadJson(carsFileName, jsonSerializer, out List<CarModel> cars))
        {
            const int numCarsToGenerate = 5;
            cars = new List<CarModel>();
            var rng = new Random(80850);

            string[] firstNames = { "Steve", "John", "Maria", "Grace", };
            string[] lastNames = { "Smith", "Miller", "Martin", "Bower", };

            for (int i = 0; i < numCarsToGenerate; i++)
            {
                var car = new CarModel();
                car.EngineKind = (EngineKind) (rng.Next() % (int) EngineKind.Count);
                car.CountryId = rng.Next() % carRegistry.Countries.Length;
                car.ManufacturerId = rng.Next() % carRegistry.Manufacturers.Length;

                car.NumberplateText = ""
                    + rng.NextRandomCapitalLetter()
                    + rng.NextRandomCapitalLetter()
                    + (rng.Next() % 10000).ToString("D5");

                {
                    const int min = 0;
                    const int max = 100000;
                    car.KilometersTravelled = rng.Range(min, max);
                }

                {
                    const int min = 500;
                    const int max = 3000;
                    car.MassInKilograms = rng.Range(min, max);
                }

                {
                    var minDate = new DateTime(year: 1980, month: 1, day: 1);
                    var maxDate = new DateTime(year: 2022, month: 9, day: 16);
                    var diff = maxDate.Subtract(minDate);
                    car.ManufacturedDate = minDate.AddTicks(rng.NextInt64() % diff.Ticks);
                }

                car.NumDoors = rng.Range(1, 4) * 2;
                car.NumWheels = rng.Range(1, 4) * 2;
                car.Price.Value = (decimal) rng.Range(1_000, 100_000);
                car.Price.Kind = (CurrencyKind) (rng.Next() % (int) CurrencyKind.Count);
                
                if (rng.NextDouble() > 0.5f)
                {
                    car.Owner = new PersonNames
                    {
                        FirstName = firstNames[(rng.Next() % firstNames.Length)],
                        LastName = lastNames[(rng.Next() % lastNames.Length)],
                    };
                }

                {
                    var color = new RGBColor(unchecked((uint) rng.Next()));
                    color.Alpha = 255;
                    car.Color = color;
                }

                cars.Add(car);

            }
            assets.WriteJson(carsFileName, jsonSerializer, cars); 
        }


        return 0;
    }
}
