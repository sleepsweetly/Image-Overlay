using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SimpleOverlay;

public partial class MainWindow : Window
{
    private OverlayWindow? _overlayWindow;

    public MainWindow()
    {
        InitializeComponent();
        MouseLeftButtonDown += (_, __) => DragMove();
        KeyDown += MainWindow_KeyDown;
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        // Ctrl+M ile move mode toggle
        if (e.Key == Key.M && Keyboard.Modifiers == ModifierKeys.Control)
        {
            BtnMoveMode.IsChecked = !BtnMoveMode.IsChecked;
            e.Handled = true;
        }
        // Escape ile move mode kapat
        else if (e.Key == Key.Escape)
        {
            BtnMoveMode.IsChecked = false;
            e.Handled = true;
        }
    }

    private void BtnLoad_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "Select Image",
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff;*.webp|All Files|*.*"
        };
        
        if (dlg.ShowDialog() == true)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(dlg.FileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                
                if (_overlayWindow == null)
                {
                    _overlayWindow = new OverlayWindow();
                    _overlayWindow.Owner = this;
                    _overlayWindow.Show();
                }
                
                _overlayWindow.SetImage(bitmap);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void BtnMoveMode_Checked(object sender, RoutedEventArgs e)
    {
        _overlayWindow?.SetMoveMode(true);
        BtnMoveMode.Content = "Normal Mode";
    }

    private void BtnMoveMode_Unchecked(object sender, RoutedEventArgs e)
    {
        _overlayWindow?.SetMoveMode(false);
        BtnMoveMode.Content = "Move Mode";
    }

    private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_overlayWindow != null)
        {
            _overlayWindow.Opacity = e.NewValue;
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        _overlayWindow?.Close();
        Close();
    }
}