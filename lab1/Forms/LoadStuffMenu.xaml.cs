using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using CarApp.Assets;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;

namespace CarApp;

// Right now it's kinda two things,
// both loader of things and also context of the domain,
// but not the context of the currently open car database.
public class AssetLoaderContext
{

    // It's fine, can be shared.
    public JsonSerializer CarSerializer { get; }
    public Assembly ResourceAssembly { get; }
    public IReadOnlyList<string> RequiredResourceNames { get; }

    public AssetLoaderContext(JsonSerializer carSerializer, Assembly resourceAssembly, string[] requiredResourceNames)
    {
        CarSerializer = carSerializer;
        ResourceAssembly = resourceAssembly;
        RequiredResourceNames = requiredResourceNames;
    }
}

public class CarAssetHandlerState : IAssetContext
{
    public string DataPath { get; set; }
    public bool IsDataDirectoryInitialized { get; set; }
    public string CarDataPath;
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

public sealed class AssetLoaderViewModel : INotifyPropertyChanged
{
    private AssetLoaderContext _assets;
    public event PropertyChangedEventHandler PropertyChanged;

    public AssetLoaderViewModel(AssetLoaderContext assets)
    {
        _assets = assets;
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string DataPath
    {
        get => _assets.DataPath;
        set
        {

        }
    }

    public bool CanLoad
    {
        get => _assets.IsDataDirectoryInitialized;
    }
}

public partial class LoadStuffMenu : Window
{
    private AssetLoaderContext _assetLoader;
    private AssetLoaderViewModel _assetsViewModel;
    private CarAssetHandlerState _carAssetState;


    public LoadStuffMenu(CarDatabase database, AssetLoaderContext assetLoader)
    {
        DataContext = database;
        _assetLoader = assetLoader;
        _assetsViewModel = new(_assetLoader);
        _carAssetState = new();

        InitializeComponent();
    }

    internal void PrepareDataFolderForUse(object sender, EventArgs e)
    {
        if (_assetLoader.IsDataDirectoryInitialized
            || _assetLoader.DataPath is null)
        {
            return;
        }

        // Should be factored out into a method on the asset loader (or even better on the view model).
        var paths = AssetHelper.GetRequiredDataPaths(_assetLoader.RequiredResourceNames, _assetLoader.DataPath);
        AssetHelper.InitializeDataPath(_assetLoader.ResourceAssembly, paths);
        // TODO: set the bool.
        _assetLoader.IsDataDirectoryInitialized = true;
    }

    internal void ShowOpenDataFolderInExplorerDialog(object sender, EventArgs e)
    {
        if (_assetLoader.DataPath is not null)
            Process.Start(_assetLoader.DataPath);
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
        _assetLoader.DataPath = selectedPath;
        _assetLoader.IsDataDirectoryInitialized = AssetHelper.IsDataPathInitialized(
            _assetLoader.ResourceAssembly, _assetLoader.RequiredResourceNames, selectedPath);
    }

    private string GetDefaultSavePath()
    {
        if (_carAssetState.CarDataPath is not null)
        {
            var filePath = _carAssetState.CarDataPath;
            var folderPath = Path.GetDirectoryName(filePath);
            return folderPath;
        }

        if (_assetLoader.DataPath is not null)
            return _assetLoader.DataPath;

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
        // TODO: call the loading functton here.
    }

    internal void SaveCarDatabase_MaybeShowSaveAsDialog(object sender, EventArgs e)
    {
        if (_carAssetState.CarDataPath is null)
        {
            ShowCreateNewCarDatabaseDialog(sender, e);
            return;
        }

        // TODO: write to file.
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
        var dialog = GetCarSaveFileDialog();
        dialog.CheckFileExists = true;

        if (!(dialog.ShowDialog(this) ?? false))
            return;

        var filePath = dialog.FileName;
        _carAssetState.CarDataPath = filePath;

        // TODO: save.
    }
    
    internal void ShowSaveAsDialog(object sender, EventArgs e)
    {
        var dialog = GetCarSaveFileDialog();
        dialog.CheckFileExists = false;
        
        // TODO: refactor with the method above.
        if (!(dialog.ShowDialog(this) ?? false))
            return;

        var filePath = dialog.FileName;
        _carAssetState.CarDataPath = filePath;

        // TODO: save.
    }
}
