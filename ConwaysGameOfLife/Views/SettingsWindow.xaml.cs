using System.Windows;
using ConwaysGameOfLife.ViewModels;

namespace ConwaysGameOfLife.Views;

public partial class SettingsWindow : Window
{
    private MainViewModel _viewModel;
    public SettingsWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        _viewModel = mainViewModel;
        DataContext = _viewModel;
    }

    private void OnClick_Random(object sender, RoutedEventArgs e)
    {
        _viewModel.GameOfLife.SetPattern(Models.enums.STARTING_LAYOUT.RANDOM);
    }

    private void OnClick_StartStop(object sender, RoutedEventArgs e)
    {
        _viewModel.GameOfLife.IsRunning = _viewModel.GameOfLife.IsRunning ? false : true;
    }

    private void OnClick_Clear(object sender, RoutedEventArgs e)
    {
        _viewModel.GameOfLife.Clear();
    }

    private void OnClick_Calculator(object sender, RoutedEventArgs e)
    {
        _viewModel.GameOfLife.SetPattern(Models.enums.STARTING_LAYOUT.CALCULATOR);
    }

    private void OnClick_Turing(object sender, RoutedEventArgs e)
    {
        _viewModel.GameOfLife.SetPattern(Models.enums.STARTING_LAYOUT.TURING);
    }

    private void OnClick_Omaton(object sender, RoutedEventArgs e)
    {
        _viewModel.GameOfLife.SetPattern(Models.enums.STARTING_LAYOUT.OMATON);
    }

    private void OnClick_Corder(object sender, RoutedEventArgs e)
    {
        _viewModel.GameOfLife.SetPattern(Models.enums.STARTING_LAYOUT.CORDER);
    }

    private void OnClick_Gun(object sender, RoutedEventArgs e)
    {
        _viewModel.GameOfLife.SetPattern(Models.enums.STARTING_LAYOUT.GUNS);
    }

    private void OnClick_Spiral(object sender, RoutedEventArgs e)
    {
        _viewModel.GameOfLife.SetPattern(Models.enums.STARTING_LAYOUT.SPIRAL);
    }

    private void OnClick_ImportRle(object sender, RoutedEventArgs e)
    {
        string rle = RleInput.Text;

        if (!int.TryParse(XOffsetInput.Text, out int xOffset))
            xOffset = 0;

        if (!int.TryParse(YOffsetInput.Text, out int yOffset))
            yOffset = 0;

        if (string.IsNullOrWhiteSpace(rle))
        {
            MessageBox.Show("RLE input is empty.", "Import Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Zakładam, że masz jakąś metodę do wstawiania RLE
        // np. GameOfLife.ImportRle(rle, xOffset, yOffset);

        _viewModel.GameOfLife.SetCustomPattern(xOffset,yOffset,rle);
    }


}
