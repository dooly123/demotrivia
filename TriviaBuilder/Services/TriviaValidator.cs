using TriviaBuilder.Models;

namespace TriviaBuilder.Services;

// Bounds mirror the clamps in BasisTriviaGame.ParseConfig (Basis Silk/Assets/TriviaGame),
// the game that actually downloads and reads this JSON at QuestionsUrl. Out-of-range values
// there get silently clamped at runtime rather than rejected, so we flag them here instead
// of letting the saved file promise something the live game won't do.
public static class TriviaValidator
{
    public static List<string> Validate(TriviaSetDto data)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(data.Title)) errors.Add("Title is required.");

        if (data.Teams.Count == 0) errors.Add("At least one team is required.");
        for (var i = 0; i < data.Teams.Count; i++)
        {
            var t = data.Teams[i];
            if (string.IsNullOrWhiteSpace(t.Name)) errors.Add($"Team {i + 1}: name is required.");
            if (string.IsNullOrWhiteSpace(t.Color)) errors.Add($"Team {i + 1}: color is required.");
        }

        var s = data.Settings;
        if (s.QuestionSeconds is < 3 or > 300) errors.Add("Settings: question time must be between 3 and 300 seconds.");
        if (s.RevealSeconds is < 1 or > 60) errors.Add("Settings: reveal time must be between 1 and 60 seconds.");
        if (s.PointsDefault is < 1 or > 1000) errors.Add("Settings: default points must be between 1 and 1000.");
        if (s.SpeedBonusMax is < 0 or > 2) errors.Add("Settings: max speed bonus must be between 0 and 2.");
        if (s.InactiveLimit is < 0 or > 50) errors.Add("Settings: inactive limit must be between 0 and 50.");
        if (s.MaxAnswers < 2) errors.Add("Settings: max answers must be at least 2.");
        if (s.StatsPageSeconds is < 3 or > 60) errors.Add("Settings: stats page time must be between 3 and 60 seconds.");

        if (data.Questions.Count == 0) errors.Add("At least one question is required.");
        for (var i = 0; i < data.Questions.Count; i++)
        {
            var q = data.Questions[i];
            var n = i + 1;
            if (string.IsNullOrWhiteSpace(q.Q)) errors.Add($"Question {n}: text is required.");
            if (q.A.Count < 2)
            {
                errors.Add($"Question {n}: needs at least 2 answers.");
            }
            else
            {
                if (q.A.Count > s.MaxAnswers)
                    errors.Add($"Question {n}: has {q.A.Count} answers, more than settings.maxAnswers ({s.MaxAnswers}) — the game only shows the first {s.MaxAnswers} and drops the question entirely if \"correct\" points past that.");
                for (var ai = 0; ai < q.A.Count; ai++)
                    if (string.IsNullOrWhiteSpace(q.A[ai])) errors.Add($"Question {n}: answer {ai + 1} is empty.");
                if (q.Correct < 0 || q.Correct >= q.A.Count)
                    errors.Add($"Question {n}: correct index ({q.Correct}) is out of range for {q.A.Count} answers.");
            }
            if (q.Points is < 1 or > 1000) errors.Add($"Question {n}: points override must be between 1 and 1000.");
            if (q.Seconds is < 3 or > 300) errors.Add($"Question {n}: seconds override must be between 3 and 300.");
        }

        return errors;
    }
}
