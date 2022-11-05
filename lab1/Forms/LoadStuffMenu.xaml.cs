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

    public void ReadDomainData(DynamicCarDomain outDomain, string dataPath)
    {
        void Read(IList<string> output, string resourceName)
        {
            output.Clear();
            var p = Path.Join(dataPath, RequiredResourceNames[0]); 
            foreach (var country in File.ReadAllLines(p))
                output.Add(country);
        }
        Read(outDomain.Countries, RequiredResourceNames.First(n => n.Contains("manufacturers")));
        Read(outDomain.Manufacturers, RequiredResourceNames.First(n => n.Contains("countries")));
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
    public SessionData Session;
    public string DataPath
    {
        get => Session.DataPath;
        set => Session.DataPath = value;
    }
    public bool IsDataDirectoryInitialized { get; set; }
    public string CarDataPath
    {
        get => Session.CarDataPath;
        set => Session.CarDataPath = value;
    }
    public bool IsDirty { get; set; }
}

public static class AssetHelper
{
    public static bool IsDataPathInitialized(Assembly resourceAssembly, IEnumerable<string> requiredDataResourceNames, string dataPath)
    {
        return GetRequiredDataPaths(requiredDataResourceNames, dataPath).All(p => File.Exists(p.FullPath));
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


public class CarAssetViewModel : INotifyPropertyChanged
{
    private CarAssetModel _model;

    public CarAssetViewModel(CarAssetModel model)
    {
        _model = model;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string DataPath
    {
        get => _model.DataPath;
        set
        {
            if (DataPath != value)
            {
                bool openBefore = IsDataPathOpen;
                _model.DataPath = value;
                OnPropertyChanged(nameof(DataPath));
                if (IsDataPathOpen != openBefore)
                    OnPropertyChanged(nameof(IsDataPathOpen));
            }
        }
    }
    public bool IsDataPathOpen => DataPath is not null;
    public bool IsDataDirectoryInitialized
    {
        get => _model.IsDataDirectoryInitialized;
        set
        {
            if (IsDataDirectoryInitialized != value)
            {
                bool canInitializeBefore = CanInitializeDataPath;
                _model.IsDataDirectoryInitialized = value;
                OnPropertyChanged(nameof(IsDataDirectoryInitialized));
                if (canInitializeBefore != CanInitializeDataPath)
                    OnPropertyChanged(nameof(CanInitializeDataPath));
            }
        }
    }
    public bool CanInitializeDataPath
    {
        get => !IsDataDirectoryInitialized && IsDataPathOpen;
    }
    public bool IsCarDatabaseOpen
    {
        get => CarDataPath is not null;
    }
    public string CarDataPath
    {
        get => _model.CarDataPath;
        set
        {
            if (CarDataPath != value)
            {
                bool openBefore = IsCarDatabaseOpen;
                _model.CarDataPath = value;
                OnPropertyChanged(nameof(CarDataPath));

                if (openBefore != IsCarDatabaseOpen)
                   OnPropertyChanged(nameof(IsCarDatabaseOpen));
            }
        }
    }
    public bool IsDirty
    {
        get => _model.IsDirty;
        set
        {
            if (IsDirty != value)
            {
                _model.IsDirty = value;
                OnPropertyChanged(nameof(IsDirty));
            }
        }
    }
    public bool SaveOnExit
    {
        get => _model.Session.SaveOnExit;
        set
        {
            if (SaveOnExit != value)
            {
                _model.Session.SaveOnExit = value;
                OnPropertyChanged(nameof(SaveOnExit));
            }
        }
    }
}

// It's kind of a mess, but i'm too lazy to clean it up.
public class CarAssetDatabaseOperations
{
    private IAssetLoaderService AssetLoader { get; }
    private CarAssetViewModel AssetViewModel { get; }
    private CarDatabase Database { get; }

    public CarAssetDatabaseOperations(IAssetLoaderService assetLoader, CarAssetViewModel assetViewModel, CarDatabase database)
    {
        AssetLoader = assetLoader;
        AssetViewModel = assetViewModel;
        Database = database;
    }

    public void InitializeDataPath()
    {
        if (AssetViewModel.IsDataDirectoryInitialized
            || AssetViewModel.DataPath is null)
        {
            return;
        }

        AssetLoader.InitializeDataPath(AssetViewModel.DataPath);
        AssetViewModel.IsDataDirectoryInitialized = true;
    }

    private static void ShowInExplorer(string path)
    {
        Process.Start("explorer.exe", path);
    }

    private static void ShowFileInExplorer(string path)
    {
        Process.Start("explorer.exe", $"/select, \"{path}\"");
    }

    public void ShowDataFolderInExplorer()
    {
        if (AssetViewModel.DataPath is not null)
            ShowInExplorer(AssetViewModel.DataPath);
    }

    public void ShowCarDatabaseInExplorer()
    {
        if (AssetViewModel.CarDataPath is not null)
            ShowFileInExplorer(AssetViewModel.CarDataPath);
    }

    public string GetDefaultSavePath()
    {
        if (AssetViewModel.CarDataPath is not null)
        {
            var filePath = AssetViewModel.CarDataPath;
            var folderPath = Path.GetDirectoryName(filePath);
            return folderPath;
        }

        if (AssetViewModel.DataPath is not null)
            return AssetViewModel.DataPath;

        Debug.Fail("The default data path should be initially set to somewhere in the appdata");
        return null;
    }

    public bool SaveCurrentCars()
    {
        try
        {
            AssetLoader.SaveCars(Database.Cars, AssetViewModel.CarDataPath);
            AssetViewModel.IsDirty = false;
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not save file {AssetViewModel.CarDataPath}: {ex.Message}.\r\n{ex.StackTrace}",
                "Could not save file",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }
    }

    public void Reset()
    {
        Database.Cars.Clear();
        AssetViewModel.CarDataPath = null;
    }

    // Shouldn't be here. Should probably be available on the domain.
    public static readonly string[] _FirstNames = { "Steve", "John", "Maria", "Grace", };
    public static readonly string[] _LastNames = { "Smith", "Miller", "Martin", "Bower", };

    public void GenerateRandomCars(Random rng, int numCarsToGenerate = 5)
    {
        var cars = new List<CarModel>();

        for (int i = 0; i < numCarsToGenerate; i++)
        {
            var car = CarModelUtils.GenerateRandomModel(
                rng,
                Database.Domain.Manufacturers.Count,
                Database.Domain.Countries.Count,
                _FirstNames, _LastNames);
            cars.Add(car);
        }
    }

    public void LoadCarsFile(string selectedFile)
    {
        try
        {
            var cars = AssetLoader.LoadCars(selectedFile);
            Database.ResetCars(cars);
            AssetViewModel.IsDirty = false;
            AssetViewModel.CarDataPath = selectedFile;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not load file {selectedFile}: {ex.Message}",
                "Could not load file",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }
    }

    public void SelectDataFolder(string selectedPath)
    {
        AssetViewModel.DataPath = selectedPath;
        AssetViewModel.IsDataDirectoryInitialized = AssetLoader.CheckDataPathInitialized(selectedPath);
    }
}

public partial class LoadStuffMenu : Window
{
    private CarAssetDatabaseOperations _carOperations;
    private CarAssetViewModel AssetViewModel { get; }

    private VistaFolderBrowserDialog _selectDataPathDialog;
    private VistaOpenFileDialog _openCarDatabaseDialog;
    private VistaSaveFileDialog _saveCarDatabaseDialog;
    private TaskDialog _whetherToSaveDirtiedFileDialog;

    public LoadStuffMenu(
        CarAssetDatabaseOperations carOperations,
        CarAssetViewModel assetViewModel,
        CarDatabase database)
    {
        _carOperations = carOperations;
        AssetViewModel = assetViewModel;
        DataContext = AssetViewModel;

        _openCarDatabaseDialog = new()
        {
            Title = "Select the car database file",
            Multiselect = false,
            AddExtension = true,
            CheckFileExists = true,
            DefaultExt = "json",
            InitialDirectory = AssetViewModel.DataPath,
            FileName = AssetViewModel.CarDataPath,
        };

        _saveCarDatabaseDialog = new()
        {
            Title = "Save the car database file",
            AddExtension = true,
            DefaultExt = "json",
            InitialDirectory = AssetViewModel.DataPath,
            FileName = AssetViewModel.CarDataPath,
        };

        _selectDataPathDialog = new()
        {
            Description = "Select the data folder",
            UseDescriptionForTitle = true,
            Multiselect = false,
            SelectedPath = AssetViewModel.DataPath,
        };

        {
            var d = new TaskDialog();
            _whetherToSaveDirtiedFileDialog = d;

            d.Buttons.Add(new TaskDialogButton
            {
                Text = "Save",
            });
            d.Buttons.Add(new TaskDialogButton
            {
                Text = "Discard",
            });
            d.Buttons.Add(new TaskDialogButton
            {
                Text = "Cancel",
            });
            d.WindowTitle = "There are unsaved changes";
            d.Content = "What do I do with the unsaved changes?";
        }

        InitializeComponent();

        var dataGrid = new CarDataGrid(database);
        CarsGroupBox.Content = dataGrid;
    }

    internal void ShowSelectDataFolderDialog(object sender, EventArgs e)
    {
        if (_selectDataPathDialog.ShowDialog(this) is false or null)
            return;
        
        string selectedPath = _selectDataPathDialog.SelectedPath;
        _carOperations.SelectDataFolder(selectedPath);
    }

    internal void ShowOpenCarDatabaseDialog(object sender, EventArgs e)
    {
        if (_openCarDatabaseDialog.ShowDialog(this) is false or null)
            return;

        string selectedFile = _openCarDatabaseDialog.FileName;
        _carOperations.LoadCarsFile(selectedFile);
    }

    internal void SaveCarDatabase_MaybeShowSaveAsDialog(object sender, EventArgs e)
    {
        if (AssetViewModel.CarDataPath is null)
        {
            ShowSaveAsDialog(sender, e);
            return;
        }
        _carOperations.SaveCurrentCars();
    }

    internal void ShowCreateNewCarDatabaseDialog(object sender, EventArgs e)
    {
        if (!AssetViewModel.IsDirty)
        {
            _carOperations.Reset();
            return;
        }

        var button = _whetherToSaveDirtiedFileDialog.Show();
        if (button is null)
            return;
        switch (button.Text)
        {
            case "Save":
            {
                if (AssetViewModel.CarDataPath is null)
                {
                    ShowSaveAsDialog(sender, e);
                    return;
                }
                if (!_carOperations.SaveCurrentCars())
                    return;
                else
                    break;
            }
            case "Discard":
                break;
            case "Cancel":
                return;
            default:
            {
                Debug.Fail("No such button " + button.Text);
                return;
            }
        }
        _carOperations.Reset();
    }
    
    internal void ShowSaveAsDialog(object sender, EventArgs e)
    {
        var dialog = _saveCarDatabaseDialog;
        
        if (dialog.ShowDialog(this) is false or null)
            return;

        AssetViewModel.CarDataPath = dialog.FileName;
        _carOperations.SaveCurrentCars();
    }

    internal void ShowDataFolderInExplorer(object sender, EventArgs e)
    {
        _carOperations.ShowDataFolderInExplorer();
    }

    internal void InitializeDataPath(object sender, EventArgs e)
    {
        _carOperations.InitializeDataPath();
    }

    internal void ShowCarDatabaseInExplorer(object sender, EventArgs e)
    {
        _carOperations.ShowCarDatabaseInExplorer();
    }
}
