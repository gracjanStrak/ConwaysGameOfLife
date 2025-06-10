using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ConwaysGameOfLife;

public partial class SettingsWindow : Window
{
    private MainViewModel _viewModel;
    public SettingsWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        _viewModel = mainViewModel;
        DataContext = _viewModel;
    }

    private void OnClick_Reset(object sender, RoutedEventArgs e)
    {
        _viewModel.Reset();
    }

    private void OnClick_StartStop(object sender, RoutedEventArgs e)
    {
        _viewModel.IsRunning = _viewModel.IsRunning ? false : true;
    }

    private void OnClick_Clear(object sender, RoutedEventArgs e)
    {
        _viewModel.Clear();
    }

}
