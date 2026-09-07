using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TriviaBuilder.Models;

// Bindable editing model shown by the UI. Mapped to/from TriviaSetDto (Dto.cs)
// on load/save so JSON property names and UI-friendly names can differ.

public partial class TextItem : ObservableObject
{
    [ObservableProperty]
    public partial string Value { get; set; } = "";
}

public partial class Team : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = "";

    [ObservableProperty]
    public partial string Color { get; set; } = "#888888";
}

public partial class TriviaSettings : ObservableObject
{
    [ObservableProperty]
    public partial int QuestionSeconds { get; set; } = 20;

    [ObservableProperty]
    public partial int RevealSeconds { get; set; } = 6;

    [ObservableProperty]
    public partial int PointsDefault { get; set; } = 10;

    [ObservableProperty]
    public partial double SpeedBonusMax { get; set; } = 0.5;

    [ObservableProperty]
    public partial int InactiveLimit { get; set; } = 5;

    [ObservableProperty]
    public partial bool Shuffle { get; set; } = true;

    [ObservableProperty]
    public partial int MaxAnswers { get; set; } = 4;

    [ObservableProperty]
    public partial int StatsPageSeconds { get; set; } = 8;
}

public partial class Question : ObservableObject
{
    [ObservableProperty]
    public partial string Text { get; set; } = "";

    [ObservableProperty]
    public partial int Correct { get; set; }

    [ObservableProperty]
    public partial int? Points { get; set; }

    [ObservableProperty]
    public partial int? Seconds { get; set; }

    [ObservableProperty]
    public partial string? Fact { get; set; }

    // UI-only (not persisted, not part of dirty tracking): whether this question's
    // card starts open. Deliberately not [ObservableProperty] — see MainViewModel's
    // AttachDirtyTracking, which would otherwise flag expand/collapse as an edit.
    public bool IsExpanded { get; set; }

    public ObservableCollection<TextItem> Answers { get; } = new();

    public void RemoveAnswer(TextItem answer)
    {
        var idx = Answers.IndexOf(answer);
        if (idx < 0) return;
        Answers.RemoveAt(idx);
        if (Correct >= Answers.Count) Correct = Math.Max(0, Answers.Count - 1);
        else if (Correct > idx) Correct -= 1;
    }
}

public partial class TriviaSet : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; } = "";

    public ObservableCollection<Team> Teams { get; } = new();
    public TriviaSettings Settings { get; } = new();
    public ObservableCollection<Question> Questions { get; } = new();
    public ObservableCollection<TextItem> CorrectMessages { get; } = new();
    public ObservableCollection<TextItem> WrongMessages { get; } = new();
}
