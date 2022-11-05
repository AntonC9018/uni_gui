using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using CarApp.Model;

namespace CarApp;

public class CarDataGridViewModel : INotifyPropertyChanged
{
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
}

public partial class CarDataGrid : UserControl
{
    private CarDataGridViewModel ViewModel
    {
        get => (CarDataGridViewModel) DataContext;
        set => DataContext = value;
    }
    
    public CarDataGrid(CarDatabase database)
    {
        ViewModel = new CarDataGridViewModel(database);
        InitializeComponent();
    }

    private ObservableCollection<CarModel> Cars => ViewModel.Database.Cars;

    // This lacks an undo system
    internal void AddNewCar(object sender, EventArgs e)
    {
        var cars = Cars;
        CarModel copy;
        if (Grid.SelectedIndex != -1)
            copy = cars[Grid.SelectedIndex].Copy();
        else if (cars.Count > 0)
            copy = cars[^1].Copy();
        else
            copy = new CarModel();
        cars.Add(copy);
        Grid.SelectedIndex = cars.Count - 1;
    }

    internal void RemoveCurrentCar(object sender, EventArgs e)
    {
        var cars = Cars;
        if (Grid.SelectedIndex != -1)
            cars.RemoveAt(Grid.SelectedIndex);
        else if (cars.Count > 0)
            cars.RemoveAt(cars.Count - 1);
    }
}
