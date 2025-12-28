using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using LolStatsTracker.TrayApp.ViewModels;

namespace LolStatsTracker.TrayApp.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        
        this.Loaded += (s, e) => 
        {
             if (DataContext is SettingsViewModel vm)
             {
                 vm.CloseAction = new Action(this.Close);
             }
        };
    }
    
    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}
