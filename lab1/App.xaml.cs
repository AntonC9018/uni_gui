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
}


public static class CarDatabaseOperations
{
    public static void ResetCars(this CarDatabase db, IEnumerable<CarModel> newCars)
    {
        db.Cars.Clear();
        foreach (var c in newCars)
            db.Cars.Add(c);
    }

    public static CarModel AddNewCar(this CarDatabase db)
    {
        var car = new CarModel();
        db.Cars.Add(car);
        return car;
    }
}

public class SessionData
{
    public string DataPath { get; set; }
    public string CarDataPath { get; set; }
    public bool SaveOnExit { get; set; }
    public bool ShowToolBar { get; set; } = true;
    public bool ShowStatusBar { get; set; } = true;
}

public class AppCache
{
    public SessionData LastSession { get; set; }
}

public partial class App : Application
{
    private AppCache _appCache;
    private string _appCacheFilePath;

    public void SerializeCache()
    {
        var str = JsonConvert.SerializeObject(_appCache);
        File.WriteAllText(_appCacheFilePath, str);
    }

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


        var appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var cachePath = Path.Join(appdataPath, ".carapp");
        Directory.CreateDirectory(cachePath);

        _appCacheFilePath = Path.Join(cachePath, "cache.json");
        _appCache = LoadAppCache();
        AppCache LoadAppCache()
        {
            if (!File.Exists(_appCacheFilePath))
                return new AppCache();
            
            try
            {
                var appCacheText = File.ReadAllText(_appCacheFilePath);
                return JsonConvert.DeserializeObject<AppCache>(appCacheText);
            }
            catch (JsonException exc)
            {
                Console.WriteLine(exc);
                return new AppCache();
            }
        }
        var session = _appCache.LastSession;
        bool isDataDirectoryInitialized;
        if (session is null)
        {
            if (!assetLoader.CheckDataPathInitialized(cachePath))
                assetLoader.InitializeDataPath(cachePath);
            isDataDirectoryInitialized = true;
            _appCache.LastSession = session = new SessionData
            {
                DataPath = cachePath,
                CarDataPath = null,
            };
        }
        else
        {
            isDataDirectoryInitialized = assetLoader.CheckDataPathInitialized(session.DataPath);
        }

        var rng = new Random(80800);
        var domain = new DynamicCarDomain(new(), new());
        var validator = new CarValidator(domain);
        var viewDomain = new CarViewDomain(validator, domain);
        var database = new CarDatabase(new(), viewDomain);

        var carAssetModel = new CarAssetModel
        {
            Session = session,
            IsDataDirectoryInitialized = isDataDirectoryInitialized,
            IsDirty = false,
        };
        var carAssetViewModel = new CarAssetViewModel(carAssetModel);
        var carOperations = new CarAssetDatabaseOperations(assetLoader, carAssetViewModel, database);

        if (isDataDirectoryInitialized)
            assetLoader.ReadDomainData(domain, session.DataPath);

        database.CarBindings.CollectionChanged += (sender, e) =>
        {
            carAssetViewModel.IsDirty = true;
            if (e.NewItems is null)
                return;
            foreach (var it in e.NewItems)
            {
                ((CarViewModel) it).PropertyChanged += (_, _) =>
                {
                    carAssetViewModel.IsDirty = true;
                };
            }
        };

        if (session.CarDataPath is not null)
        {
            if (File.Exists(session.CarDataPath))
            {
                try
                {
                    carOperations.LoadCarsFile(session.CarDataPath);
                }
                catch (JsonException exc)
                {
                    session.CarDataPath = null;
                    Console.WriteLine("Wrong format? " + exc);
                }
            }
            else
            {
                session.CarDataPath = null;
            }
        }

        this.Exit += (_, _) =>
        {
            SerializeCache();
            if (_appCache.LastSession.SaveOnExit)
                carOperations.SaveCurrentCars();
        };

        var form = new LoadStuffMenu(carOperations, carAssetViewModel, database);
        form.Show();
    }
}

