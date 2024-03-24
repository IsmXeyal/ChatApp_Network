using ClientSide.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ClientSide.Views;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel(this);
    }
    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
}
