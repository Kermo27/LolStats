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

    private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter && DataContext is LoginViewModel vm)
        {
            if (vm.LoginCommand.CanExecute(null))
            {
                vm.LoginCommand.Execute(null);
            }
        }
    }
}
