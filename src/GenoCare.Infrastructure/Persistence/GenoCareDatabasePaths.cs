namespace GenoCare.Infrastructure.Persistence;

public static class GenoCareDatabasePaths
{
    public static string GetDefaultPath(string applicationName = "GenoCare", string? baseDirectory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationName);
        baseDirectory ??= Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);
        var directory = Path.Combine(baseDirectory, applicationName);
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "genocare.db");
    }
}
