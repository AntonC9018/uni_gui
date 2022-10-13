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

public partial class Form1 : Window
{
    private CarDatabase _database;

    public Form1(CarDatabase database)
    {
        _database = database;
        
        DataContext = database;
        InitializeComponent();
        // InitializeComponent2();

        var dispatcherTimer = new DispatcherTimer();
        dispatcherTimer.Tick += (_, _) =>
        {
            database.CarBindings[0].Price_Value += 10;
            database.CarBindings[0].Owner_FirstName = "anton";
            database.CarBindings[0].Owner_LastName += "a";
        };
        dispatcherTimer.Interval = TimeSpan.FromMilliseconds(3000);
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
