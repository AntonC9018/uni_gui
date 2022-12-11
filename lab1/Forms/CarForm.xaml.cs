using System.Windows.Controls;

namespace CarApp;

public partial class CarForm : UserControl
{
    public CarForm(CarViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}