using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace CarApp.Model;

public interface ICarDomain
{
    IList<string> Manufacturers { get; }
    IList<string> Countries { get; }
    IList<string> EngineKinds { get; }
    IList<string> CurrencyKinds { get; }
    AbstractValidator<ICarModel> Validator { get; }
}

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
