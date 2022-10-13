using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using CarApp.Assets;
using Ookii.Dialogs.Wpf;

namespace CarApp;

public class AssetLoaderService : IAssetContext
{
    public string DataPath { get; set; }
    public bool IsDataDirectoryInitialized;
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
    private AssetLoaderService _assets;
    public event PropertyChangedEventHandler PropertyChanged;

    public AssetLoaderViewModel(AssetLoaderService assets)
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
    private AssetLoaderService _assetLoader;
    private AssetLoaderViewModel _assetsViewModel;

    private Assembly _resourceAssembly;
    private string[] _requiredResourceNames;

    public LoadStuffMenu(CarDatabase database, AssetLoaderService assetLoader)
    {
        DataContext = database;
        _assetLoader = assetLoader;
        _assetsViewModel = new(_assetLoader);

        _resourceAssembly = Assembly.GetExecutingAssembly();
        _requiredResourceNames = _resourceAssembly.GetManifestResourceNames()
            .Where(n => n.EndsWith(".txt"))
            .ToArray();

        InitializeComponent();
    }

    internal void PrepareDataFolderForUse(object sender, EventArgs e)
    {
        if (_assetLoader.IsDataDirectoryInitialized
            || _assetLoader.DataPath is null)
        {
            return;
        }

        var paths = AssetHelper.GetRequiredDataPaths(_requiredResourceNames, _assetLoader.DataPath);
        AssetHelper.InitializeDataPath(_resourceAssembly, paths);

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

        string selectedPath = dialog.SelectedPath;
        _assetLoader.DataPath = selectedPath;
        _assetLoader.IsDataDirectoryInitialized = AssetHelper.IsDataPathInitialized(
            _resourceAssembly, _requiredResourceNames, selectedPath);
    }

    internal void ShowOpenCarDatabaseDialog(object sender, EventArgs e)
    {
        var dialog = new VistaOpenFileDialog();
        dialog.Title = "Select the car database file";
        dialog.Multiselect = false;
        dialog.AddExtension = true;
        dialog.CheckFileExists = true;
        dialog.DefaultExt = "json";

        string GetDefaultPath()
        {
            if (_assetLoader.CarDataPath is not null)
            {
                var filePath = _assetLoader.CarDataPath;
                var folderPath = Path.GetDirectoryName(filePath);
                return folderPath;
            }

            if (_assetLoader.DataPath is not null)
                return _assetLoader.DataPath;

            Debug.Fail("The default data path should be initially set to somewhere in the appdata");
            return null;
        }

        var folder = GetDefaultPath();
        dialog.InitialDirectory = folder;

        if (!(dialog.ShowDialog(this) ?? false))
            return;

        string selectedFile = dialog.FileName;
        // TODO: call the loading functton here.
    }

    internal void SaveCarDatabase_MaybeShowSaveAsDialog(object sender, EventArgs e)
    {
        if (_assetLoader.CarDataPath is null)
        {
            ShowCreateNewCarDatabaseDialog(sender, e);
            return;
        }

        
    }

    internal void ShowCreateNewCarDatabaseDialog(object sender, EventArgs e)
    {
    }
    
    internal void ShowSaveAsDialog(object sender, EventArgs e)
    {
    }
}
