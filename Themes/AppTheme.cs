using MudBlazor;

namespace LolStatsTracker.Themes;

public static class AppTheme
{
    public static readonly MudTheme DarkTheme = new MudTheme()
    {
        PaletteDark = new PaletteDark()
        {
            Primary = "#3B82F6",
            Background = "#0f0f13",
            Surface = "#1a1a20",
            AppbarBackground = "#111827",
            TextPrimary = "#E5E7EB",
            DrawerBackground = "#1a1a20"
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
            Background = "#f9fafb",
            Surface = "#ffffff",
            AppbarBackground = "#ffffff",
            TextPrimary = "#111827",
            DrawerBackground = "#f3f4f6"
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