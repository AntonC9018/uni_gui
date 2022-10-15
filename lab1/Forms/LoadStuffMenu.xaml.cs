using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using CarApp.Assets;
using CarApp.Model;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;

namespace CarApp;

public interface IAssetLoaderService
{
    void InitializeDataPath(string dataPath);
    bool CheckDataPathInitialized(string dataPath);
    void SaveCars(IEnumerable<CarModel> cars, string path);
    IEnumerable<CarModel> LoadCars(string path);
}

// Right now it's kinda two things,
// both loader of things and also context of the domain,
// but not the context of the currently open car database.
public class AssetLoader : IAssetLoaderService
{
    private JsonSerializer CarSerializer { get; }
    private Assembly ResourceAssembly { get; }
    private IReadOnlyList<string> RequiredResourceNames { get; }

    public AssetLoader(JsonSerializer carSerializer, Assembly resourceAssembly, string[] requiredResourceNames)
    {
        CarSerializer = carSerializer;
        ResourceAssembly = resourceAssembly;
        RequiredResourceNames = requiredResourceNames;
    }

    public void InitializeDataPath(string dataPath)
    {
        // Should be factored out into a method on the asset loader (or even better on the view model).
        var paths = AssetHelper.GetRequiredDataPaths(RequiredResourceNames, dataPath);
        AssetHelper.InitializeDataPath(ResourceAssembly, paths);
    }

    public bool CheckDataPathInitialized(string dataPath)
    {
        return AssetHelper.IsDataPathInitialized(ResourceAssembly, RequiredResourceNames, dataPath);
    }

    public void SaveCars(IEnumerable<CarModel> cars, string path)
    {
        DataHelper.WriteJson(path, CarSerializer, cars);
    }

    public IEnumerable<CarModel> LoadCars(string path)
    {
        return DataHelper.ReadJson<List<CarModel>>(path, CarSerializer);
    }
}

public class CarAssetModel : IAssetContext
{
    public string DataPath { get; set; }
    public bool IsDataDirectoryInitialized { get; set; }
    public string CarDataPath;
    public bool IsDirty { get; set; }
}

public static class AssetHelper
{
    public static bool IsDataPathInitialized(Assembly resourceAssembly, IEnumerable<string> requiredDataResourceNames, string dataPath)
    {
        return GetRequiredDataPaths(requiredDataResourceNames, dataPath).Any(p => !File.Exists(p.FullPath));
    }

    public record struct ResourcePath(string FullPath, string ResourceName);

    public static IEnumerable<ResourcePath> GetRequiredDataPaths(
        IEnumerable<string> requiredDataResourceNames, string dataPath)
    {
        return requiredDataResourceNames.Select(rname =>
        {
            var path = Path.Join(dataPath, rname);
            return new ResourcePath(path, rname);
        });
    }

    public static void InitializeDataPath(Assembly resourceAssembly, IEnumerable<ResourcePath> dataPaths)
    {
        var e = dataPaths.GetEnumerator();
        if (!e.MoveNext())
            return;

        var dataDirectoryName = Path.GetDirectoryName(e.Current.FullPath);
        Directory.CreateDirectory(dataDirectoryName);
        
        do
        {
            var c = e.Current;
            if (File.Exists(c.FullPath))
                continue;

            using var stream = resourceAssembly.GetManifestResourceStream(c.ResourceName);
            using var fileStream = new FileStream(c.FullPath, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);
        }
        while (e.MoveNext());
    }
}


public partial class LoadStuffMenu : Window
{
    private IAssetLoaderService _assetLoader;
    private CarAssetModel _assetModel;
    private CarDatabase _database;

    public LoadStuffMenu(CarDatabase database, IAssetLoaderService assetLoader)
    {
        _database = database;
        _assetLoader = assetLoader;
        _assetModel = new();
        
        _database.CarBindings.CollectionChanged += (sender, e) =>
        {
            foreach (var it in e.NewItems)
            {
                ((CarViewModel) it).PropertyChanged += (_, _) =>
                {
                    // This should be under a property, with update events.
                    _assetModel.IsDirty = true;
                };
            }
        };

        InitializeComponent();
    }

    internal void PrepareDataFolderForUse(object sender, EventArgs e)
    {
        if (_assetModel.IsDataDirectoryInitialized
            || _assetModel.DataPath is null)
        {
            return;
        }

        _assetLoader.InitializeDataPath(_assetModel.DataPath);
        _assetModel.IsDataDirectoryInitialized = true;
    }

