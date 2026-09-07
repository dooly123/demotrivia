using TriviaBuilder.Models;

namespace TriviaBuilder.Services;

public record TriviaFileEntry(string FileName, string Title, int QuestionCount);

public class TriviaFileService
{
    public string DataDirectory { get; }

    public TriviaFileService(string dataDirectory)
    {
        DataDirectory = dataDirectory;
    }

    public List<TriviaFileEntry> ListFiles()
    {
        var result = new List<TriviaFileEntry>();
        if (!Directory.Exists(DataDirectory)) return result;

        foreach (var path in Directory.GetFiles(DataDirectory, "*.json").OrderBy(p => p))
        {
            string json;
            try { json = File.ReadAllText(path); }
            catch { continue; }

            if (!TriviaJson.LooksLikeTriviaSet(json)) continue;
            var dto = TriviaJson.TryParse(json);
            if (dto == null) continue;

            var name = Path.GetFileName(path);
            result.Add(new TriviaFileEntry(name, string.IsNullOrWhiteSpace(dto.Title) ? name : dto.Title, dto.Questions.Count));
        }
        return result;
    }

    public TriviaSet Load(string fileName)
    {
        var path = ResolvePath(fileName);
        var json = File.ReadAllText(path);
        var dto = TriviaJson.TryParse(json) ?? throw new InvalidDataException($"{fileName} is not a valid trivia set.");
        return TriviaMapper.ToEditable(dto);
    }

    public List<string> Validate(TriviaSet set) => TriviaValidator.Validate(TriviaMapper.ToDto(set));

    public void Save(string fileName, TriviaSet set)
    {
        var path = ResolvePath(fileName);
        File.WriteAllText(path, TriviaJson.Serialize(TriviaMapper.ToDto(set)));
    }

    public void Create(string fileName, TriviaSet set)
    {
        var path = ResolvePath(fileName);
        if (File.Exists(path)) throw new IOException($"{fileName} already exists.");
        File.WriteAllText(path, TriviaJson.Serialize(TriviaMapper.ToDto(set)));
    }

    public void Delete(string fileName)
    {
        var path = ResolvePath(fileName);
        if (File.Exists(path)) File.Delete(path);
    }

    public string EnsureJsonExtension(string name)
        => name.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? name : name + ".json";

    private string ResolvePath(string fileName)
    {
        var safe = Path.GetFileName(fileName);
        if (!safe.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("File name must end with .json");
        return Path.Combine(DataDirectory, safe);
    }
}
