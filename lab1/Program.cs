using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using CarApp.Model;
using CarApp.Assets;
using System.Windows.Forms;

namespace CarApp;

public class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        var basePath = (args.Length > 1) ? args[1] : Directory.GetCurrentDirectory();
        var dataPath = Path.Join(basePath, "data");
        var assets = DataHelper.CreateAssetContext(dataPath);
        if (assets is null)
            return 1;

        var carRegistry = new CarDependenciesRegistry();
        if (!carRegistry.Initialize(assets))
            return 1;

        var jsonSettings = CarModelUtils.GetSerializerSettings();
        var jsonSerializer = JsonSerializer.Create(jsonSettings);

        const string carsFileName = "cars.json";
        if (!assets.TryReadJson(carsFileName, jsonSerializer, out List<CarModel> cars))
        {
            const int numCarsToGenerate = 5;
            cars = new List<CarModel>();
            var rng = new Random(80850);

            string[] firstNames = { "Steve", "John", "Maria", "Grace", };
            string[] lastNames = { "Smith", "Miller", "Martin", "Bower", };

            for (int i = 0; i < numCarsToGenerate; i++)
            {
                var car = CarModelUtils.GenerateRandomModel(rng, carRegistry, firstNames, lastNames);
                cars.Add(car);
            }
            assets.WriteJson(carsFileName, jsonSerializer, cars);
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());

        return 0;
    }
}
