using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ConwaysGameOfLife
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        private double zoomFactor = 1.5;
        private const double ZoomStep = 1.5;
        private const double ZoomMin = 1.5;
        private const double ZoomMax = 100.0;

        private double simulationSpeed = 200;
        private double simulationSpeedStep = 25;

        private Point lastMousePosition;
        private bool isPanning = false;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;
            var pos = e.GetPosition(image);

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {

                double actualWidth = image.ActualWidth;
                double actualHeight = image.ActualHeight;

                double pixelWidth = _viewModel.GameOfLife.Bitmap.PixelWidth;
                double pixelHeight = _viewModel.GameOfLife.Bitmap.PixelHeight;

                int x = (int)(pos.X * pixelWidth / actualWidth);
                int y = (int)(pos.Y * pixelHeight / actualHeight);

                _viewModel.OnLeftClick(x, y);
            }

            if(Mouse.RightButton == MouseButtonState.Pressed)
            {
                double actualWidth = image.ActualWidth;
                double actualHeight = image.ActualHeight;

                double pixelWidth = _viewModel.GameOfLife.Bitmap.PixelWidth;
                double pixelHeight = _viewModel.GameOfLife.Bitmap.PixelHeight;

                int x = (int)(pos.X * pixelWidth / actualWidth);
                int y = (int)(pos.Y * pixelHeight / actualHeight);

                _viewModel.OnRightClick(x, y);
            }

            if (Mouse.MiddleButton == MouseButtonState.Pressed)
            {
                isPanning = true;
                pos = e.GetPosition(this);
                lastMousePosition = pos;
            }

           
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(Mouse.MiddleButton == MouseButtonState.Released)
            {
                isPanning = false;
            }
            
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.GameOfLife.IsRunning = !vm.GameOfLife.IsRunning;
                }
            }

            if(e.Key == Key.R)
            {
                _viewModel.GameOfLife.Reset();
            }

            if (e.Key == Key.F1)
            {
                SettingsButton_Click(sender, e);
            }
        }

        private void GameImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                var image = (Image)sender;
                var cursorPosition = e.GetPosition(image);

                double absoluteX = (cursorPosition.X - GameTranslateTransform.X) / zoomFactor;
                double absoluteY = (cursorPosition.Y - GameTranslateTransform.Y) / zoomFactor;

                if (e.Delta > 0)
                    zoomFactor += ZoomStep;
                else
                    zoomFactor -= ZoomStep;

                zoomFactor = Math.Clamp(zoomFactor, ZoomMin, ZoomMax);

                GameScaleTransform.ScaleX = zoomFactor;
                GameScaleTransform.ScaleY = zoomFactor;

                // zooming on cursorPosition
                GameTranslateTransform.X = cursorPosition.X - absoluteX * zoomFactor;
                GameTranslateTransform.Y = cursorPosition.Y - absoluteY * zoomFactor;
            }

            else if(Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                if(e.Delta > 0)
                {
                    simulationSpeed -= simulationSpeedStep;
                } else
                {
                    simulationSpeed += simulationSpeedStep;
                }

                simulationSpeed = Math.Clamp(simulationSpeed, 10, 300);

                _viewModel.SimulationSpeed(simulationSpeed);
            }

           
        }

        private void GameImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                var pos = e.GetPosition(this); 

                Vector delta = pos - lastMousePosition;

                GameTranslateTransform.X += delta.X;
                GameTranslateTransform.Y += delta.Y;

                lastMousePosition = pos;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(DataContext as MainViewModel);
           
            settingsWindow.Show();
        }
    }
}
