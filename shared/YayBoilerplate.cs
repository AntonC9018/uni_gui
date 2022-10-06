using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;

namespace CarApp.Model;

public interface IGet<T> where T : class
{
    T Value { get; }
}

// Hurray, I love boilerplate so much (I don't).
public class CarModelBindingSource : System.ComponentModel.INotifyPropertyChanged, IDataErrorInfo
{
    private AbstractValidator<CarModelBindingSource> _validator;
    private CarModel _model;
    public event PropertyChangedEventHandler PropertyChanged;

    public CarModelBindingSource(AbstractValidator<CarModelBindingSource> validator)
    {
        _validator = validator;
    }
    
    public CarModel Model
    {
        get => _model;
        set
        {
            _model = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        _cachedValidationResult = null;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string NumberplateText
    {
        get => Model.NumberplateText; 
        set
        {
            if (NumberplateText != value)
            {
                Model.NumberplateText = value; 
                OnPropertyChanged("NumberplateText");
            }
        }
    }
    public DateTime ManufacturedDate
    {
        get => Model.ManufacturedDate; 
        set
        {
            if (ManufacturedDate != value)
            {
                Model.ManufacturedDate = value; 
                OnPropertyChanged("ManufacturedDate");
            }
        }
    }
    public int ManufacturerId
    {
        get => Model.ManufacturerId; 
        set
        {
            if (ManufacturerId != value)
            {
                Model.ManufacturerId = value; 
                OnPropertyChanged("ManufacturerId");
            }
        }
    }
    public Currency Price
    {
        get => Model.Price; 
        set
        {
            if (Price != value)
            {
                Model.Price = value; 
                OnPropertyChanged("Price");
            }
        }
    }
    public decimal Price_Value
    {
        get => Model.Price.Value; 
        set
        {
            if (Price_Value != value)
            {
                var p = Model.Price;
                p.Value = value;
                Model.Price = p;
                OnPropertyChanged("Price_Value");
            }
        }
    }
    public CurrencyKind Price_Kind
    {
        get => Model.Price.Kind; 
        set
        {
            if (Price_Kind != value)
            {
                var p = Model.Price;
                p.Kind = value;
                Model.Price = p;
                OnPropertyChanged("Price_Kind");
            }
        }
    }
    public int CountryId
    {
        get => Model.CountryId; 
        set
        {
            if (CountryId != value)
            {
                Model.CountryId = value; 
                OnPropertyChanged("CountryId");
            }
        }
    }
    public PersonNames? Owner
    {
        get => Model.Owner; 
        set
        {
            if (Owner != value)
            {
                Model.Owner = value; 
                OnPropertyChanged("Owner");
            }
        }
    }
    public string Owner_FirstName
    {
        get => Model.Owner.Value.FirstName; 
        set
        {
            if (Owner_FirstName != value)
            {
                var p = Model.Owner.Value;
                p.FirstName = value;
                Model.Owner = p;
                OnPropertyChanged("Owner_FirstName");
            }
        }
    }
    public string Owner_LastName
    {
        get => Model.Owner.Value.LastName; 
        set
        {
            if (Owner_LastName != value)
            {
                var p = Model.Owner.Value;
                p.LastName = value;
                Model.Owner = p;
                OnPropertyChanged("Owner_LastName");
            }
        }
    }
    public EngineKind EngineKind
    {
        get => Model.EngineKind; 
        set
        {
            if (EngineKind != value)
            {
                Model.EngineKind = value; 
                OnPropertyChanged("EngineKind");
            }
        }
    }
    public float KilometersTravelled
    {
        get => Model.KilometersTravelled; 
        set
        {
            if (KilometersTravelled != value)
            {
                Model.KilometersTravelled = value; 
                OnPropertyChanged("KilometersTravelled");
            }
        }
    }
    public int NumWheels
    {
        get => Model.NumWheels; 
        set
        {
            if (NumWheels != value)
            {
                Model.NumWheels = value; 
                OnPropertyChanged("NumWheels");
            }
        }
    }
    public RGBAColor Color
    {
        get => Model.Color; 
        set
        {
            if (Color != value)
            {
                Model.Color = value; 
                OnPropertyChanged("Color");
            }
        }
    }

    public byte Color_Red
    {
        get => (byte) Model.Color.Red; 
        set
        {
            if (Color.Red != value)
            {
                Model.Color.Red = value; 
                OnPropertyChanged("Color_Red");
            }
        }
    }

    public byte Color_Green
    {
        get => (byte) Model.Color.Green; 
        set
        {
            if (Color.Green != value)
            {
                Model.Color.Green = value; 
                OnPropertyChanged("Color_Green");
            }
        }
    }

    public byte Color_Blue
    {
        get => (byte) Model.Color.Blue; 
        set
        {
            if (Color.Blue != value)
            {
                Model.Color.Blue = value; 
                OnPropertyChanged("Color_Blue");
            }
        }
    }

    public byte Color_Alpha
    {
        get => (byte) Model.Color.Alpha; 
        set
        {
            if (Color.Alpha != value)
            {
                Model.Color.Alpha = value; 
                OnPropertyChanged("Color_Alpha");
            }
        }
    }

    private ValidationResult _cachedValidationResult;
    private ValidationResult ValidationResult => _cachedValidationResult ??= _validator.Validate(this);

    string IDataErrorInfo.Error
    {
        get
        {
            Console.WriteLine("Getting all errors");
            var r = ValidationResult;
            if (r.Errors.Count == 0)
                return null;
            var errors = string.Join(Environment.NewLine, r.Errors.Select(x => x.ErrorMessage));
            return errors;
        }
    }
    public string this[string columnName]
    {
        get
        {
            Console.WriteLine("Getting errors for " + columnName);
            var firstOrDefault = ValidationResult.Errors.FirstOrDefault(p => p.PropertyName == columnName);
            Console.WriteLine(firstOrDefault);
            return firstOrDefault?.ErrorMessage;
        }
    }
}


// Might want to pass this thing an interface with all of the properties instead.
// But then the CarModel would also have to implement those... It sucks either way...
public class CarValidator : AbstractValidator<CarModelBindingSource>
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
        RuleFor(x => x.Owner).Must((model, x, context) =>
        {
            if (!x.HasValue)
                return true;

            var names = x.Value;

            bool result = true;
            const string commonString = " does not match regex, must start with a capital letter.";

            void Validate(string prop, string propNormalCasing, string value)
            {
                if (value is null)
                {
                    context.AddFailure(prop, "No " + propNormalCasing + ".");
                    result = false;
                }
                else if (!_NameRegex.IsMatch(value))
                {
                    context.AddFailure(prop, "The " + propNormalCasing + commonString);
                    result = false;
                }
            }
            Validate("Owner_FirstName", "first name", names.FirstName);
            Validate("Owner_LastName", "last name", names.LastName);
            return result;
        });
    }
}


