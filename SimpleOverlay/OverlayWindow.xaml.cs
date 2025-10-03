using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleOverlay;

public partial class OverlayWindow : Window
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED = 0x00080000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private bool _isDragging;
    private bool _isResizing;
    private Point _lastMousePos;
    private Point _resizeStartPos;
    private bool _moveMode;
    private double _initialWidth, _initialHeight;
    private double _aspectRatio;
    private bool _hasMovedMouse;

    public OverlayWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        SizeChanged += (s, e) => UpdateResizeHandlePosition();
        OverlayImage.SizeChanged += (s, e) => UpdateResizeHandlePosition();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Başlangıçta click-through aktif
        SetClickThrough(true);
    }

    public void SetImage(BitmapSource image)
    {
        OverlayImage.Source = image;
        
        // Aspect ratio'yu kaydet
        _aspectRatio = (double)image.PixelWidth / image.PixelHeight;
        
        // Resim boyutuna göre pencere boyutunu ayarla
        var maxWidth = SystemParameters.PrimaryScreenWidth * 0.8;
        var maxHeight = SystemParameters.PrimaryScreenHeight * 0.8;
        
        if (image.PixelWidth > maxWidth || image.PixelHeight > maxHeight)
        {
            // Resim çok büyükse orantılı küçült
            var scale = Math.Min(maxWidth / image.PixelWidth, maxHeight / image.PixelHeight);
            Width = image.PixelWidth * scale;
            Height = image.PixelHeight * scale;
        }
        else
        {
            // Resim uygun boyuttaysa orijinal boyutunu kullan
            Width = image.PixelWidth;
            Height = image.PixelHeight;
        }
        
        // Resmi pencere boyutuna ayarla (sol üst köşeye sabitlenmiş)
        OverlayImage.Width = Width;
        OverlayImage.Height = Height;
        
        // Ekranın ortasına yerleştir
        Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
        Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;
        
        // Resize handle pozisyonunu güncelle
        Dispatcher.BeginInvoke(() => UpdateResizeHandlePosition());
    }

    public void SetMoveMode(bool enabled)
    {
        _moveMode = enabled;
        
        if (enabled)
        {
            // Move mode: Click-through'u kapat, görsel feedback göster
            SetClickThrough(false);
            MoveBorder.Visibility = Visibility.Visible;
            ResizeCanvas.Visibility = Visibility.Visible;
            OverlayImage.Cursor = Cursors.SizeAll;
            UpdateResizeHandlePosition();
            UpdateMoveBorderSize(); // Border'ı resim boyutuna ayarla
        }
        else
        {
            // Normal mode: Click-through'u aç, görsel feedback'i gizle
            SetClickThrough(true);
            MoveBorder.Visibility = Visibility.Collapsed;
            ResizeCanvas.Visibility = Visibility.Collapsed;
            OverlayImage.Cursor = Cursors.Arrow;
        }
    }

    private void UpdateResizeHandlePosition()
    {
        if (OverlayImage.Source != null)
        {
            // Resim artık sol üst köşede sabit, pencere boyutunda
            // Handle'ı sağ alt köşeye yerleştir
            var handleX = Width - 25; // Handle boyutu 25x25
            var handleY = Height - 25;
            
            // Handle'ın pencere sınırları içinde kalmasını sağla
            handleX = Math.Min(handleX, Width - 30);
            handleY = Math.Min(handleY, Height - 30);
            handleX = Math.Max(5, handleX);
            handleY = Math.Max(5, handleY);
            
            Canvas.SetLeft(ResizeHandle, handleX);
            Canvas.SetTop(ResizeHandle, handleY);
        }
        
        // Move border'ı da resim boyutuna göre ayarla
        UpdateMoveBorderSize();
    }

    private void UpdateMoveBorderSize()
    {
        if (OverlayImage.Source != null)
        {
            // Resim artık sol üst köşede sabit, pencere boyutunda
            // Border'ı da aynı boyutta yap
            MoveBorder.Width = Width;
            MoveBorder.Height = Height;
            MoveBorder.Margin = new Thickness(0);
            MoveBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            MoveBorder.VerticalAlignment = VerticalAlignment.Stretch;
        }
    }

    private void SetClickThrough(bool enabled)
    {
        var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero) return;

        try
        {
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            
            if (enabled)
            {
                exStyle |= WS_EX_TRANSPARENT | WS_EX_LAYERED;
            }
            else
            {
                exStyle &= ~WS_EX_TRANSPARENT;
                exStyle |= WS_EX_LAYERED;
            }
            
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Click-through ayarlanamadı: {ex.Message}");
        }
    }

    // Resim sürükleme (sadece resim alanı)
    private void Image_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!_moveMode || e.ChangedButton != MouseButton.Left) return;
        
        _isDragging = true;
        _hasMovedMouse = false;
        _lastMousePos = PointToScreen(Mouse.GetPosition(this));
        OverlayImage.CaptureMouse();
        e.Handled = true;
    }

    private void Image_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging && _moveMode && e.LeftButton == MouseButtonState.Pressed)
        {
            var currentScreenPos = PointToScreen(Mouse.GetPosition(this));
            var deltaX = currentScreenPos.X - _lastMousePos.X;
            var deltaY = currentScreenPos.Y - _lastMousePos.Y;
            
            // Minimum hareket kontrolü
            if (!_hasMovedMouse && (Math.Abs(deltaX) > 2 || Math.Abs(deltaY) > 2))
            {
                _hasMovedMouse = true;
            }
            
            if (_hasMovedMouse)
            {
                Left += deltaX;
                Top += deltaY;
                _lastMousePos = currentScreenPos;
            }
        }
    }

    private void Image_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging && e.ChangedButton == MouseButton.Left)
        {
            _isDragging = false;
            _hasMovedMouse = false;
            OverlayImage.ReleaseMouseCapture();
        }
    }

    // Resize handle
    private void Resize_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!_moveMode || e.ChangedButton != MouseButton.Left) return;
        
        _isResizing = true;
        _resizeStartPos = PointToScreen(Mouse.GetPosition(this));
        _initialWidth = Width;
        _initialHeight = Height;
        
        ResizeHandle.CaptureMouse();
        e.Handled = true;
        
        // Resize başladığında renk değiştir
        UpdateResizeHandleColor();
    }

    private void Resize_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isResizing && _moveMode && e.LeftButton == MouseButtonState.Pressed)
        {
            // Mouse'un şu anki pozisyonunu al
            var currentMousePos = PointToScreen(Mouse.GetPosition(this));
            
            // Handle'ın şu anki pozisyonunu al
            var handleLeft = Canvas.GetLeft(ResizeHandle);
            var handleTop = Canvas.GetTop(ResizeHandle);
            var handleScreenPos = PointToScreen(new Point(handleLeft + 12.5, handleTop + 12.5));
            
            // Mouse ile handle arasındaki farkı hesapla
            var deltaX = currentMousePos.X - handleScreenPos.X;
            var deltaY = currentMousePos.Y - handleScreenPos.Y;
            
            double newWidth, newHeight;
            
            // Shift tuşu basılıysa proportional resize
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                // Diagonal mesafeyi kullan (proportional)
                var diagonal = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                var scale = deltaX > 0 || deltaY > 0 ? diagonal * 0.5 : -diagonal * 0.5;
                
                newWidth = Math.Max(200, Width + scale);
                newHeight = newWidth / _aspectRatio;
                
                // Minimum boyut kontrolü
                if (newHeight < 150)
                {
                    newHeight = 150;
                    newWidth = newHeight * _aspectRatio;
                }
            }
            else
            {
                // Free resize - mouse'u direkt takip et
                newWidth = Math.Max(200, Width + deltaX * 0.5);
                newHeight = Math.Max(150, Height + deltaY * 0.5);
            }
            
            Width = newWidth;
            Height = newHeight;
            
            // Resmi de aynı boyutta yap (sol üst köşeye sabitlenmiş)
            OverlayImage.Width = newWidth;
            OverlayImage.Height = newHeight;
            
            UpdateResizeHandlePosition();
            UpdateResizeHandleColor();
            
            // Border boyutunu da güncelle
            Dispatcher.BeginInvoke(() => UpdateMoveBorderSize());
        }
    }

    private void Resize_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isResizing && e.ChangedButton == MouseButton.Left)
        {
            _isResizing = false;
            ResizeHandle.ReleaseMouseCapture();
            
            // Normal renge döndür
            ResizeHandle.Stroke = new SolidColorBrush(Colors.Blue);
        }
    }

    private void UpdateResizeHandleColor()
    {
        if (_isResizing)
        {
            // Shift basılıysa yeşil (proportional), değilse kırmızı (free)
            var color = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? Colors.Green : Colors.Red;
            ResizeHandle.Stroke = new SolidColorBrush(color);
        }
    }

    private void ResizeHandle_MouseEnter(object sender, MouseEventArgs e)
    {
        if (!_isResizing)
        {
            // Handle'ı büyüt
            var scaleAnimation = new System.Windows.Media.Animation.DoubleAnimation(1.2, TimeSpan.FromMilliseconds(150));
            HandleScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            HandleScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }
    }

    private void ResizeHandle_MouseLeave(object sender, MouseEventArgs e)
    {
        if (!_isResizing)
        {
            // Handle'ı normale döndür
            var scaleAnimation = new System.Windows.Media.Animation.DoubleAnimation(1.0, TimeSpan.FromMilliseconds(150));
            HandleScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            HandleScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }
    }
}