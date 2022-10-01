using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using CarApp.Model;

namespace CarApp;


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
        text.Foreground = new SolidColorBrush
        {
            Color = Color.FromRgb(0xff, 0xff, 0xff),
        };
        container.Children.Add(text);

        var textBox = new TextBox();
        container.Children.Add(textBox);

        container.Background = new SolidColorBrush
        {
            Color = Color.FromRgb(0, 0, 0),
        };
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
