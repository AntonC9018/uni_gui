using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace CarApp.Model;

public interface IGet<T> where T : class
{
    T Value { get; }
}


// Hurray, I love boilerplate so much (I don't).
public class CarViewModel : INotifyPropertyChanged, IDataErrorInfo, ICarModel
{
    private ICarDomain _domain;
    private CarModel _model;
    public event PropertyChangedEventHandler PropertyChanged;

    public CarViewModel(ICarDomain domain)
    {
        _domain = domain;
    }

    public CarViewModel(ICarDomain domain, CarModel model)
    {
        _domain = domain;
        _model = model;
    }

    // Used to diplay comboboxes in the ui.
    public ICarDomain Domain => _domain;

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

                // see explanation below.
                OnPropertyChanged("Price_KindIndex");
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
    public bool HasOwner
    {
        get => Model.HasOwner;
        set
        {
            if (HasOwner != value)
            {
                Model.HasOwner = value;
                OnPropertyChanged("HasOwner");
            }
        }
    }
    public PersonNames Owner
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
        get => Model.Owner.FirstName;
        set
        {
            if (Owner_FirstName != value)
            {
                Model.Owner.FirstName = value;
                OnPropertyChanged("Owner_FirstName");
            }
        }
    }
    public string Owner_LastName
    {
        get => Model.Owner.LastName;
        set
        {
            if (Owner_LastName != value)
            {
                Model.Owner.LastName = value;
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
                
                // see explanation below.
                OnPropertyChanged("EngineKindIndex");
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

    public Color ExtColor
    {
        get
        {
            var c = Model.Color;
            return System.Drawing.Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);
        }
        set
        {
            var c = ExtColor;
            if (c != value)
            {
                Model.Color = new RGBAColor(c.R, c.G, c.B, c.A);
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
    private ValidationResult ValidationResult => _cachedValidationResult ??= _domain.Validator.Validate(this);

    string IDataErrorInfo.Error
    {
        get
        {
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
            var errors = ValidationResult.Errors.Where(p => p.PropertyName == columnName);
            return string.Join(Environment.NewLine, errors);
        }
    }

    
    // I'm offering help with assigning the enum as an int rather than as enum.
    // This is to be able to set custom display strings for the enum fields,
    // plus it's faster in case of two-way bindings.
    public int Price_KindIndex
    {
        get => (int) Model.Price.Kind;
        set => Price_Kind = (CurrencyKind) value;
    }
    
    public int EngineKindIndex
    {
        get => (int) Model.EngineKind;
        set => EngineKind = (EngineKind) value;
    }
}
