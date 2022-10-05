using System;
using System.ComponentModel;
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
    private AbstractValidator<CarModel> _validator;
    private CarModel _model;
    public event PropertyChangedEventHandler PropertyChanged;

    public CarModelBindingSource(AbstractValidator<CarModel> validator)
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

    public string NumberplateText
    {
        get => Model.NumberplateText; 
        set
        {
            if (NumberplateText != value)
            {
                Model.NumberplateText = value; 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NumberplateText"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ManufacturedDate"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ManufacturerId"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Price"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Price_Value"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Price_Kind"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CountryId"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Owner"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Owner_FirstName"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Owner_LastName"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EngineKind"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("KilometersTravelled"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NumWheels"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color_Red"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color_Green"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color_Blue"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Color_Alpha"));
            }
        }
    }

    string IDataErrorInfo.Error => null;
    string IDataErrorInfo.this[string columnName]
    {
        get
        {
        }
    }
}

public class CarValidator : AbstractValidator<CarModel>
{
    private static readonly Regex _NameRegex = new Regex("^[A-Z][a-z]*$", RegexOptions.Compiled);

    public CarValidator(CarDependenciesRegistry registry)
    {
        RuleFor(x => x.CountryId).GreaterThanOrEqualTo(0).LessThan(_ => registry.Countries.Length);
        RuleFor(x => x.ManufacturerId).GreaterThanOrEqualTo(0).LessThan(_ => registry.Manufacturers.Length);
        RuleFor(x => x.Color.Alpha).Equal(255);
        RuleFor(x => x.KilometersTravelled).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price.Value).GreaterThan(0);
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
            if (!_NameRegex.IsMatch(names.FirstName))
            {
                context.AddFailure("The first name " + commonString);
                result = false;
            }
            if (!_NameRegex.IsMatch(names.LastName))
            {
                context.AddFailure("The last name " + commonString);
                result = false;
            }
            return result;
        });
    }
}


