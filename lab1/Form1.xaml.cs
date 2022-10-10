using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using CarApp.Model;

namespace CarApp;

public class CarDatabase
{
    public readonly ObservableCollection<CarModel> Cars;
    public readonly CarDependenciesRegistry Registry;
    public readonly ObservableViewModelCollection<CarViewModel, CarModel> CarBindings;

    public CarDatabase(ObservableCollection<CarModel> cars, CarDependenciesRegistry registry)
    {
        Cars = cars;
        Registry = registry;

        var validator = new CarValidator(registry);
        CarBindings = new(cars, c => new CarViewModel(validator, c));
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
    private static readonly DependencyProperty _TemplateBinding = DependencyProperty.RegisterAttached(
        nameof(TemplateBinding), ownerType: typeof(DataGridColumn), propertyType: typeof(Binding));
    public static DependencyProperty TemplateBinding => _TemplateBinding;

    public static Binding GetTemplateBinding(DataGridColumn target)
    {
        return (Binding) target.GetValue(TemplateBinding);
    }

    public static void SetTemplateBinding(DataGridColumn target, Binding value)
    {
        target.SetValue(TemplateBinding, value);
    }
}

public partial class Form1 : Window
{
    private CarDatabase _database;

    public Form1(CarDatabase database)
    {
        _ = Properties.TemplateBinding;
        _database = database;
        InitializeComponent();
        // InitializeComponent2();

        CarDataGrid.ItemsSource = database.CarBindings;
    }
    
    private void InitializeComponent2()
    {
        var container = (Panel) this.Content;

        var text = new TextBlock();
        text.Text = "Hello world";
        Grid.SetColumn(text, 0);
        container.Children.Add(text);

        var validationTemplate = (ControlTemplate) this.Resources["validationTemplate"];
        Debug.Assert(validationTemplate is not null);

        var numberplateTextBox = new TextBox();
        numberplateTextBox.Text = "Some default text";
        Grid.SetColumn(numberplateTextBox, 1);
        Grid.SetRow(numberplateTextBox, 0);
        container.Children.Add(numberplateTextBox);
        Validation.SetErrorTemplate(numberplateTextBox, validationTemplate);

        var firstNameTextBox = new TextBox();
        firstNameTextBox.Text = "Some default text";
        Grid.SetColumn(firstNameTextBox, 1);
        Grid.SetRow(firstNameTextBox, 1);
        container.Children.Add(firstNameTextBox);
        Validation.SetErrorTemplate(firstNameTextBox, validationTemplate);

        var carModel = new CarModel();
        var validator = new CarValidator(_database.Registry);
        var carModelBindingSource = new CarViewModel(validator);
        carModelBindingSource.Model = carModel;

        carModelBindingSource.NumberplateText = "Hello";
        carModelBindingSource.Owner = new PersonNames();

        Binding CreateBinding(string propPath)
        {
            var binding = new Binding(propPath);
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            binding.Mode = BindingMode.TwoWay;
            binding.Source = carModelBindingSource;
            binding.ValidatesOnDataErrors = true;
            binding.NotifyOnValidationError = true;
            return binding;
        }
        {
            var binding = CreateBinding(nameof(carModelBindingSource.NumberplateText));
            BindingOperations.SetBinding(numberplateTextBox, TextBox.TextProperty, binding);
        }
        {
            var binding = CreateBinding(nameof(carModelBindingSource.Owner_FirstName));
            BindingOperations.SetBinding(firstNameTextBox, TextBox.TextProperty, binding);
        }

        var dispatcherTimer = new DispatcherTimer();
        dispatcherTimer.Tick += (_, _) =>
        {
            var t = this;
        };
        dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1000);
        dispatcherTimer.Start();
    }

#if !VISUAL_STUDIO_DESIGNER && false
    private void InitializeComponent()
    {
        var panel = new FlowLayoutPanel();
        panel.FlowDirection = FlowDirection.TopDown;
        panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        panel.AutoSize = true;
        Controls.Add(panel);

        var controls = panel.Controls;

        var domainUpDown = new DomainUpDown();
        domainUpDown.Sorted = true;
        domainUpDown.AutoSize = true;
        controls.Add(domainUpDown);

        var selectedItemIndexLabel = new Label();
        selectedItemIndexLabel.AutoSize = true;
        controls.Add(selectedItemIndexLabel);

        var submitButtom = new Button();
        submitButtom.AutoSize = true;
        submitButtom.Text = "Submit item";
        controls.Add(submitButtom);

        var textBox = new TextBox();
        textBox.AutoSize = true;
        textBox.PlaceholderText = "Enter item text";
        controls.Add(textBox);

        var isSortedCheckBox = new CheckBox();
        isSortedCheckBox.AutoSize = true;
        isSortedCheckBox.Text = "Sorted?";
        isSortedCheckBox.Checked = domainUpDown.Sorted;
        controls.Add(isSortedCheckBox);

        domainUpDown.SelectedItemChanged += (_, _) =>
        {
            selectedItemIndexLabel.Text = "Currently selected index: " + domainUpDown.SelectedIndex.ToString();
        };

        EventHandler submitHandler = (_, _) =>
        {
            var text = textBox.Text.Trim();
            if (text.Length == 0)
                return;
            Console.WriteLine("Adding item " + text);

            domainUpDown.Items.Add(text);
            domainUpDown.SelectedItem = text;

            textBox.ResetText();
        };

        textBox.Enter += submitHandler;
        submitButtom.Click += submitHandler;

        isSortedCheckBox.Click += (_, _) =>
        {
            domainUpDown.Sorted = isSortedCheckBox.Checked;
        };
    }
#endif
}
