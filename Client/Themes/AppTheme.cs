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
            DrawerBackground = "#111827",
            TextPrimary = "#E5E7EB",
            TextSecondary = "#9CA3AF",
            Info = "#38BDF8",
            Success = "#16A34A",
            Warning = "#FBBF24",
            Error = "#DC2626",
            LinesDefault = "rgba(156, 163, 175, 0.2)",
            TableLines = "rgba(156, 163, 175, 0.2)"
        },
        
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px"
        },
        
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = new[] { "Inter", "Segoe UI", "Roboto", "sans-serif" },
                LineHeight = "1.6"
            },
            H6 = new H6Typography() 
            { 
                FontSize = "1.0rem", 
                FontWeight = "600"
            },
        }
    };
    
    public static readonly MudTheme LightTheme = new MudTheme()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#3B82F6",           
            Secondary = "#F59E0B",         
            Background = "#F3F4F6",        
            Surface = "#ffffff",            
            AppbarBackground = "#ffffff",
            DrawerBackground = "#F9FAFB",
            TextPrimary = "#1F2937",
            TextSecondary = "#6B7280",
            Info = "#38BDF8",
            Success = "#16A34A",
            Warning = "#FBBF24",
            Error = "#DC2626",
            LinesDefault = "rgba(156, 163, 175, 0.5)",
            TableLines = "rgba(156, 163, 175, 0.5)"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = new[] { "Inter", "Segoe UI", "Roboto", "sans-serif" },
                LineHeight = "1.6"
            }
        }
    };
}