using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using ConwaysGameOfLife.Models;

namespace ConwaysGameOfLife.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private GameOfLife _gameOfLife;

    public WriteableBitmap Bitmap
    {
        get => _gameOfLife.Bitmap;
    }
    public int Generation
    {
        get => _gameOfLife.Generation;
    }
    public string GenerationText => $"{_gameOfLife.GenerationText}";
    public int Fps
    {
        get => _gameOfLife.Fps;
    }

    public string FpsText => $"{_gameOfLife.FpsText}";
    public int Population
    {
        get => _gameOfLife.Population;
    }

    public string PopulationText => $"{_gameOfLife.PopulationText}";

    public GameOfLife GameOfLife
    {
        get { return _gameOfLife; }
    }

    private void GameOfLife_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(GameOfLife.Generation):
                OnPropertyChanged(nameof(Generation));
                OnPropertyChanged(nameof(GenerationText));
                break;

            case nameof(GameOfLife.Fps):
                OnPropertyChanged(nameof(Fps));
                OnPropertyChanged(nameof(FpsText));
                break;

            case nameof(GameOfLife.Population):
                OnPropertyChanged(nameof(Population));
                OnPropertyChanged(nameof(PopulationText));
                break;

            case nameof(GameOfLife.Bitmap):
                OnPropertyChanged(nameof(Bitmap));
                break;
        }
    }

    public MainViewModel()
    {
        _gameOfLife = new GameOfLife();
        _gameOfLife.PropertyChanged += GameOfLife_PropertyChanged;
    }

    public void OnLeftClick(int x, int y)
    {
       _gameOfLife.HandleLeftClick(x, y);
    }

    public void OnRightClick(int x, int y)
    {
      _gameOfLife.HandleRightClick(x, y);
    }

    public void SimulationSpeed(double simulationSpeed)
    {
       _gameOfLife.SimulationSpeed(simulationSpeed); 
    }

    public void Reset()
    {
        _gameOfLife.Reset();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
