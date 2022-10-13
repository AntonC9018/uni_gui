using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace CarApp.Model;

public class CarDomain : ICarDomain
{
    // TODO: should be dynamic
    private CarDependenciesRegistry _registry;
    private AbstractValidator<ICarModel> _validator;

    public CarDomain(AbstractValidator<ICarModel> validator, CarDependenciesRegistry registry)
    {
        _validator = validator;
        _registry = registry;
        EngineKinds = typeof(EngineKind).GetPublicEnumNames();
        CurrencyKinds = CurrencyKindHelper.Symbols;
    }

    public IList<string> Manufacturers => _registry.Manufacturers;
    public IList<string> Countries => _registry.Countries;
    public IList<string> EngineKinds { get; }
    public IList<string> CurrencyKinds { get; }
    public AbstractValidator<ICarModel> Validator => _validator;
}

public interface ICarDomain
{
    IList<string> Manufacturers { get; }
    IList<string> Countries { get; }
    IList<string> EngineKinds { get; }
    IList<string> CurrencyKinds { get; }
    AbstractValidator<ICarModel> Validator { get; }
}

public class EnumViewModel<T>
{
    public EnumViewModel(string displayString, T value)
    {
        DisplayString = displayString;
        Value = value;
    }

    public string DisplayString { get; }
    public T Value { get; }
}

public static class EnumHelper
{
    public static string[] GetPublicEnumNames(this System.Type enumType)
    {
        return enumType.GetEnumNames().Where(n => !n.StartsWith("_")).ToArray();
    }

    public static IEnumerable<(T Value, string Name)> GetPublicEnumNameValues<T>()
    {
        return typeof(T)
            .GetEnumValues()
            .Cast<T>()
            .Distinct()
            .Select(value => (Value: value, Name: typeof(T).GetEnumName(value)))
            .Where(t => !t.Name.StartsWith("_"));
    }

    public static IEnumerable<T> GetPublicEnumValues<T>()
    {
        return GetPublicEnumNameValues<T>().Select(t => t.Value);
    }
}


