namespace GenoCare.Infrastructure.Persistence;

public static class GenoCareDatabasePaths
{
    public static string GetDefaultPath(string applicationName = "GenoCare")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationName);
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), applicationName);
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "genocare.db");
    }
}
