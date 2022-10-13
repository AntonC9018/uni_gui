using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace CarApp;


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
}

public static partial class Properties
{
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

public static partial class Properties
{
    public static readonly DependencyProperty TemplateNameProperty = CreateProperty<string>("TemplateName");

    public static string GetTemplateName(this DataGridColumn target)
    {
        return (string) target.GetValue(TemplateNameProperty);
    }

    public static void SetTemplateName(this DataGridColumn target, string value)
    {
        target.SetValue(TemplateNameProperty, value);
    }
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

public class MyDataTemplateSelector : DataTemplateSelector
{
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        var column = (DataGridColumn) container;

        var templateName = column.GetTemplateName();
        if (templateName is null)
            return null;

        if (container is not FrameworkElement frameworkElement)
        {
            Console.WriteLine(container + " is not a framework element?");
            return null;
        }

        return (DataTemplate) frameworkElement.FindResource(templateName);
    }
}

// https://iyalovoi.wordpress.com/2009/08/21/wpf-datagrid-tabbing-from-cell-to-cell-does-not-set-focus-on-control/
public static partial class Properties
{
    public static readonly DependencyProperty FocusProperty = CreateProperty<bool>(
        "Focus",
        new PropertyMetadata(defaultValue: false, propertyChangedCallback: (sender, e) =>
        {
            if ((bool) e.NewValue)
                (sender as UIElement)?.Focus();
        }));

    public static bool GetFocus(DependencyObject d)
    {
        return (bool) d.GetValue(FocusProperty);
    }

    public static void SetFocus(DependencyObject d, bool value)
    {
        d.SetValue(FocusProperty, value);
    }
}

[MarkupExtensionReturnType(typeof(Binding))]
public class ReactiveBinding : MarkupExtension
{
    public string Path { get; set; }
    public string StringFormat { get; set; }

    public ReactiveBinding(string path)
    {
        Path = path;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding(Path)
        {
            Mode = BindingMode.TwoWay,
            ValidatesOnDataErrors = true,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            StringFormat = StringFormat,
        };
    }
}

// I want to have _Count field in my enums, which is why these functions exist at all.
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
