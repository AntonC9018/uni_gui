using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace CarApp.Model;

// Hurray, I love boilerplate so much (I don't).
public sealed class CarViewModel : INotifyPropertyChanged, IDataErrorInfo, ICarModel
{
    private ICarViewDomain _domain;
    private CarModel _model;
    public event PropertyChangedEventHandler PropertyChanged;

    public CarViewModel(ICarViewDomain domain)
    {
        _domain = domain;
    }

    public CarViewModel(ICarViewDomain domain, CarModel model)
    {
        _domain = domain;
        _model = model;
    }

    // Used to diplay comboboxes in the ui.
    public ICarViewDomain Domain => _domain;

    public CarModel Model
    {
        get => _model;
        set
        {
            _model = value;
            OnPropertyChanged(null);
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
                OnPropertyChanged(nameof(NumberplateText));
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
                OnPropertyChanged(nameof(ManufacturedDate));
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
                OnPropertyChanged(nameof(ManufacturerId));
            }
        }
    }
    public Currency Price
    {
        get => Model.Price;
        set
        {
            bool areDifferent = false;
            if (Model.Price.Kind != value.Kind)
            {
                Model.Price.Kind = value.Kind;
                areDifferent = true;
                OnPropertyChanged(nameof(Price.Kind));
            }
            if (Model.Price.Value != value.Value)
            {
                Model.Price.Value = value.Value;
                areDifferent = true;
                OnPropertyChanged(nameof(Price.Value));
            }
            if (areDifferent)
                OnPropertyChanged(nameof(Price));
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
                OnPropertyChanged(nameof(Price));
                OnPropertyChanged(nameof(Price_Value));
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
                OnPropertyChanged(nameof(Price));
                OnPropertyChanged(nameof(Price_Kind));

                // see explanation below.
                OnPropertyChanged(nameof(Price_KindIndex));
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
                OnPropertyChanged(nameof(CountryId));
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
                OnPropertyChanged(nameof(HasOwner));

                // Property interdependencies are not handled automatically.
                // This hack forces the system to update validation ui of the dependent props.
                OnPropertyChanged(nameof(Owner));
                OnPropertyChanged(nameof(Owner_FirstName));
                OnPropertyChanged(nameof(Owner_LastName));
            }
        }
    }
    public PersonNames Owner
    {
        get => Model.Owner;
        set
        {
            bool areDifferent = false;
            if (Owner_FirstName != value.FirstName)
            {
                Model.Owner.FirstName = value.FirstName;
                areDifferent = true;
                OnPropertyChanged(nameof(Owner_FirstName));
            }
            if (Owner_LastName != value.LastName)
            {
                Model.Owner.LastName = value.LastName;
                areDifferent = true;
                OnPropertyChanged(nameof(Owner_LastName));
            }
            if (areDifferent)
                OnPropertyChanged(nameof(Owner));
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
                OnPropertyChanged(nameof(Owner));
                OnPropertyChanged(nameof(Owner_FirstName));
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
                OnPropertyChanged(nameof(Owner));
                OnPropertyChanged(nameof(Owner_LastName));
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
                OnPropertyChanged(nameof(EngineKind));
                
                // see explanation below.
                OnPropertyChanged(nameof(EngineKindIndex));
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
                OnPropertyChanged(nameof(KilometersTravelled));
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
                OnPropertyChanged(nameof(NumWheels));
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
                OnPropertyChanged(nameof(Color));
                OnPropertyChanged(nameof(Windows_Media_Color));
                OnPropertyChanged(nameof(Color_Red));
                OnPropertyChanged(nameof(Color_Green));
                OnPropertyChanged(nameof(Color_Blue));
                OnPropertyChanged(nameof(Color_Alpha));
            }
        }
    }

    public System.Windows.Media.Color Windows_Media_Color
    {
        get
        {
            var c = Model.Color;
            return System.Windows.Media.Color.FromArgb(
                (byte) c.Alpha, (byte) c.Red, (byte) c.Green, (byte) c.Blue);
        }
        set
        {
            Color = new RGBAColor(value.R, value.G, value.B, value.A);
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
                
                OnPropertyChanged(nameof(Color));
                OnPropertyChanged(nameof(Windows_Media_Color));
                OnPropertyChanged(nameof(Color_Red));
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
                
                OnPropertyChanged(nameof(Color));
                OnPropertyChanged(nameof(Windows_Media_Color));
                OnPropertyChanged(nameof(Color_Green));
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
                
                OnPropertyChanged(nameof(Color));
                OnPropertyChanged(nameof(Windows_Media_Color));
                OnPropertyChanged(nameof(Color_Blue));
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

                OnPropertyChanged(nameof(Color));
                OnPropertyChanged(nameof(Windows_Media_Color));
                OnPropertyChanged(nameof(Color_Alpha));
            }
        }
    }

    private ValidationResult _cachedValidationResult;
    
    // Prevent property evaluation by the debugger
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
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


    private static readonly Dictionary<string, string[]> _LazyPropertyMap = new();

    public string this[string columnName]
    {
        get
        {
            static string[] GetActualColumnName(string columnName)
            {
                if (_LazyPropertyMap.TryGetValue(columnName, out var v))
                    return v;

                return _LazyPropertyMap[columnName] = columnName switch
                {
                    nameof(Color) => new[]
                    {
                        nameof(Color_Alpha),
                        nameof(Color_Blue),
                        nameof(Color_Red),
                        nameof(Color_Green),
                    },
                    nameof(Owner) => new[]
                    {
                        nameof(Owner_FirstName),
                        nameof(Owner_LastName),
                    },
                    nameof(Price) => new[]
                    {
                        nameof(Price_Kind),
                        nameof(Price_Value),
                    },
                    nameof(Price_KindIndex) => new[] { nameof(Price_Kind) },
                    nameof(EngineKindIndex) => new[] { nameof(EngineKind) },
                    nameof(Windows_Media_Color) => GetActualColumnName(nameof(Color)),
                    _ => new[] { columnName },
                };
            }

            string[] actualColumnNames = GetActualColumnName(columnName);
            var errors = ValidationResult.Errors.Where(p => Array.IndexOf(actualColumnNames, p.PropertyName) != -1);
            return string.Join(Environment.NewLine, errors.Select(e => e.ErrorMessage));
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
