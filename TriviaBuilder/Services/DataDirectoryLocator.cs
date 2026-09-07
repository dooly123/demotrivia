namespace TriviaBuilder.Services;

public static class DataDirectoryLocator
{
    // Walks up from the running exe looking for the folder that holds the
    // trivia*.json files, so this works both via `dotnet run` (cwd = project
    // dir) and a built exe several levels under bin/Debug/... .
    public static string Resolve()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
        {
            if (dir.GetFiles("trivia*.json").Length > 0) return dir.FullName;
        }
        return AppContext.BaseDirectory;
    }
}
