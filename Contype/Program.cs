using System.Diagnostics;

var textProvider = new TextProvider();

var stopwatch = new Stopwatch();
var totalWordCount = 0d;
var totalTime = TimeSpan.Zero;

while (true)
{
    var paragraph = textProvider.GetNextParagraph();
    var errorCount = 0;

    var cursorPosition = Console.GetCursorPosition();
    TextStyles.Uncompleted.Write(paragraph);
    Console.SetCursorPosition(cursorPosition.Left, cursorPosition.Top);

    var nextIndex = 0;

    while (true)
    {
        var keyChar = ReadCharKey();

        stopwatch.Start();

        if (keyChar == paragraph[nextIndex])
        {
            TextStyles.Completed.Write(keyChar);

            nextIndex++;
            if (nextIndex == paragraph.Length)
                break;
        }
        else
        {
            errorCount++;

            // TODO: replace with int + ANSI codes
            var errorPositions = new List<(int Left, int Top)>
            {
                Console.GetCursorPosition(),
            };

            TextStyles.Error.Write(paragraph[nextIndex]);

            while (true)
            {
                if (ReadCharKey(orBackspace: true) == '\b')
                {
                    var previousPosition = errorPositions[^1];
                    Console.SetCursorPosition(previousPosition.Left, previousPosition.Top);

                    var charIndex = nextIndex + errorPositions.Count - 1;
                    TextStyles.Uncompleted.Write(charIndex < paragraph.Length ? paragraph[charIndex] : ' ');

                    Console.SetCursorPosition(previousPosition.Left, previousPosition.Top);

                    errorPositions.RemoveAt(errorPositions.Count - 1);
                    if (errorPositions.Count == 0)
                        break;
                }
                else
                {
                    errorPositions.Add(Console.GetCursorPosition());
                    var charIndex = nextIndex + errorPositions.Count - 1;
                    TextStyles.Error.Write(charIndex < paragraph.Length ? paragraph[charIndex] : ' ');
                }
            }
        }
    }

    stopwatch.Stop();

    var wordCount = paragraph.Length / 5d;
    var wordsPerMinute = wordCount / stopwatch.Elapsed.TotalMinutes;

    totalWordCount += wordCount;
    totalTime += stopwatch.Elapsed;
    var totalWordsPerMinute = totalWordCount / totalTime.TotalMinutes;

    stopwatch.Reset();

    Console.WriteLine();
    TextStyles.StatsLabel.Write("WPM ");
    TextStyles.StatsValue.Write(wordsPerMinute.ToString("N0"));
    TextStyles.StatsLabel.Write(" (total ");
    TextStyles.StatsValue.Write(totalWordsPerMinute.ToString("N0"));
    TextStyles.StatsLabel.Write("), errors ");
    TextStyles.StatsValue.Write(errorCount.ToString("N0"));
    Console.WriteLine();
    Console.WriteLine();
}

static char ReadCharKey(bool orBackspace = false)
{
    while (true)
    {
        switch (Console.ReadKey(intercept: true).KeyChar)
        {
            case '\0' or '\r' or '\t':
            case '\b' when !orBackspace:
                continue;
            case var charValue:
                return charValue;
        }
    }
}
