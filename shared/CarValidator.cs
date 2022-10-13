using System;
using System.Text.RegularExpressions;
using FluentValidation;

namespace CarApp.Model;

public interface ICarModel
{
    CarModel Model { get; set; }
    string NumberplateText { get; set; }
    DateTime ManufacturedDate { get; set; }
    int ManufacturerId { get; set; }
    Currency Price { get; set; }
    decimal Price_Value { get; set; }
    CurrencyKind Price_Kind { get; set; }
    int CountryId { get; set; }
    bool HasOwner { get; set; }
    PersonNames Owner { get; set; }
    string Owner_FirstName { get; set; }
    string Owner_LastName { get; set; }
    EngineKind EngineKind { get; set; }
    float KilometersTravelled { get; set; }
    int NumWheels { get; set; }
    RGBAColor Color { get; set; }
    byte Color_Red { get; set; }
    byte Color_Green { get; set; }
    byte Color_Blue { get; set; }
    byte Color_Alpha { get; set; }
}

public class CarValidator : AbstractValidator<ICarModel>
{
    private static readonly Regex _NameRegex = new Regex("^[A-Z][a-z]*$", RegexOptions.Compiled);

    public CarValidator(CarDependenciesRegistry registry)
    {
        RuleFor(x => x.CountryId).GreaterThanOrEqualTo(0).LessThan(_ => registry.Countries.Length);
        RuleFor(x => x.ManufacturerId).GreaterThanOrEqualTo(0).LessThan(_ => registry.Manufacturers.Length);
        RuleFor(x => x.Color_Alpha).Equal((byte) 0xff);
        RuleFor(x => x.KilometersTravelled).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price_Value).GreaterThan(0);
        RuleFor(x => x.ManufacturedDate)
            .GreaterThan(new DateTime(year: 1886, month: 1, day: 29))
            .LessThanOrEqualTo(_ => DateTime.Now);
        RuleFor(x => x.NumWheels).GreaterThan(0);

        RuleFor(x => x.Owner_FirstName).Must((model, x, context) =>
        {
            if (model.HasOwner)
                ValidateName(nameof(ICarModel.Owner_FirstName), "first name", x, context);
            return true;
        });
        RuleFor(x => x.Owner_LastName).Must((model, x, context) =>
        {
            if (model.HasOwner)
                ValidateName(nameof(ICarModel.Owner_LastName), "last name", x, context);
            return true;
        });

        static bool ValidateName(
            string prop,
            string propNormalCasing,
            string value,
            ValidationContext<ICarModel> context)
        {
            const string commonString = " does not match regex, must start with a capital letter.";
            if (value is null)
            {
                context.AddFailure(prop, "No " + propNormalCasing + ".");
                return false;
            }
            else if (!_NameRegex.IsMatch(value))
            {
                context.AddFailure(prop, "The " + propNormalCasing + commonString);
                return false;
            }
            return true;
        }
    }
}


