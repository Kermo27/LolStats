using System.Windows;
using System.Windows.Controls;
using LolStatsTracker.TrayApp.Services;
using LolStatsTracker.TrayApp.ViewModels;

namespace LolStatsTracker.TrayApp.Views;

public partial class LoginWindow : Window
{
    public bool LoginSuccessful { get; set; } = false;

    public LoginWindow()
    {
        InitializeComponent();
    }
    
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.Password = ((PasswordBox)sender).Password;
        }
    }
}
