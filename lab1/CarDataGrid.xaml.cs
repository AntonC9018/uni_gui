using System.Windows;

namespace CarApp;

public partial class CarDataGrid : Window
{
    public CarDataGrid(CarDatabase database)
    {
        DataContext = database;
        InitializeComponent();
    }
}
