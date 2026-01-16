using System.Collections.ObjectModel;
using Example.Models;

namespace Example.ViewModels;

public class WeatherViewModel : BaseViewModel
{
    public WeatherViewModel()
    {
        RefreshCommand = new Command(OnRefresh);
        CurrentWeather = GenerateCurrentWeather();
    }

    private async void OnRefresh()
    {
        IsRefreshing = true;
        OnPropertyChanged(nameof(IsRefreshing));
        await Task.Delay(3000);

        IsRefreshing = false;
        OnPropertyChanged(nameof(IsRefreshing));

        await Task.Delay(250);

        CurrentWeather = GenerateCurrentWeather();
        WeatherItems = new ObservableCollection<Weather>(GenerateWeatherForDays(5));

        OnPropertyChanged(nameof(CurrentWeather));
        OnPropertyChanged(nameof(WeatherItems));
    }

    private Weather _currentWeather;
    public Weather CurrentWeather
    {
        get => _currentWeather;
        set => Set(ref _currentWeather, value);
    }

    public bool IsRefreshing { get; set; }
    public ObservableCollection<Weather> WeatherItems { get; set; } = new ObservableCollection<Weather>();

    public Command RefreshCommand { get; }

    public override void OnAppearing()
    {
        Task.Run(async () =>
        {
            if (WeatherItems.Any())
                return;

            CurrentWeather = GenerateCurrentWeather();
            WeatherItems = new ObservableCollection<Weather>(GenerateWeatherForDays(5));
            await Task.Delay(250);
            OnPropertyChanged(nameof(WeatherItems));
        });
    }

    private List<Weather> GenerateWeatherForDays(int days)
    {
        var random = new Random();
        var weatherTypes = Enum.GetValues(typeof(WeatherType)).Cast<WeatherType>().ToArray();
        var weatherList = new List<Weather>();

        for (int i = 1; i <= days; i++)
        {
            var weather = new Weather
            {
                Date = DateTimeOffset.Now.AddDays(i),
                Type = weatherTypes[random.Next(weatherTypes.Length)],
                CurrentTemperature = random.Next(-10, 35),
                MinTemperature = random.Next(-10, 20),
                MaxTemperature = random.Next(20, 40),
                Wind = random.Next(0, 50)
            };
            weatherList.Add(weather);
        }

        return weatherList;
    }

    private Weather GenerateCurrentWeather()
    {
        var random = new Random();
        var weatherTypes = Enum.GetValues(typeof(WeatherType)).Cast<WeatherType>().ToArray();

        return new Weather
        {
            Date = DateTimeOffset.Now,
            Type = weatherTypes[random.Next(weatherTypes.Length)],
            CurrentTemperature = random.Next(-10, 35),
            MinTemperature = random.Next(-10, 20),
            MaxTemperature = random.Next(20, 40),
            Wind = random.Next(0, 50)
        };
    }
}
