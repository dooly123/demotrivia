using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using TriviaBuilder.Models;
using TriviaBuilder.Services;
using TriviaBuilder.ViewModels;

namespace TriviaBuilder.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DragDrop.AddDropHandler(this, OnDrop);
        DragDrop.AddDragOverHandler(this, OnDragOver);
    }

    private MainViewModel? Vm => DataContext as MainViewModel;

    private async void OpenFile_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider == null || Vm is not { } vm) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open trivia JSON",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("Trivia JSON (*.json)") { Patterns = new[] { "*.json" } } },
        });

        var path = files.Count > 0 ? files[0].TryGetLocalPath() : null;
        if (!string.IsNullOrEmpty(path)) vm.OpenExternalFile(path);
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.DataTransfer.Formats.Contains(DataFormat.File) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        if (Vm is not { } vm) return;
        var file = e.DataTransfer.TryGetFiles()?.FirstOrDefault();
        var path = file?.Path.LocalPath;
        if (!string.IsNullOrEmpty(path)) vm.OpenExternalFile(path);
    }

    private void RemoveTeam_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: Team team } && Vm is { } vm) vm.RemoveTeam(team);
    }

    private void MoveQuestionUp_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: Question q } && Vm is { } vm) vm.MoveQuestionUp(q);
    }

    private void MoveQuestionDown_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: Question q } && Vm is { } vm) vm.MoveQuestionDown(q);
    }

    private void RemoveQuestion_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: Question q } && Vm is { } vm) vm.RemoveQuestion(q);
    }

    private async void PasteQuestions_Click(object? sender, RoutedEventArgs e)
    {
        if (Vm is not { } vm) return;
        var dialog = new ImportQuestionsWindow();
        var result = await dialog.ShowDialog<List<ImportedQuestion>?>(this);
        if (result is { Count: > 0 }) vm.ImportQuestions(result);
    }

    private void AddAnswer_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: Question q } && Vm is { } vm) vm.AddAnswer(q);
    }

    private void RemoveAnswer_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: TextItem item } && Vm is { } vm) vm.RemoveAnswer(item);
    }

    private void RemoveCorrectMessage_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: TextItem item } && Vm is { } vm) vm.RemoveCorrectMessage(item);
    }

    private void RemoveWrongMessage_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: TextItem item } && Vm is { } vm) vm.RemoveWrongMessage(item);
    }
}
