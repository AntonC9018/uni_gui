using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using Newtonsoft.Json;
using CarApp.Model;
using CarApp.Assets;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;

namespace CarApp;

public class CarDatabase
{
    public ObservableCollection<CarModel> Cars { get; }
    public ICarViewDomain Domain { get; }
    public ObservableViewModelCollection<CarViewModel, CarModel> CarBindings { get; }

    public CarDatabase(ObservableCollection<CarModel> cars, ICarViewDomain domain)
    {
        Cars = cars;
        Domain = domain;
        CarBindings = new(cars, c => new CarViewModel(Domain, c));
    }

    public void ResetCars(IEnumerable<CarModel> newCars)
    {
        Cars.Clear();
        foreach (var c in newCars)
            Cars.Add(c);
    } 
}

public partial class App : Application
{
    private void Main(object sender, StartupEventArgs e)
    {
        this.DispatcherUnhandledException += (_, e) =>
        {
            MessageBox.Show(
                "An unhandled exception just occurred: " + e.Exception.Message,
                e.Exception.GetType().FullName,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            e.Handled = true;
        };

        var resourceAssembly = Assembly.GetExecutingAssembly();
        var requiredResourceNames = resourceAssembly.GetManifestResourceNames()
            .Where(n => n.EndsWith(".txt"))
            .ToArray();
        var jsonSettings = CarModelUtils.GetSerializerSettings();
        var jsonSerializer = JsonSerializer.Create(jsonSettings);
        var assetLoader = new AssetLoader(jsonSerializer, resourceAssembly, requiredResourceNames);

        var rng = new Random(80850);
        var domain = new DynamicCarDomain(new(), new());
        var validator = new CarValidator(domain);
        var viewDomain = new CarViewDomain(validator, domain);
        var database = new CarDatabase(new(), viewDomain);

        var form = new LoadStuffMenu(database, assetLoader);
        form.Show();
    }
}

