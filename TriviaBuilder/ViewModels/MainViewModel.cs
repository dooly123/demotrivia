using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TriviaBuilder.Models;
using TriviaBuilder.Services;

namespace TriviaBuilder.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private TriviaFileService fileService;
    private string? currentFileName;

    [ObservableProperty]
    public partial ObservableCollection<TriviaFileEntry> Files { get; set; } = new();

    [ObservableProperty]
    public partial TriviaFileEntry? SelectedFile { get; set; }

    [ObservableProperty]
    public partial TriviaSet? Current { get; set; }

    [ObservableProperty]
    public partial bool IsDirty { get; set; }

    [ObservableProperty]
    public partial string StatusText { get; set; } = "";

    [ObservableProperty]
    public partial ObservableCollection<string> Errors { get; set; } = new();

    public string DataDirectory { get; }

    public MainViewModel() : this(DataDirectoryLocator.Resolve()) { }

    public MainViewModel(string dataDirectory)
    {
        DataDirectory = dataDirectory;
        fileService = new TriviaFileService(dataDirectory);
        StatusText = $"Folder: {dataDirectory}";
        RefreshFiles();
    }

    partial void OnSelectedFileChanged(TriviaFileEntry? value)
    {
        if (value != null && value.FileName != currentFileName) LoadFile(value.FileName);
    }

    // Used by both the Open… file picker and drag-and-drop from Explorer.
    public void OpenExternalFile(string fullPath)
    {
        string? dir;
        string name;
        try
        {
            dir = Path.GetDirectoryName(fullPath);
            name = Path.GetFileName(fullPath);
        }
        catch (Exception ex)
        {
            StatusText = $"Could not open that file: {ex.Message}";
            return;
        }

        if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(name))
        {
            StatusText = "Could not open that file.";
            return;
        }

        string json;
        try { json = File.ReadAllText(fullPath); }
        catch (Exception ex)
        {
            StatusText = $"Could not read {name}: {ex.Message}";
            return;
        }

        if (!TriviaJson.LooksLikeTriviaSet(json))
        {
            StatusText = $"{name} doesn't look like a trivia set (needs \"questions\" and \"teams\" arrays).";
            return;
        }

        if (!string.Equals(dir, fileService.DataDirectory, StringComparison.OrdinalIgnoreCase))
            fileService = new TriviaFileService(dir);

        RefreshFiles(name);
    }

    private void RefreshFiles(string? selectFileName = null)
    {
        var entries = fileService.ListFiles();
        Files = new ObservableCollection<TriviaFileEntry>(entries);
        var target = entries.FirstOrDefault(f => f.FileName == (selectFileName ?? currentFileName))
                     ?? entries.FirstOrDefault();
        SelectedFile = target;
        if (target == null)
        {
            Current = null;
            currentFileName = null;
        }
    }

    private void LoadFile(string fileName)
    {
        try
        {
            var set = fileService.Load(fileName);
            AttachDirtyTracking(set);
            Current = set;
            currentFileName = fileName;
            IsDirty = false;
            Errors.Clear();
            StatusText = $"Loaded {fileName}.";
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to load {fileName}: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Save()
    {
        if (Current == null || currentFileName == null) return;
        var errors = fileService.Validate(Current);
        Errors = new ObservableCollection<string>(errors);
        if (errors.Count > 0)
        {
            StatusText = "Fix the errors below before saving.";
            return;
        }
        fileService.Save(currentFileName, Current);
        IsDirty = false;
        StatusText = $"Saved {currentFileName}.";
        RefreshFiles(currentFileName);
    }

    [RelayCommand]
    private void NewSet()
    {
        var name = fileService.EnsureJsonExtension(SuggestNextName());
        var set = new TriviaSet { Title = "New Trivia Set" };

        if (Current != null)
        {
            foreach (var t in Current.Teams) set.Teams.Add(new Team { Name = t.Name, Color = t.Color });
            set.Settings.QuestionSeconds = Current.Settings.QuestionSeconds;
            set.Settings.RevealSeconds = Current.Settings.RevealSeconds;
            set.Settings.PointsDefault = Current.Settings.PointsDefault;
            set.Settings.SpeedBonusMax = Current.Settings.SpeedBonusMax;
            set.Settings.InactiveLimit = Current.Settings.InactiveLimit;
            set.Settings.Shuffle = Current.Settings.Shuffle;
            set.Settings.MaxAnswers = Current.Settings.MaxAnswers;
            set.Settings.StatsPageSeconds = Current.Settings.StatsPageSeconds;
        }
        else
        {
            set.Teams.Add(new Team { Name = "Red", Color = "#E5484D" });
            set.Teams.Add(new Team { Name = "Blue", Color = "#3E63DD" });
            set.Teams.Add(new Team { Name = "Green", Color = "#30A46C" });
        }

        var q = new Question { Text = "", Correct = 0, IsExpanded = true };
        q.Answers.Add(new TextItem());
        q.Answers.Add(new TextItem());
        set.Questions.Add(q);
        set.CorrectMessages.Add(new TextItem { Value = "Nice!" });
        set.WrongMessages.Add(new TextItem { Value = "Not quite." });

        CreateAndLoad(name, set);
    }

    [RelayCommand]
    private void Duplicate()
    {
        if (Current == null) return;
        var name = fileService.EnsureJsonExtension(SuggestNextName());
        var copy = TriviaMapper.ToEditable(TriviaMapper.ToDto(Current));
        CreateAndLoad(name, copy);
    }

    private void CreateAndLoad(string name, TriviaSet set)
    {
        try
        {
            fileService.Create(name, set);
            RefreshFiles(name);
            StatusText = $"Created {name}.";
        }
        catch (Exception ex)
        {
            StatusText = $"Could not create {name}: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Delete()
    {
        if (currentFileName == null) return;
        var deleted = currentFileName;
        fileService.Delete(currentFileName);
        currentFileName = null;
        RefreshFiles();
        StatusText = $"Deleted {deleted}.";
    }

    private string SuggestNextName()
    {
        var numbers = Files
            .Select(f => Regex.Match(f.FileName, @"(\d+)(?=\.json$)"))
            .Where(m => m.Success)
            .Select(m => int.Parse(m.Value));
        var list = numbers.ToList();
        var next = (list.Count > 0 ? list.Max() : 0) + 1;
        return $"trivia{next}.json";
    }

    [RelayCommand]
    private void AddTeam() => Current?.Teams.Add(new Team { Name = "New Team", Color = "#888888" });

    [RelayCommand]
    public void RemoveTeam(Team team) => Current?.Teams.Remove(team);

    [RelayCommand]
    private void AddQuestion()
    {
        var q = new Question { Text = "", Correct = 0, IsExpanded = true };
        q.Answers.Add(new TextItem());
        q.Answers.Add(new TextItem());
        Current?.Questions.Add(q);
    }

    public void ImportQuestions(IReadOnlyList<ImportedQuestion> imported)
    {
        if (Current == null || imported.Count == 0) return;
        foreach (var iq in imported)
        {
            var q = new Question { Text = iq.Text, Correct = iq.Correct };
            foreach (var a in iq.Answers) q.Answers.Add(new TextItem { Value = a });
            Current.Questions.Add(q);
        }
        StatusText = $"Imported {imported.Count} question(s).";
    }

    [RelayCommand]
    public void RemoveQuestion(Question question) => Current?.Questions.Remove(question);

    [RelayCommand]
    public void MoveQuestionUp(Question question)
    {
        if (Current == null) return;
        var i = Current.Questions.IndexOf(question);
        if (i > 0) Current.Questions.Move(i, i - 1);
    }

    [RelayCommand]
    public void MoveQuestionDown(Question question)
    {
        if (Current == null) return;
        var i = Current.Questions.IndexOf(question);
        if (i >= 0 && i < Current.Questions.Count - 1) Current.Questions.Move(i, i + 1);
    }

    [RelayCommand]
    public void AddAnswer(Question question) => question.Answers.Add(new TextItem());

    [RelayCommand]
    public void RemoveAnswer(TextItem answer)
    {
        if (Current == null) return;
        foreach (var q in Current.Questions)
        {
            if (q.Answers.Contains(answer))
            {
                q.RemoveAnswer(answer);
                break;
            }
        }
    }

    [RelayCommand]
    private void AddCorrectMessage() => Current?.CorrectMessages.Add(new TextItem());

    [RelayCommand]
    private void AddWrongMessage() => Current?.WrongMessages.Add(new TextItem());

    [RelayCommand]
    public void RemoveCorrectMessage(TextItem item) => Current?.CorrectMessages.Remove(item);

    [RelayCommand]
    public void RemoveWrongMessage(TextItem item) => Current?.WrongMessages.Remove(item);

    private void AttachDirtyTracking(TriviaSet set)
    {
        void HookItem(INotifyPropertyChanged o) => o.PropertyChanged += (_, _) => IsDirty = true;

        void HookCollection<T>(ObservableCollection<T> col, Action<T>? onAdd = null)
            where T : INotifyPropertyChanged
        {
            foreach (var item in col)
            {
                HookItem(item);
                onAdd?.Invoke(item);
            }
            col.CollectionChanged += (_, e) =>
            {
                IsDirty = true;
                if (e.NewItems == null) return;
                foreach (T item in e.NewItems)
                {
                    HookItem(item);
                    onAdd?.Invoke(item);
                }
            };
        }

        HookItem(set);
        HookCollection(set.Teams);
        HookItem(set.Settings);
        HookCollection(set.Questions, q => HookCollection(q.Answers));
        HookCollection(set.CorrectMessages);
        HookCollection(set.WrongMessages);
    }
}
