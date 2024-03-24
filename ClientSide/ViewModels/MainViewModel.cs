using ClientSide.Views;

namespace ClientSide.ViewModels;

public class MainViewModel
{
    public MainView? MainView { get; set; }
    public MainViewModel(MainView mainView)
    {
        MainView = mainView;
    }
}