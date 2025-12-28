using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LolStatsTracker.TrayApp.Services;

namespace LolStatsTracker.TrayApp.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly TrayAuthService _authService;

    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isLoading = false;

    public Action<bool>? LoginResultAction { get; set; }

    public LoginViewModel(TrayAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter username and password";
            return;
        }

        IsLoading = true;
        ErrorMessage = "";

        try
        {
            var result = await _authService.LoginAsync(Username, Password);
            if (result.Success)
            {
                LoginResultAction?.Invoke(true);
            }
            else
            {
                ErrorMessage = result.Error ?? "Invalid credentials";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
