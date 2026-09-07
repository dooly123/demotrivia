using TriviaBuilder.Models;

namespace TriviaBuilder.Services;

public static class TriviaMapper
{
    public static TriviaSet ToEditable(TriviaSetDto dto)
    {
        var set = new TriviaSet { Title = dto.Title };

        foreach (var t in dto.Teams)
            set.Teams.Add(new Team { Name = t.Name, Color = t.Color });

        set.Settings.QuestionSeconds = dto.Settings.QuestionSeconds;
        set.Settings.RevealSeconds = dto.Settings.RevealSeconds;
        set.Settings.PointsDefault = dto.Settings.PointsDefault;
        set.Settings.SpeedBonusMax = dto.Settings.SpeedBonusMax;
        set.Settings.InactiveLimit = dto.Settings.InactiveLimit;
        set.Settings.Shuffle = dto.Settings.Shuffle;
        set.Settings.MaxAnswers = dto.Settings.MaxAnswers;
        set.Settings.StatsPageSeconds = dto.Settings.StatsPageSeconds;

        foreach (var q in dto.Questions)
        {
            var question = new Question
            {
                Text = q.Q,
                Correct = q.Correct,
                Points = q.Points,
                Seconds = q.Seconds,
                Fact = q.Fact,
            };
            foreach (var a in q.A) question.Answers.Add(new TextItem { Value = a });
            set.Questions.Add(question);
        }

        foreach (var m in dto.Messages.Correct) set.CorrectMessages.Add(new TextItem { Value = m });
        foreach (var m in dto.Messages.Wrong) set.WrongMessages.Add(new TextItem { Value = m });

        return set;
    }

    public static TriviaSetDto ToDto(TriviaSet set)
    {
        var dto = new TriviaSetDto { Title = set.Title };

        foreach (var t in set.Teams)
            dto.Teams.Add(new TeamDto { Name = t.Name, Color = t.Color });

        dto.Settings = new SettingsDto
        {
            QuestionSeconds = set.Settings.QuestionSeconds,
            RevealSeconds = set.Settings.RevealSeconds,
            PointsDefault = set.Settings.PointsDefault,
            SpeedBonusMax = set.Settings.SpeedBonusMax,
            InactiveLimit = set.Settings.InactiveLimit,
            Shuffle = set.Settings.Shuffle,
            MaxAnswers = set.Settings.MaxAnswers,
            StatsPageSeconds = set.Settings.StatsPageSeconds,
        };

        foreach (var q in set.Questions)
        {
            dto.Questions.Add(new QuestionDto
            {
                Q = q.Text,
                A = q.Answers.Select(a => a.Value).ToList(),
                Correct = q.Correct,
                Points = q.Points,
                Seconds = q.Seconds,
                Fact = string.IsNullOrWhiteSpace(q.Fact) ? null : q.Fact,
            });
        }

        dto.Messages.Correct = set.CorrectMessages.Select(m => m.Value).ToList();
        dto.Messages.Wrong = set.WrongMessages.Select(m => m.Value).ToList();

        return dto;
    }
}
