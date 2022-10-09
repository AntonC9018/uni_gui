using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using CarApp.Model;

namespace CarApp;

public class DirectGet<T> : IGet<T> where T : class
{
    public T Value { get; }
    public DirectGet(T value) { Value = value; }
}

public class CarDatabase
{
    public readonly ObservableCollection<CarModel> Cars;
    public readonly CarDependenciesRegistry Registry;
    public readonly ObservableCollection<CarModelBindingSource> CarBindings;
    private readonly CarValidator _validator;

    public CarDatabase(ObservableCollection<CarModel> cars, CarDependenciesRegistry registry)
    {
        Cars = cars;
        Registry = registry;

        var validator = new CarValidator(registry);
        _validator = validator;
        CarBindings = new(cars.Select(c => new CarModelBindingSource(validator, c)));

        Cars.CollectionChanged += (sender, e) =>
        {
            Debug.Assert(sender is not null);
            var collection = (ObservableCollection<CarModel>) sender;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    CarBindings.Add(new CarModelBindingSource(_validator, collection[^1]));
                    break;
                }
                case NotifyCollectionChangedAction.Move:
                {
                    CarBindings.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                }
                // case NotifyCollectionChangedAction.Replace:
                // {
                //     Debug.Fail("Not supported");
                //     break;
                // }
                // case NotifyCollectionChangedAction.Reset:
                // {
                //     Debug.Fail("Not supported");
                //     break;
                // }
                case NotifyCollectionChangedAction.Remove:
                {
                    CarBindings.RemoveAt(e.OldStartingIndex);
                    break;
                }
                default:
                {
                    Debug.Fail("Unsupported case " + e.Action);
                    break;
                }
            }
        };

        CarBindings.CollectionChanged += (sender, e) =>
        {
            Debug.Assert(sender is not null);
            var collection = (ObservableCollection<CarModelBindingSource>) sender;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    // Should have been added manually at this point.
                    break;
                }
                case NotifyCollectionChangedAction.Move:
                {
                    Cars.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    Cars.RemoveAt(e.OldStartingIndex);
                    break;
                }
                default:
                {
                    Debug.Fail("Unsupported case " + e.Action);
                    break;
                }
            }
        };
    }
}

public static class PropertyChangedEventHandlerExtensions
{
}

public partial class Form1 : Window
{
    private CarDatabase _database;

    public Form1(CarDatabase database)
    {
        _database = database;
        InitializeComponent();
        // InitializeComponent2();
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
        var carModelBindingSource = new CarModelBindingSource(validator);
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
