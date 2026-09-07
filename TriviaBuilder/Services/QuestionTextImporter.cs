namespace TriviaBuilder.Services;

public record ImportedQuestion(string Text, List<string> Answers, int Correct);

public record ImportResult(List<ImportedQuestion> Questions, List<string> Warnings);

// Parses pasted plain text into questions: a question line (or lines), a blank
// line, then one answer per line. The first answer is treated as correct unless
// one is marked with a leading "*" (or "[x]"). Blocks are separated by another
// blank line, so multiple questions can be pasted at once.
public static class QuestionTextImporter
{
    public static ImportResult Parse(string input)
    {
        var warnings = new List<string>();
        var questions = new List<ImportedQuestion>();
        var paragraphs = SplitParagraphs(input);

        var pairCount = paragraphs.Count / 2;
        for (var i = 0; i < pairCount; i++)
        {
            var questionText = string.Join(' ', paragraphs[i * 2]).Trim();
            var answerLines = paragraphs[i * 2 + 1];

            if (string.IsNullOrWhiteSpace(questionText))
            {
                warnings.Add($"Block {i + 1}: empty question text, skipped.");
                continue;
            }

            var answers = new List<string>();
            var correct = 0;
            var markedCount = 0;
            foreach (var raw in answerLines)
            {
                var isMarked = TryStripCorrectMarker(raw.Trim(), out var line);
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (isMarked)
                {
                    markedCount++;
                    if (markedCount == 1) correct = answers.Count;
                }
                answers.Add(line);
            }

            if (answers.Count < 2)
            {
                warnings.Add($"\"{Truncate(questionText)}\": only {answers.Count} answer(s) found, needs at least 2 — skipped.");
                continue;
            }
            if (markedCount > 1) warnings.Add($"\"{Truncate(questionText)}\": more than one answer marked correct, used the first.");
            if (correct >= answers.Count) correct = 0;

            questions.Add(new ImportedQuestion(questionText, answers, correct));
        }

        if (paragraphs.Count % 2 != 0)
            warnings.Add("The last block has a question with no answer lines under it — ignored.");
        if (paragraphs.Count == 0)
            warnings.Add("Nothing recognizable yet — paste a question, a blank line, then one answer per line.");

        return new ImportResult(questions, warnings);
    }

    private static bool TryStripCorrectMarker(string line, out string stripped)
    {
        if (line.StartsWith("*")) { stripped = line[1..].Trim(); return true; }
        if (line.StartsWith("[x]", StringComparison.OrdinalIgnoreCase)) { stripped = line[3..].Trim(); return true; }
        if (line.StartsWith("[correct]", StringComparison.OrdinalIgnoreCase)) { stripped = line[9..].Trim(); return true; }
        stripped = line;
        return false;
    }

    private static string Truncate(string s) => s.Length <= 40 ? s : s[..40] + "…";

    // Consecutive non-blank lines form a paragraph; one or more blank lines separate them.
    private static List<List<string>> SplitParagraphs(string input)
    {
        var lines = input.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var paragraphs = new List<List<string>>();
        List<string>? current = null;
        foreach (var raw in lines)
        {
            var line = raw.TrimEnd();
            if (string.IsNullOrWhiteSpace(line))
            {
                if (current is { Count: > 0 }) paragraphs.Add(current);
                current = null;
                continue;
            }
            current ??= new List<string>();
            current.Add(line);
        }
        if (current is { Count: > 0 }) paragraphs.Add(current);
        return paragraphs;
    }
}
