using System;
using System.Windows;
using System.Windows.Controls;

namespace CarApp;

public static partial class Properties
{
    private static readonly DependencyProperty _TemplateName = DependencyProperty.RegisterAttached(
        nameof(TemplateName), ownerType: typeof(DataGridColumn), propertyType: typeof(string));
    public static DependencyProperty TemplateName => _TemplateName;

    public static string GetTemplateName(this DataGridColumn target)
    {
        return (string) target.GetValue(TemplateName);
    }

    public static void SetTemplateName(this DataGridColumn target, string value)
    {
        target.SetValue(TemplateName, value);
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