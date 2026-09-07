using System.Text.Json;
using System.Text.Json.Serialization;
using TriviaBuilder.Models;

namespace TriviaBuilder.Services;

public static class TriviaJson
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static TriviaSetDto? TryParse(string json)
    {
        try { return JsonSerializer.Deserialize<TriviaSetDto>(json, ReadOptions); }
        catch { return null; }
    }

    public static bool LooksLikeTriviaSet(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            return root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("questions", out var q) && q.ValueKind == JsonValueKind.Array
                && root.TryGetProperty("teams", out var t) && t.ValueKind == JsonValueKind.Array;
        }
        catch { return false; }
    }

    private static string Compact<T>(T value) => JsonSerializer.Serialize(value, CompactOptions);

    // Mirrors the hand-written layout already used in trivia1-4.json: one compact
    // object per line for teams/questions, so edits stay small in git diffs.
    // (Field spacing inside each compact object is standard System.Text.Json
    // minified style rather than the original hand-typed "{ "a": 1 }" spacing.)
    public static string Serialize(TriviaSetDto data)
    {
        var lines = new List<string> { "{" };
        lines.Add($"  \"title\": {Compact(data.Title)},");

        lines.Add("  \"teams\": [");
        for (var i = 0; i < data.Teams.Count; i++)
            lines.Add($"    {Compact(data.Teams[i])}{(i < data.Teams.Count - 1 ? "," : "")}");
        lines.Add("  ],");

        lines.Add("  \"settings\": {");
        lines.Add($"    \"questionSeconds\": {Compact(data.Settings.QuestionSeconds)},");
        lines.Add($"    \"revealSeconds\": {Compact(data.Settings.RevealSeconds)},");
        lines.Add($"    \"pointsDefault\": {Compact(data.Settings.PointsDefault)},");
        lines.Add($"    \"speedBonusMax\": {Compact(data.Settings.SpeedBonusMax)},");
        lines.Add($"    \"inactiveLimit\": {Compact(data.Settings.InactiveLimit)},");
        lines.Add($"    \"shuffle\": {Compact(data.Settings.Shuffle)},");
        lines.Add($"    \"maxAnswers\": {Compact(data.Settings.MaxAnswers)},");
        lines.Add($"    \"statsPageSeconds\": {Compact(data.Settings.StatsPageSeconds)}");
        lines.Add("  },");

        lines.Add("  \"questions\": [");
        for (var i = 0; i < data.Questions.Count; i++)
            lines.Add($"    {Compact(data.Questions[i])}{(i < data.Questions.Count - 1 ? "," : "")}");
        lines.Add("  ],");

        lines.Add("  \"messages\": {");
        lines.Add($"    \"correct\": {Compact(data.Messages.Correct)},");
        lines.Add($"    \"wrong\": {Compact(data.Messages.Wrong)}");
        lines.Add("  }");

        lines.Add("}");
        return string.Join('\n', lines) + "\n";
    }
}
