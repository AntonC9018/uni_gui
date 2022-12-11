using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using AvalonDock.Layout;
using CarApp.Model;

namespace CarApp;

public class CarDataGridViewModel : INotifyPropertyChanged
{
    private int _selectedIndex = -1;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex != value)
            {
                _selectedIndex = value;
                OnPropertyChanged(nameof(SelectedIndex));
                OnPropertyChanged(nameof(CanPopCarIntoNewDocument));
            }
        }
    }

    public CarDatabase Database { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    public CarDataGridViewModel(CarDatabase database)
    {
        Database = database;

        database.Cars.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is not null && e.NewItems.Count > 0
                || e.Action
                    is NotifyCollectionChangedAction.Reset
                    or NotifyCollectionChangedAction.Remove
                    && database.Cars.Count == 0)
            {
                OnPropertyChanged(nameof(CanRemoveCar));
            }
        };
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool CanAddCar => true;
    public bool CanRemoveCar
    {
        get => Database.Cars.Count > 0;
    }

    public bool CanPopCarIntoNewDocument
    {
        get => Database.Cars.Count > 0 && SelectedIndex != -1;
    }
}

public partial class CarDataGrid : UserControl
{
    private readonly LayoutDocumentPaneGroup _paneGroup;

    private CarDataGridViewModel ViewModel
    {
        get => (CarDataGridViewModel) DataContext;
        set => DataContext = value;
    }
    
    public CarDataGrid(CarDatabase database, LayoutDocumentPaneGroup paneGroup)
    {
        _paneGroup = paneGroup;

        ViewModel = new CarDataGridViewModel(database);
        InitializeComponent();
    }

    private ObservableCollection<CarModel> Cars => ViewModel.Database.Cars;

    // This lacks an undo system
    internal void AddNewCar(object sender, EventArgs e)
    {
        var cars = Cars;
        CarModel copy;
        if (ViewModel.SelectedIndex != -1)
            copy = cars[ViewModel.SelectedIndex].Copy();
        else if (cars.Count > 0)
            copy = cars[^1].Copy();
        else
            copy = new CarModel();
        cars.Add(copy);
        ViewModel.SelectedIndex = cars.Count - 1;
    }

    internal void RemoveCurrentCar(object sender, EventArgs e)
    {
        var cars = Cars;
        if (ViewModel.SelectedIndex != -1)
            cars.RemoveAt(ViewModel.SelectedIndex);
        else if (cars.Count > 0)
            cars.RemoveAt(cars.Count - 1);
    }

    internal void PopCarIntoNewDocument(object sender, EventArgs e)
    {
        int selectedIndex = ViewModel.SelectedIndex;
        if (selectedIndex == -1)
            return;

        var car = ViewModel.Database.CarBindings[selectedIndex];
        var documentPane = new LayoutDocumentPane();
        var document = new LayoutDocument
        {
            Title = car.NumberplateText,
            Content = new CarForm(car)
        };
        documentPane.Children.Add(document);
        _paneGroup.Children.Add(documentPane);
        document.IsActive = true;
    }
}
