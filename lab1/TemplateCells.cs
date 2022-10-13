using System;
using System.Windows;
using System.Windows.Controls;

namespace CarApp;

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