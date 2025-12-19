using System.Windows;
using LolStatsTracker.TrayApp.Services;

namespace LolStatsTracker.TrayApp.Views;

public partial class LoginWindow : Window
{
    private readonly TrayAuthService _authService;
    private bool _isRegisterMode;

    public bool LoginSuccessful { get; private set; }

    public LoginWindow(TrayAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private void ToggleMode_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _isRegisterMode = !_isRegisterMode;
        UpdateModeUI();
    }

    private void UpdateModeUI()
    {
        if (_isRegisterMode)
        {
            ModeText.Text = "Create a new account";
            SubmitButton.Content = "Register";
            ToggleText.Text = "Already have an account?";
            ToggleLink.Text = " Login here";
            
            EmailLabel.Visibility = Visibility.Visible;
            EmailBox.Visibility = Visibility.Visible;
            ConfirmPasswordLabel.Visibility = Visibility.Visible;
            ConfirmPasswordBox.Visibility = Visibility.Visible;
        }
        else
        {
            ModeText.Text = "Login to your account";
            SubmitButton.Content = "Login";
            ToggleText.Text = "Don't have an account?";
            ToggleLink.Text = " Register here";
            
            EmailLabel.Visibility = Visibility.Collapsed;
            EmailBox.Visibility = Visibility.Collapsed;
            ConfirmPasswordLabel.Visibility = Visibility.Collapsed;
            ConfirmPasswordBox.Visibility = Visibility.Collapsed;
        }
        
        ClearError();
    }

    private async void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        var username = UsernameBox.Text?.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Please enter username and password");
            return;
        }

        if (_isRegisterMode)
        {
            if (password.Length < 6)
            {
                ShowError("Password must be at least 6 characters");
                return;
            }

            if (password != ConfirmPasswordBox.Password)
            {
                ShowError("Passwords do not match");
                return;
            }
        }

        SetLoading(true);
        ClearError();

        try
        {
            if (_isRegisterMode)
            {
                var email = EmailBox.Text?.Trim();
                var (success, error) = await _authService.RegisterAsync(username, password, email);
                
                if (success)
                {
                    LoginSuccessful = true;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowError(error ?? "Registration failed");
                }
            }
            else
            {
                var (success, error) = await _authService.LoginAsync(username, password);
                
                if (success)
                {
                    LoginSuccessful = true;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowError(error ?? "Login failed");
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"An error occurred: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorBorder.Visibility = Visibility.Visible;
    }

    private void ClearError()
    {
        ErrorBorder.Visibility = Visibility.Collapsed;
        ErrorText.Text = string.Empty;
    }

    private void SetLoading(bool loading)
    {
        LoadingPanel.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;
        SubmitButton.IsEnabled = !loading;
        UsernameBox.IsEnabled = !loading;
        PasswordBox.IsEnabled = !loading;
        EmailBox.IsEnabled = !loading;
        ConfirmPasswordBox.IsEnabled = !loading;
    }
}
