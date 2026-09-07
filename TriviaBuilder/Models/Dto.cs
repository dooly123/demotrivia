namespace TriviaBuilder.Models;

// Plain shapes matching trivia*.json exactly. Used only for file I/O;
// the editable UI model lives in Editable.cs and is mapped to/from these.

public class TriviaSetDto
{
    public string Title { get; set; } = "";
    public List<TeamDto> Teams { get; set; } = new();
    public SettingsDto Settings { get; set; } = new();
    public List<QuestionDto> Questions { get; set; } = new();
    public MessagesDto Messages { get; set; } = new();
}

public class TeamDto
{
    public string Name { get; set; } = "";
    public string Color { get; set; } = "";
}

public class SettingsDto
{
    public int QuestionSeconds { get; set; } = 20;
    public int RevealSeconds { get; set; } = 6;
    public int PointsDefault { get; set; } = 10;
    public double SpeedBonusMax { get; set; } = 0.5;
    public int InactiveLimit { get; set; } = 5;
    public bool Shuffle { get; set; } = true;
    public int MaxAnswers { get; set; } = 4;
    public int StatsPageSeconds { get; set; } = 8;
}

public class QuestionDto
{
    public string Q { get; set; } = "";
    public List<string> A { get; set; } = new();
    public int Correct { get; set; }
    public int? Points { get; set; }
    public int? Seconds { get; set; }
    public string? Fact { get; set; }
}

public class MessagesDto
{
    public List<string> Correct { get; set; } = new();
    public List<string> Wrong { get; set; } = new();
}
