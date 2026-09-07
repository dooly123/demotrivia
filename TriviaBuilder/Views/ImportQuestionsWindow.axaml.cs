using Avalonia.Controls;
using Avalonia.Interactivity;
using TriviaBuilder.Services;

namespace TriviaBuilder.Views;

public partial class ImportQuestionsWindow : Window
{
    private ImportResult lastResult = new([], []);

    public ImportQuestionsWindow()
    {
        InitializeComponent();
        PasteBox.TextChanged += (_, _) => UpdatePreview();
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        lastResult = QuestionTextImporter.Parse(PasteBox.Text ?? "");
        var lines = new List<string>
        {
            lastResult.Questions.Count == 1 ? "1 question found." : $"{lastResult.Questions.Count} questions found.",
        };
        lines.AddRange(lastResult.Warnings);
        SummaryText.Text = string.Join("\n", lines);
        ImportButton.IsEnabled = lastResult.Questions.Count > 0;
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e) => Close(null);

    private void Import_Click(object? sender, RoutedEventArgs e) => Close(lastResult.Questions);
}
