﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using Newtonsoft.Json;
using CarApp.Model;
using CarApp.Assets;
using System.Collections.ObjectModel;

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

        var basePath = (e.Args.Length > 1) ? e.Args[1] : Directory.GetCurrentDirectory();
        var dataPath = Path.Join(basePath, "data");
        var assets = new AssetLoaderService();
        assets.DataPath = dataPath;
        if (!Directory.Exists(dataPath))
        {
            Console.WriteLine("No data folder found. Make sure you're running the program in the right directory.");
            return;
        }

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

        var validator = new CarValidator(carRegistry);
        var domain = new CarDomain(validator, carRegistry);
        var db = new CarDatabase(new(cars), domain);
        var form = new LoadStuffMenu(db, assets);
        form.Show();
    }
}

