using MudBlazor;

namespace LolStatsTracker.Themes;

public static class AppTheme
{
    public static readonly MudTheme DarkTheme = new MudTheme()
    {
        PaletteDark = new PaletteDark()
        {
            Primary = "#3B82F6",          
            Secondary = "#8B5CF6",
            Tertiary = "#10B981",
            Background = "#08080c",
            Surface = "#12121a",
            AppbarBackground = "#0c0c14",
            DrawerBackground = "#0c0c14",
            DrawerText = "#94A3B8",
            TextPrimary = "#F1F5F9",
            TextSecondary = "#94A3B8",
            Info = "#38BDF8",
            Success = "#22C55E",
            Warning = "#F59E0B",
            Error = "#EF4444",
            LinesDefault = "rgba(148, 163, 184, 0.15)",
            TableLines = "rgba(148, 163, 184, 0.1)",
            ActionDefault = "#94A3B8",
            ActionDisabled = "rgba(148, 163, 184, 0.3)",
            Divider = "rgba(148, 163, 184, 0.12)"
        },
        
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px"
        },
        
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = new[] { "Inter", "Segoe UI", "Roboto", "sans-serif" },
                LineHeight = "1.6",
                LetterSpacing = "-0.01em"
            },
            H4 = new H4Typography()
            {
                FontSize = "1.75rem",
                FontWeight = "700",
                LetterSpacing = "-0.02em"
            },
            H5 = new H5Typography()
            {
                FontSize = "1.25rem",
                FontWeight = "600",
                LetterSpacing = "-0.01em"
            },
            H6 = new H6Typography() 
            { 
                FontSize = "1.0rem", 
                FontWeight = "600",
                LetterSpacing = "-0.01em"
            },
            Subtitle1 = new Subtitle1Typography()
            {
                FontSize = "0.95rem",
                FontWeight = "500"
            },
            Body1 = new Body1Typography()
            {
                FontSize = "0.9rem"
            }
        }
    };
    
    public static readonly MudTheme LightTheme = new MudTheme()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#3B82F6",           
            Secondary = "#8B5CF6",
            Tertiary = "#10B981",
            Background = "#F8FAFC",        
            Surface = "#ffffff",            
            AppbarBackground = "#ffffff",
            DrawerBackground = "#F1F5F9",
            TextPrimary = "#0F172A",
            TextSecondary = "#64748B",
            Info = "#0EA5E9",
            Success = "#22C55E",
            Warning = "#F59E0B",
            Error = "#EF4444",
            LinesDefault = "rgba(100, 116, 139, 0.2)",
            TableLines = "rgba(100, 116, 139, 0.15)",
            Divider = "rgba(100, 116, 139, 0.12)"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = new[] { "Inter", "Segoe UI", "Roboto", "sans-serif" },
                LineHeight = "1.6",
                LetterSpacing = "-0.01em"
            },
            H4 = new H4Typography()
            {
                FontSize = "1.75rem",
                FontWeight = "700",
                LetterSpacing = "-0.02em"
            },
            H5 = new H5Typography()
            {
                FontSize = "1.25rem",
                FontWeight = "600"
            },
            H6 = new H6Typography() 
            { 
                FontSize = "1.0rem", 
                FontWeight = "600"
            }
        }
    };
}