using System.IO;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LolStatsTracker.TrayApp.Views;

public partial class SettingsWindow : Window
{
    private readonly string _settingsPath = "appsettings.json";
    
    public SettingsWindow()
    {
        InitializeComponent();
        LoadSettings();
    }
    
    private void LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath))
                return;
            
            var json = File.ReadAllText(_settingsPath);
            var settings = JObject.Parse(json);
            
            var appConfig = settings["AppConfiguration"];
            if (appConfig != null)
            {
                ApiUrlTextBox.Text = appConfig["ApiBaseUrl"]?.Value<string>() ?? "";
                AutoStartCheckBox.IsChecked = appConfig["AutoStartWithWindows"]?.Value<bool>() ?? false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading settings: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            TestConnectionButton.IsEnabled = false;
            TestConnectionButton.Content = "Testing...";
            
            if (string.IsNullOrWhiteSpace(ApiUrlTextBox.Text))
            {
                MessageBox.Show("Please enter an API URL", "Validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            using var httpClient = new System.Net.Http.HttpClient();
            httpClient.BaseAddress = new Uri(ApiUrlTextBox.Text);
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            
            // Test with the auth endpoint (doesn't require authentication)
            var response = await httpClient.GetAsync("/swagger/index.html");
            
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Connection successful! API is reachable.", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"API responded with: {response.StatusCode}", "Warning", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Connection failed: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            TestConnectionButton.IsEnabled = true;
            TestConnectionButton.Content = "Test Connection";
        }
    }
    
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(ApiUrlTextBox.Text))
            {
                MessageBox.Show("Please enter an API URL", "Validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Load existing settings
            var json = File.ReadAllText(_settingsPath);
            var settings = JObject.Parse(json);
            
            // Update values
            if (settings["AppConfiguration"] == null)
                settings["AppConfiguration"] = new JObject();
            
            settings["AppConfiguration"]!["ApiBaseUrl"] = ApiUrlTextBox.Text.TrimEnd('/');
            settings["AppConfiguration"]!["AutoStartWithWindows"] = AutoStartCheckBox.IsChecked ?? false;
            
            // Save
            File.WriteAllText(_settingsPath, settings.ToString(Formatting.Indented));
            
            MessageBox.Show("Settings saved successfully!\n\nPlease restart the application for changes to take effect.", 
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
