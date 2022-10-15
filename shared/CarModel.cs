using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CarApp.Assets;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CarApp.Model;

public class CarModel
{
    public string NumberplateText; // { get; set; }
    public DateTime ManufacturedDate; // { get; set; }
    public int ManufacturerId; // { get; set; }
    public Currency Price; // { get; set; }
    public int CountryId; // { get; set; }
    public bool HasOwner; // { get; set; }
    public PersonNames Owner; // { get; set; }
    public EngineKind EngineKind; // { get; set; }
    public float KilometersTravelled; // { get; set; }
    public int NumWheels; // { get; set; }
    public RGBAColor Color; // { get; set; }
    public CarModel Copy() => (CarModel) this.MemberwiseClone();
}

public record struct PersonNames
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
    _Count,
}

public record struct RGBAColor
{
    public uint HexValue;

    public RGBAColor(uint hexValue)
    {
        HexValue = hexValue;
    }

    public RGBAColor(int r, int g, int b, int a = 0xFF)
    {
        Debug.Assert(r >= 0 && r <= 0xFF);
        Debug.Assert(g >= 0 && g <= 0xFF);
        Debug.Assert(b >= 0 && b <= 0xFF);
        Debug.Assert(a >= 0 && a <= 0xFF);
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
            Debug.Assert(value >= 0 && value <= 0xFF);
            HexValue = unchecked((uint) (HexValue & ~(0xFF << 0)) | ((uint) value << 0));
        }
    }
    public int Green
    {
        readonly get => unchecked((int) ((HexValue >> 8) & 0xFF));
        set
        {
            Debug.Assert(value >= 0 && value <= 0xFF);
            HexValue = unchecked((uint) (HexValue & ~(0xFF << 8)) | ((uint) value << 8));
        }
    }
    public int Blue
    {
        readonly get => unchecked((int) ((HexValue >> 16) & 0xFF));
        set
        {
            Debug.Assert(value >= 0 && value <= 0xFF);
            HexValue = unchecked((uint) (HexValue & ~(0xFF << 16)) | ((uint) value << 16));
        }
    }
    public int Alpha
    {
        readonly get => unchecked((int) ((HexValue >> 24) & 0xFF));
        set
        {
            Debug.Assert(value >= 0 && value <= 0xFF);
            HexValue = unchecked((uint) (HexValue & ~(0xFF << 24)) | ((uint) value << 24));
        }
    }
}

public enum CurrencyKind
{
    USDollar,
    Euro,
    _Count,
}

public record struct Currency
{
    public decimal Value;
    public CurrencyKind Kind;
}


public class OnlyFieldsResolver : DefaultContractResolver
{
    public static readonly OnlyFieldsResolver Instance = new OnlyFieldsResolver();

    private OnlyFieldsResolver()
    {
        this.IgnoreSerializableAttribute = true;
        this.IgnoreSerializableInterface = true;
        this.IgnoreShouldSerializeMembers = true;
        this.SerializeCompilerGeneratedMembers = true;
    }

    protected override JsonStringContract CreateStringContract(Type objectType)
    {
        return base.CreateStringContract(objectType);
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
    private HexStringJsonConverter(){}

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

public static class CurrencyKindHelper
{
    public static readonly string[] Symbols;

    static CurrencyKindHelper()
    {
        Symbols = new string[(int) CurrencyKind._Count];
        Symbols[(int) CurrencyKind.Euro] = "€";
        Symbols[(int) CurrencyKind.USDollar] = "$";
        Debug.Assert(Symbols.All(s => s is not null));
    }

    public static string ToSymbol(this CurrencyKind kind)
    {
        return Symbols[(int) kind];
    }

    public static CurrencyKind? FromSymbol(string symbol)
    {
        int i = Array.IndexOf(Symbols, symbol);
        return i == -1 ? null : ((CurrencyKind) i);
    }
}

public sealed class CurrencyKindConverter : JsonConverter
{
    public static readonly CurrencyKindConverter Instance = new CurrencyKindConverter();
    private CurrencyKindConverter(){}

    public override bool CanConvert(Type objectType)
    {
        return typeof(CurrencyKind).Equals(objectType);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        string s = ((CurrencyKind) value).ToSymbol();
        if (s is null)
            throw new JsonSerializationException("Invalid currency kind");
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var value = reader.Value;
        if (value is not string str)
            throw new JsonSerializationException("Expected a currency string.");
        var kind = CurrencyKindHelper.FromSymbol(str);
        if (!kind.HasValue)
            throw new JsonSerializationException("Unknown currency type.");
        return kind.Value;
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
    private EnumAsStringsConverter(){}

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

public class CarDependenciesRegistry
{
    private const string _ManufacturersFileName = "car_manufacturers.txt";
    public string[] Manufacturers { get; private set; }

    private const string _CountriesFileName = "countries.txt";
    public string[] Countries { get; private set; }

    public bool Initialize(IAssetContext assets)
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

public static class CarModelUtils
{
    public static CarModel GenerateRandomModel(
        Random rng,
        int numManufacturers,
        int numCountries,
        ReadOnlySpan<string> firstNames,
        ReadOnlySpan<string> lastNames)
    {
        var car = new CarModel();
        car.EngineKind = (EngineKind) (rng.Next() % (int) EngineKind._Count);
        car.CountryId = rng.Next() % numCountries;
        car.ManufacturerId = rng.Next() % numManufacturers;

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
            var minDate = new DateTime(year: 1980, month: 1, day: 1);
            var maxDate = new DateTime(year: 2022, month: 9, day: 16);
            var diff = maxDate.Subtract(minDate);
            car.ManufacturedDate = minDate.AddTicks(rng.NextInt64() % diff.Ticks);
        }

        car.NumWheels = rng.Range(1, 4) * 2;
        car.Price = new Currency()
        {
            Value = (decimal) rng.Range(1_000, 100_000),
            Kind = (CurrencyKind) (rng.Next() % (int) CurrencyKind._Count),
        };
        
        if (rng.NextDouble() > 0.5f)
        {
            car.HasOwner = true;
            car.Owner = new PersonNames
            {
                FirstName = firstNames[(rng.Next() % firstNames.Length)],
                LastName = lastNames[(rng.Next() % lastNames.Length)],
            };
        }

        {
            var color = new RGBAColor(unchecked((uint) rng.Next()));
            color.Alpha = 255;
            car.Color = color;
        }

        return car;
    }

    public static readonly JsonConverter[] JsonConverters = new JsonConverter[]
    {
        HexStringJsonConverter.Instance,
        CurrencyKindConverter.Instance,
        EnumAsStringsConverter<EngineKind>.Instance,
    };
    
    public static JsonSerializerSettings GetSerializerSettings()
    {
        return new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = OnlyFieldsResolver.Instance,
            Converters = JsonConverters,
        };
    }
}