    internal void ShowOpenDataFolderInExplorerDialog(object sender, EventArgs e)
    {
        if (_assetModel.DataPath is not null)
            Process.Start(_assetModel.DataPath);
    }

    internal void ShowSelectDataFolderDialog(object sender, EventArgs e)
    {
        var dialog = new VistaFolderBrowserDialog();
        dialog.Description = "Select the data folder";
        dialog.UseDescriptionForTitle = true;
        dialog.Multiselect = false;
        
        if (!(dialog.ShowDialog(this) ?? false))
            return;
        
        // Same thing, should probably be moved.
        string selectedPath = dialog.SelectedPath;
        _assetModel.DataPath = selectedPath;
        _assetModel.IsDataDirectoryInitialized = _assetLoader.CheckDataPathInitialized(selectedPath);
    }

    private string GetDefaultSavePath()
    {
        if (_assetModel.CarDataPath is not null)
        {
            var filePath = _assetModel.CarDataPath;
            var folderPath = Path.GetDirectoryName(filePath);
            return folderPath;
        }

        if (_assetModel.DataPath is not null)
            return _assetModel.DataPath;

        Debug.Fail("The default data path should be initially set to somewhere in the appdata");
        return null;
    }

    internal void ShowOpenCarDatabaseDialog(object sender, EventArgs e)
    {
        var dialog = new VistaOpenFileDialog();
        dialog.Title = "Select the car database file";
        dialog.Multiselect = false;
        dialog.AddExtension = true;
        dialog.CheckFileExists = true;
        dialog.DefaultExt = "json";
        dialog.InitialDirectory = GetDefaultSavePath();

        if (!(dialog.ShowDialog(this) ?? false))
            return;

        string selectedFile = dialog.FileName;
        try
        {
            var cars = _assetLoader.LoadCars(selectedFile);
            _database.ResetCars(cars);
            _assetModel.IsDirty = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not load file {selectedFile}: {ex.Message}.\r\n{ex.StackTrace}",
                "Could not load file",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }
    }

    private void SaveCurrentCars()
    {
        try
        {
            _assetLoader.SaveCars(_database.Cars, _assetModel.CarDataPath);
            _assetModel.IsDirty = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not save file {_assetModel.CarDataPath}: {ex.Message}.\r\n{ex.StackTrace}",
                "Could not save file",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }
    }

    internal void SaveCarDatabase_MaybeShowSaveAsDialog(object sender, EventArgs e)
    {
        if (_assetModel.CarDataPath is null)
        {
            ShowCreateNewCarDatabaseDialog(sender, e);
            return;
        }
        SaveCurrentCars();
    }

    private VistaSaveFileDialog GetCarSaveFileDialog()
    {
        var dialog = new VistaSaveFileDialog();
        dialog.Title = "Save the car database file";
        dialog.AddExtension = true;
        dialog.DefaultExt = "json";
        dialog.InitialDirectory = GetDefaultSavePath();
        return dialog;
    }

    internal void ShowCreateNewCarDatabaseDialog(object sender, EventArgs e)
    {
        ShowSaveAsDialog(sender, e);
    }
    
    internal void ShowSaveAsDialog(object sender, EventArgs e)
    {
        var dialog = GetCarSaveFileDialog();
        dialog.CheckFileExists = false;
        
        if (!(dialog.ShowDialog(this) ?? false))
            return;

        _assetModel.CarDataPath = dialog.FileName;
        SaveCurrentCars();
    }

    
    private static readonly string[] _FirstNames = { "Steve", "John", "Maria", "Grace", };
    private static readonly string[] _LastNames = { "Smith", "Miller", "Martin", "Bower", };

    internal void GenerateRandomCars(Random rng, int numCarsToGenerate = 5)
    {
        var cars = new List<CarModel>();

        for (int i = 0; i < numCarsToGenerate; i++)
        {
            var car = CarModelUtils.GenerateRandomModel(
                rng,
                _database.Domain.Manufacturers.Count,
                _database.Domain.Countries.Count,
                _FirstNames, _LastNames);
            cars.Add(car);
        }
    }

    private void OpenPreviousSession()
    {
        // TODO: save the previous path in the registry or the app config?
        // const string carsFileName = "cars.json";
        // if (!assets.TryReadJson(carsFileName, jsonSerializer, out List<CarModel> cars))
        //     assets.WriteJson(carsFileName, jsonSerializer, cars);
    }
}
