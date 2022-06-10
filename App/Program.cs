namespace App;

public static class Program
{
    public static async Task Main()
    {
        Console.WriteLine("App starting");
        
        var app = new App();
        
        await app.Run();
        
        Console.WriteLine("App stopped");
    }
}

public class App
{
    public App()
    {
        Console.WriteLine("App created");
    }

    public async Task Run()
    {
        Console.InputEncoding = System.Text.Encoding.Unicode;
        Console.OutputEncoding = System.Text.Encoding.Unicode;
        Console.CancelKeyPress += (_, args) => args.Cancel = args.SpecialKey is ConsoleSpecialKey.ControlBreak;
        var wordList = GetWordsTxt();
        Console.WriteLine($"~~~ loaded {wordList.Count} words ~~~\n" +
                          $"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

        do
        {
            Console.Write("Enter word length (default 6):\n>");
            var wordLengthString = Console.ReadLine()?.Trim().ToLower();

            if (string.IsNullOrEmpty(wordLengthString) || !int.TryParse(wordLengthString, out var wordLength))
            {
                wordLength = 6;
            }

            Console.WriteLine($"Word length: {wordLength}\n");

            List<string> triedWordsList = new();
            List<string> lettersPositionsList = new();

            var attemptCount = 0;

            do
            {
                List<char> correctLettersChars = new();
                bool checkCorrectLetters = false;
                List<char> wrongLettersChars = new();
                bool checkWrongLetters = false;
                var correctLettersPositionsList = new List<string>();
                var wrongLettersPositionsList = new List<string>();

                if (attemptCount > 0)
                {
                    do
                    {
                        Console.WriteLine($"===================\n" +
                                          $"=== Attempt # {attemptCount} ===\n" +
                                          $"===================\n");
                        Console.Write("Enter word:\n>");
                        var word = Console.ReadLine()?.Trim().ToLower();
                        if (string.IsNullOrEmpty(word) || word.Length != wordLength)
                        {
                            Console.WriteLine($"Word must be of the same length as query text {wordLength}");
                            continue;
                        }

                        triedWordsList.Add(word);

                    } while (false);

                    Console.WriteLine($"tried words:\n  {string.Join("\n  ", triedWordsList)}\n" +
                                      $"~~~~~~~~~~~~~~\n");

                    Console.Write("Enter mask ('_' - wrong letter, '*' - wrong position, '+' - correct letter):\n>");
                    var mask = Console.ReadLine()?.Trim().ToLower();
                    lettersPositionsList.Add(string.IsNullOrEmpty(mask)
                        ? string.Concat(Enumerable.Repeat('_', wordLength))
                        : mask);

                    for (int i = 0; i < lettersPositionsList.Count; i++)
                    {
                        if (lettersPositionsList[i].Length < wordLength)
                        {
                            lettersPositionsList[i] += string.Concat(Enumerable.Repeat('_', wordLength - lettersPositionsList[i].Length));
                        }

                        lettersPositionsList[i] = lettersPositionsList[i][..wordLength];

                        var resolvedLetters = string.Empty;
                        var wrongLetters = string.Empty;
                        for (int j = 0; j < lettersPositionsList[i].Length; j++)
                        {
                            resolvedLetters += lettersPositionsList[i][j] == '+' ? triedWordsList[i][j] : '_';
                            wrongLetters += lettersPositionsList[i][j] == '*' ? triedWordsList[i][j] : '_';
                        }

                        correctLettersPositionsList.Add(resolvedLetters);
                        wrongLettersPositionsList.Add(wrongLetters);
                    }

                    Console.WriteLine($"Correct letters positions:\n  {string.Join("\n  ", correctLettersPositionsList)}\n" +
                                      $"~~~~~~~~~~~~~~\n");

                    Console.WriteLine($"Wrong letters positions:\n  {string.Join("\n  ", wrongLettersPositionsList)}\n" +
                                      $"~~~~~~~~~~~~~~\n");

                    correctLettersChars = (string.Concat(correctLettersPositionsList.SelectMany(s => s.Replace("_", ""))) +
                                           string.Concat(wrongLettersPositionsList.SelectMany(s => s.Replace("_", "")))).Distinct().ToList();
                    checkCorrectLetters = correctLettersChars is { Count: > 0 };
                    Console.WriteLine($"Correct letters: {string.Concat(correctLettersChars)}");

                    wrongLettersChars = triedWordsList.SelectMany(s => s).Distinct().Except(correctLettersChars).ToList();
                    checkWrongLetters = wrongLettersChars is { Count: > 0 };
                    Console.WriteLine($"Wrong letters: {string.Concat(wrongLettersChars)}");
                    Console.WriteLine();
                }

                var result = new List<string>();
                foreach (var word in wordList.Where(w => w.Length == wordLength))
                {
                    var wordChars = word.Distinct().ToList();

                    if (checkCorrectLetters && correctLettersChars.Any(c => !wordChars.Contains(c))
                        || checkWrongLetters && wrongLettersChars.Any(c => wordChars.Contains(c)))
                    {
                        continue;
                    }

                    bool addWord = true;
                    for (int i = 0; i < wordLength; i++)
                    {
                        if (correctLettersPositionsList.Any(correctLetters =>
                                correctLetters[i] != '_' && correctLetters[i] != word[i] ||
                                wrongLettersPositionsList.Any(w => w[i] == word[i])))
                        {
                            addWord = false;
                        }

                        if (!addWord)
                        {
                            break;
                        }
                    }

                    if (addWord)
                    {
                        result.Add(word);
                    }
                }

                var n = Math.Max(3, (Console.BufferWidth + 1) / (wordLength + 1));
                var m = Math.Max(1, Console.WindowHeight - 9);
                if (n * m > result.Count)
                {
                    n = (int)Math.Ceiling((double)result.Count / m);
                }

                for (int y = 0; y < m; y++)
                {
                    List<int> s = new List<int>();
                    for (int x = 0; x < n; x++)
                    {
                        var i = x + y * n;
                        if (i >= result.Count)
                        {
                            break;
                        }

                        s.Add(i);
                    }

                    Console.WriteLine(string.Join(" ", s.Select(k => result[k])));
                }

                Console.WriteLine("Displaying " + Math.Min(n * m, result.Count) + " of " + result.Count + " results\n");

                attemptCount++;
                Console.WriteLine("Press Enter to continue. Press 'r' to restart. Press 'q' to quit.");
                var done = false;
                do
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key, key.KeyChar)
                    {
                        case (ConsoleKey.Enter, _):
                            done = true;
                            break;
                        case (ConsoleKey.R, _):
                        case (_, 'к'):
                        case (_, 'К'):
                            attemptCount = 0;
                            done = true;
                            break;
                        case (ConsoleKey.Q, _):
                        case (_, 'й'):
                        case (_, 'Й'):
                            return;
                    }
                } while (!done);
            } while (attemptCount > 0);
        } while (true);
    }

    private List<string> GetWordsTxt()
    {
        return new DirectoryInfo("data").GetFiles()
            .SelectMany(f => File.ReadAllLines(f.FullName))
            .Select(w => w.ToLower())
            .Distinct()
            .ToList();
    }
}
