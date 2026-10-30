namespace McggCompanion.Windows.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class DesktopFactAttribute : FactAttribute
{
    public DesktopFactAttribute()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("MCGG_DESKTOP_TESTS"), "1", StringComparison.Ordinal))
        {
            Skip = "Capture desktop memerlukan sesi interaktif. Jalankan dengan MCGG_DESKTOP_TESTS=1.";
        }
    }
}
