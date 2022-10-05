using System;
using System.Collections.Generic;
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
    public readonly List<CarModel> Cars;
    public readonly CarDependenciesRegistry Registry;

    public CarDatabase(List<CarModel> cars, CarDependenciesRegistry registry)
    {
        Cars = cars;
        Registry = registry;
    }
}

public partial class Form1 : Window
{
    private CarDatabase _database;

    public Form1(CarDatabase database)
    {
        _database = database;
        InitializeComponent();
        InitializeComponent2();
    }
    
    private void InitializeComponent2()
    {
        var container = (Panel) this.Content;

        var text = new TextBlock();
        text.Text = "Hello world";
        container.Children.Add(text);

        var textBox = new TextBox();
        textBox.Text = "Some default text";
        Grid.SetColumn(textBox, 1);
        container.Children.Add(textBox);

        var carModel = new CarModel();
        var carModelBindingSource = new CarModelBindingSource();
        carModelBindingSource.Model = carModel;

        carModelBindingSource.NumberplateText = "Hello";

        var binding = new Binding();
        binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
        binding.Mode = BindingMode.TwoWay;
        binding.Path = new PropertyPath(nameof(carModelBindingSource.NumberplateText));
        binding.Source = carModelBindingSource;
        // binding.Converter
        BindingOperations.SetBinding(textBox, TextBox.TextProperty, binding);

        var dispatcherTimer = new DispatcherTimer();
        dispatcherTimer.Tick += (_, e) =>
        {
            carModelBindingSource.NumberplateText += '1';
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
