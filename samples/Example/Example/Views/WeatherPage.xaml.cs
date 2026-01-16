using Example.ViewModels;

namespace Example.Views;

public partial class WeatherPage : BaseContentPage<WeatherViewModel>
{
    public WeatherPage(WeatherViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }
}