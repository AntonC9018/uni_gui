using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluentValidation;

namespace CarApp.Model;

public interface ICarViewDomain : ICarDomain
{
    IReadOnlyList<string> EngineKinds { get; }
    IReadOnlyList<string> CurrencyKinds { get; }
    AbstractValidator<ICarModel> Validator { get; }
}

public class DynamicCarDomain : ICarDomain
{
    public ObservableCollection<string> Manufacturers { get; }
    public ObservableCollection<string> Countries { get; }

    public DynamicCarDomain(ObservableCollection<string> manufacturers, ObservableCollection<string> countries)
    {
        Manufacturers = manufacturers;
        Countries = countries;
    }

    IList<string> ICarDomain.Manufacturers => Manufacturers;
    IList<string> ICarDomain.Countries => Countries;
}

public class CarViewDomain : ICarViewDomain
{
    // TODO: should be dynamic
    private ICarDomain _registry;
    private AbstractValidator<ICarModel> _validator;

    public CarViewDomain(AbstractValidator<ICarModel> validator, ICarDomain registry)
    {
        _validator = validator;
        _registry = registry;
        EngineKinds = typeof(EngineKind).GetPublicEnumNames();
        CurrencyKinds = CurrencyKindHelper.Symbols;
    }

    public IList<string> Manufacturers => _registry.Manufacturers;
    public IList<string> Countries => _registry.Countries;
    public IReadOnlyList<string> EngineKinds { get; }
    public IReadOnlyList<string> CurrencyKinds { get; }
    public AbstractValidator<ICarModel> Validator => _validator;
}
