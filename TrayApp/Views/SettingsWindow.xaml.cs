using System.Windows;
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
}
