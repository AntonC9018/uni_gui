using System.Windows.Forms;
using System;
using System.Collections.Generic;
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

public partial class Form1 : Form
{
    private CarDatabase _database;

    public Form1(CarDatabase database)
    {
        _database = database;
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        var comboBox = new ComboBox();
        Controls.Add(comboBox);

        var listBox = new ListBox();
        Controls.Add(listBox);

        var textBox = new TextBox();
        Controls.Add(textBox);

        // bind to List<Foo>
        BindingSource comboBoxBindingSource = new BindingSource();
        comboBoxBindingSource.DataSource = _database.Cars;
        // use this binding source in comboBox1
        // for display use FooName
        comboBox.DataSource = comboBoxBindingSource;
        comboBox.DisplayMember = "FooName";

        // bind to comboBox1's SelectedValue
        // it will point to the Foo selected in combobox
        BindingSource listBoxBindingSource = new BindingSource();
        listBoxBindingSource.DataSource = comboBox;
        listBoxBindingSource.DataMember = "SelectedValue";
        // use this binding source in listBox1
        // for display use BarName
        listBox.DataSource = listBoxBindingSource;
        listBox.DisplayMember = "BarName";

        // bind to comboBox'1s SelectedValue (reusing listBoxBindingSource)
        // and set Text to value of BarDesc
        textBox.DataBindings.Add("Text", listBoxBindingSource, "BarDesc");
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
