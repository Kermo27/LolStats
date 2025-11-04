using MudBlazor;

namespace LolStatsTracker.Themes;

public static class AppTheme
{
    public static readonly MudTheme DarkTheme = new MudTheme()
    {
        PaletteDark = new PaletteDark()
        {
            Primary = "#3B82F6",          
            Secondary = "#F59E0B",        
            Background = "#0f0f13",       
            Surface = "#1a1a20",          
            AppbarBackground = "#111827",
            DrawerBackground = "#1a1a20",
            TextPrimary = "#E5E7EB",
            TextSecondary = "#9CA3AF",
            Info = "#38BDF8",
            Success = "#22C55E",
            Warning = "#FBBF24",
            Error = "#EF4444"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = new[] { "Inter", "Segoe UI", "Roboto", "sans-serif" }
            }
        }
    };

    public static readonly MudTheme LightTheme = new MudTheme()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#3B82F6",           
            Secondary = "#F59E0B",         
            Background = "#f9fafb",        
            Surface = "#ffffff",            
            AppbarBackground = "#ffffff",
            DrawerBackground = "#f3f4f6",
            TextPrimary = "#111827",
            TextSecondary = "#4B5563",
            Info = "#38BDF8",
            Success = "#22C55E",
            Warning = "#FBBF24",
            Error = "#EF4444"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = new[] { "Inter", "Segoe UI", "Roboto", "sans-serif" }
            }
        }
    };
}
