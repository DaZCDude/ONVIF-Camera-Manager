using Avalonia.Controls;
using Avalonia.Interactivity;
using LibVLCSharp.Shared;
using LibVLCSharp.Avalonia;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using System.Linq;

namespace ONVIFManager.Views
{
    public partial class MainWindow : Window
    {
        //LibVLC and Setting variables
        private LibVLC _libVlc;
        public int avcodec = 0;
        public int swscalemode = 0;
        public int skiploopfilter = 4;
        public string streamaudio = "--no-audio";

        //Keep count of all camera URLs and amounts
        List<string> cameraUrls = new List<string>();
        private int CameraAmount = 0;

        //Path to Saved Cameras text file
        public string SavedCamerasPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saved_cameras.txt");

        public MainWindow()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (Design.IsDesignMode == true)
            {
                return;
            }

            //Initialize VLC Library on Windows. Linux needs VLC to be installed
            if (OperatingSystem.IsWindows())
            {
                //Core.Initialize(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "/Assets/VLC"));
                Core.Initialize(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "VLC"));
            }

            InitializeVLC();

            //Load all camera views
            LoadView();
        }

        private void OnVlcLog(object sender, LogEventArgs e)
        {
            // Format the log message
            string logMessage = $"[{e.Level}] {e.Module}: {e.Message}";

            // Write to a file
            File.AppendAllText("vlc_logs.txt", logMessage + Environment.NewLine);
        }

        // Override the OnKeyDown method to handle key presses
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Check if the F11 key is pressed
            if (e.Key == Key.F11)
            {
                // Call your custom function
                ToggleFullscreen();
            }
            if (e.Key == Key.Escape)
            {
                if (WindowState == WindowState.FullScreen)
                {
                    WindowState = WindowState.Normal;
                    VideoGrid.Margin = new Thickness(0, 0, 0, 100);
                }
            }
        }

        public void EditCameraListBtnClick(object sender, RoutedEventArgs args)
        {
            var ownerWindow = this;
            CameraListWindow cameraListWindow = new CameraListWindow();
            cameraListWindow.mainWindowRef = this;
            cameraListWindow.ShowDialog(ownerWindow);
        }

        public void SettingsBtnClick(object sender, RoutedEventArgs args)
        {
            var ownerWindow = this;
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.mainWindowRef = this;
            settingsWindow.ShowDialog(ownerWindow);
        }

        public void FullscreenBtnClick(object sender, RoutedEventArgs args)
        {
            ToggleFullscreen();
        }

        private void ToggleFullscreen()
        {
            if (WindowState == WindowState.FullScreen)
            {
                WindowState = WindowState.Normal;
                VideoGrid.Margin = new Thickness(0, 0, 0, 100);
            }
            else
            {
                WindowState = WindowState.FullScreen;
                VideoGrid.Margin = new Thickness(0, 0, 0, 0);
            }
        }

        public void InitializeVLC()
        {
            if (_libVlc != null)
            {
                _libVlc.Dispose();
            }

            // Initialize a single LibVLC instance
            string[] options = { "--swscale-mode=" + swscalemode.ToString(),
                                "--avcodec-hw=" + avcodec.ToString(),
                                "--verbose=2",
                                "--avcodec-skiploopfilter=" + skiploopfilter.ToString(),
                                "--vout=any",
                                streamaudio };
            _libVlc = new LibVLC(options);
            _libVlc.Log += OnVlcLog;
        }

        public void LoadView()
        {
            if (!File.Exists(SavedCamerasPath))
            {
                using (FileStream fs = File.Create(SavedCamerasPath))
                {
                    // Add default URL
                    Byte[] title = new UTF8Encoding(true).GetBytes("http://download.blender.org/peach/bigbuckbunny_movies/big_buck_bunny_480p_h264.mov");
                    fs.Write(title, 0, title.Length);
                }
            }

            int lineCount = 0;

            using (StreamReader sr = new StreamReader(SavedCamerasPath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    lineCount++;

                    // Add the URL to the list
                    cameraUrls.Add(line);
                }
            }

            // Stop and clear all video views
            foreach (var videoView in VideoGrid.Children.OfType<VideoView>().ToList())
            {
                // Stop the media player and remove the video view
                var mediaPlayer = videoView.MediaPlayer;
                if (mediaPlayer != null)
                {
                    mediaPlayer.Stop();
                }

                VideoGrid.Children.Remove(videoView);
            }

            // Optionally clear other resources if needed
            CameraAmount = 0;

            // Calculate the optimal grid size
            int requiredColumns = (int)Math.Ceiling(Math.Sqrt(lineCount)); // Square root to get a balanced grid
            int requiredRows = (int)Math.Ceiling((double)lineCount / requiredColumns);

            // Ensure the grid has enough rows and columns
            UpdateGrid(requiredRows, requiredColumns);

            // Now you have a list of camera URLs
            foreach (var url in cameraUrls)
            {
                AddVideoView(url);
            }
        }

        private void UpdateGrid(int targetRows, int targetColumns)
        {
            // Get current counts
            int currentRows = VideoGrid.RowDefinitions.Count;
            int currentColumns = VideoGrid.ColumnDefinitions.Count;

            // Update rows
            if (targetRows > currentRows)
            {
                // Add missing rows
                for (int i = currentRows; i < targetRows; i++)
                {
                    VideoGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                }
            }
            else if (targetRows < currentRows)
            {
                // Remove extra rows only if empty (from bottom-up)
                for (int i = currentRows - 1; i >= targetRows; i--)
                {
                    if (IsRowEmpty(i))
                    {
                        VideoGrid.RowDefinitions.RemoveAt(i);
                    }
                    else
                    {
                        break; // Stop removing to avoid layout issues
                    }
                }
            }

            // Update columns
            if (targetColumns > currentColumns)
            {
                // Add missing columns
                for (int i = currentColumns; i < targetColumns; i++)
                {
                    VideoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }
            }
            else if (targetColumns < currentColumns)
            {
                // Remove extra columns only if empty (from right to left)
                for (int i = currentColumns - 1; i >= targetColumns; i--)
                {
                    if (IsColumnEmpty(i))
                    {
                        VideoGrid.ColumnDefinitions.RemoveAt(i);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        // Check if a row is empty
        private bool IsRowEmpty(int row)
        {
            foreach (Control child in VideoGrid.Children)
            {
                if (Grid.GetRow(child) == row)
                {
                    return false; // Row contains at least one element
                }
            }
            return true;
        }

        // Check if a column is empty
        private bool IsColumnEmpty(int column)
        {
            foreach (Control child in VideoGrid.Children)
            {
                if (Grid.GetColumn(child) == column)
                {
                    return false; // Column contains at least one element
                }
            }
            return true;
        }

        public void AddVideoView(string streamUrl)
        {
            int maxColumns = VideoGrid.ColumnDefinitions.Count;
            int maxRows = VideoGrid.RowDefinitions.Count;
            int totalSlots = maxRows * maxColumns;

            if (CameraAmount >= totalSlots)
            {
                //MessageBox.Show("Please increase grid size to add more cameras");
                return;
            }

            // Calculate position based on current camera count
            int row = CameraAmount / maxColumns;  // Integer division
            int column = CameraAmount % maxColumns;  // Remainder

            var videoView = new VideoView
            {
                Tag = streamUrl // Store the URL inside the VideoView
            };
            videoView.Loaded += VideoView_Loaded;

            Grid.SetRow(videoView, row);
            Grid.SetColumn(videoView, column);

            CameraAmount += 1;
            VideoGrid.Children.Add(videoView);
        }

        private void VideoView_Loaded(object sender, RoutedEventArgs e)
        {
            var videoView = sender as VideoView;
            if (videoView == null || videoView.Tag == null) return;

            string streamUrl = videoView.Tag.ToString();
            if (string.IsNullOrEmpty(streamUrl)) return;

            var _mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVlc);
            _mediaPlayer.AspectRatio = "16:9";
            _mediaPlayer.NetworkCaching = 3000;

            videoView.MediaPlayer = _mediaPlayer;

            var media = new Media(_libVlc, streamUrl, FromType.FromLocation);

            media.AddOption(":avcodec-hw=" + avcodec.ToString());

            _mediaPlayer.Play(media);
        }
    }
}