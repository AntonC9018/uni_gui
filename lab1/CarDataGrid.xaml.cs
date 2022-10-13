using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using CarApp.Model;

namespace CarApp;

public class CarDatabase
{
    public ObservableCollection<CarModel> Cars { get; }
    public ICarDomain Domain { get; }
    public ObservableViewModelCollection<CarViewModel, CarModel> CarBindings { get; }

    public CarDatabase(ObservableCollection<CarModel> cars, ICarDomain domain)
    {
        Cars = cars;
        Domain = domain;
        CarBindings = new(cars, c => new CarViewModel(Domain, c));
    }
}

public class GenericProperty<TTarget, TProperty, TSelf> where TTarget : DependencyObject
{
    public static readonly DependencyProperty Property = DependencyProperty.RegisterAttached(
        typeof(TSelf).Name, ownerType: typeof(TTarget), propertyType: typeof(TProperty));

    public static TProperty Get(TTarget target)
    {
        return (TProperty) target.GetValue(Property);
    }

    public static void Set(TTarget target, TProperty value)
    {
        target.SetValue(Property, value);
    }
}

public static partial class Properties
{
    private static DependencyProperty CreateProperty<T>(string name, PropertyMetadata metadata = null)
    {
        return DependencyProperty.RegisterAttached(
            name,
            ownerType: typeof(Properties),
            propertyType: typeof(T),
            defaultMetadata: metadata);
    }

    private static readonly DependencyProperty TemplateBindingProperty = CreateProperty<Binding>("TemplateBinding");

    public static Binding GetTemplateBinding(DataGridColumn target)
    {
        return (Binding) target.GetValue(TemplateBindingProperty);
    }

    public static void SetTemplateBinding(DataGridColumn target, Binding value)
    {
        target.SetValue(TemplateBindingProperty, value);
    }
}

public partial class CarDataGrid : Window
{
    public CarDataGrid(CarDatabase database)
    {
        DataContext = database;
        InitializeComponent();
    }
}
