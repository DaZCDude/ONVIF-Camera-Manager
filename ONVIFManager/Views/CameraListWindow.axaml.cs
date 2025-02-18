using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ONVIFManager.Views;
using System;
using System.IO;
using System.Text;

namespace ONVIFManager;

public partial class CameraListWindow : Window
{
    public MainWindow mainWindowRef;

    public CameraListWindow()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (mainWindowRef == null)
        {
            Close();
            return;
        }
        
        using (StreamReader sr = new StreamReader(mainWindowRef.SavedCamerasPath))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                CameraListBox.Items.Add(line);
                
                
            }
        }
    }
    
    public void AddCameraBtnClick(object sender, RoutedEventArgs args)
    {
        CameraListBox.Items.Add(urlInput.Text);
        urlInput.Text = "";
        RemoveCameraBtn.IsEnabled = true;
    }

    public void RemoveCameraBtnClick(object sender, RoutedEventArgs args)
    {
        if (CameraListBox.SelectedIndex == -1)
        {
            return;
        }

        CameraListBox.Items.RemoveAt(CameraListBox.SelectedIndex);

        if (CameraListBox.ItemCount < 1)
        {
            RemoveCameraBtn.IsEnabled = false;
        }
    }

    public void SaveBtnClick(object sender, RoutedEventArgs args)
    {
        using (FileStream fs = File.Create(mainWindowRef.SavedCamerasPath))
        {
            // Iterate through each item in the ListBox
            foreach (var item in CameraListBox.Items)
            {
                // Convert the item to a string (assuming the items are strings)
                string url = item.ToString();

                // Convert the URL string to a byte array
                Byte[] urlBytes = new UTF8Encoding(true).GetBytes(url + Environment.NewLine);

                // Write the URL to the file
                fs.Write(urlBytes, 0, urlBytes.Length);
            }
        }

        mainWindowRef.InitializeVLC();
        mainWindowRef.LoadView();
        Close();
    }

    public void CancelBtnClick(object sender, RoutedEventArgs args)
    {
        Close();
    }
}