using Avalonia.Controls;
using Avalonia.Interactivity;
using ONVIFManager.Views;

namespace ONVIFManager;

public partial class SettingsWindow : Window
{
    public MainWindow mainWindowRef;

    public SettingsWindow()
    {
        InitializeComponent();
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        codecinput.SelectedIndex = mainWindowRef.avcodec;
        swscaleinput.SelectedIndex = mainWindowRef.swscalemode;
        skiploopfilterinput.SelectedIndex = mainWindowRef.skiploopfilter;
    }

    public void CancelBtnClick(object sender, RoutedEventArgs args)
    {
        Close();
    }

    public void ApplyBtnClick(object sender, RoutedEventArgs args)
    {
        mainWindowRef.avcodec = codecinput.SelectedIndex;
        mainWindowRef.swscalemode = swscaleinput.SelectedIndex;
        mainWindowRef.skiploopfilter = skiploopfilterinput.SelectedIndex;

        if (AudioCheckBox.IsChecked == true)
        {
            mainWindowRef.streamaudio = "";
        }
        else
        {
            mainWindowRef.streamaudio = "--no-audio";
        }

        mainWindowRef.InitializeVLC();
        mainWindowRef.LoadView();
        Close();
    }
}