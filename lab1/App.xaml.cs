using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using Newtonsoft.Json;
using CarApp.Model;
using CarApp.Assets;

namespace CarApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Main(object sender, StartupEventArgs e)
        {
            this.DispatcherUnhandledException += (_, e) =>
            {
                MessageBox.Show(
                    "An unhandled exception just occurred: " + e.Exception.Message,
                    "Exception Sample",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                e.Handled = true;
            };

            var basePath = (e.Args.Length > 1) ? e.Args[1] : Directory.GetCurrentDirectory();
            var dataPath = Path.Join(basePath, "data");
            var assets = DataHelper.CreateAssetContext(dataPath);
            if (assets is null)
                return;

            var carRegistry = new CarDependenciesRegistry();
            if (!carRegistry.Initialize(assets))
                return;

            var jsonSettings = CarModelUtils.GetSerializerSettings();
            var jsonSerializer = JsonSerializer.Create(jsonSettings);
            var rng = new Random(80850);

            const string carsFileName = "cars.json";
            if (!assets.TryReadJson(carsFileName, jsonSerializer, out List<CarModel> cars))
            {
                const int numCarsToGenerate = 5;
                cars = new List<CarModel>();

                string[] firstNames = { "Steve", "John", "Maria", "Grace", };
                string[] lastNames = { "Smith", "Miller", "Martin", "Bower", };

                for (int i = 0; i < numCarsToGenerate; i++)
                {
                    var car = CarModelUtils.GenerateRandomModel(rng, carRegistry, firstNames, lastNames);
                    cars.Add(car);
                }
                assets.WriteJson(carsFileName, jsonSerializer, cars);
            }

            var db = new CarDatabase(new(cars), carRegistry);
            var form = new Form1(db);
            form.Show();
        }
    }
}
