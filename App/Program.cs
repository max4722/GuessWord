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
        var wordList = GetWordsTxt();
        Console.WriteLine($"~~~ loaded {wordList.Count} words ~~~\n" +
                          $"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

        do
        {
            List<string> triedWordsList;
            Console.Write("1/3. Enter word length (default 6):\n>");
            var wordLengthString = Console.ReadLine()?.Trim().ToLower();

            if (string.IsNullOrEmpty(wordLengthString) || !int.TryParse(wordLengthString, out var wordLength))
            {
                wordLength = 6;
            }

            Console.WriteLine($"Word length: {wordLength}\n");


            do
            {
                Console.Write("2/3. Enter tried words:\n>");
                var triedWords = Console.ReadLine()?.Trim().ToLower();
                triedWordsList = string.IsNullOrEmpty(triedWords)
                    ? new List<string>()
                    : triedWords.Split(",").ToList();
                if (triedWordsList.Any(i => i.Length != wordLength))
                {
                    Console.WriteLine($"Tried words must be of the same length as query text {wordLength}");
                    continue;
                }
                
                break;
            } while (false);

            Console.WriteLine($"tried words:\n  {string.Join("\n  ", triedWordsList)}\n" +
                              $"~~~~~~~~~~~~~~\n");


            Console.Write("3/3. Enter letters positions:\n>");
            var lettersPositions = Console.ReadLine()?.Trim().ToLower();
            List<string> lettersPositionsList = string.IsNullOrEmpty(lettersPositions)
                ? new List<string> { string.Concat(Enumerable.Repeat('_', wordLength)) }
                : lettersPositions.Split(",").ToList();

            if (lettersPositionsList.Count > triedWordsList.Count)
            {
                lettersPositionsList =lettersPositionsList.Take(triedWordsList.Count).ToList();
            }
            
            while (lettersPositionsList.Count < triedWordsList.Count)
            {
                lettersPositionsList.Add(string.Concat(Enumerable.Repeat('_', wordLength)));
            }

            var resolvedLettersPositionsList = new List<string>();
            var wrongLettersPositionsList = new List<string>();
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
                    resolvedLetters +=  lettersPositionsList[i][j] == '+' ? triedWordsList[i][j] : '_';
                    wrongLetters += lettersPositionsList[i][j] == '*' ? triedWordsList[i][j] : '_';
                }

                resolvedLettersPositionsList.Add(resolvedLetters);
                wrongLettersPositionsList.Add(wrongLetters);
            }

            Console.WriteLine($"Resolved letters positions:\n  {string.Join("\n  ", resolvedLettersPositionsList)}\n" +
                              $"~~~~~~~~~~~~~~\n");

            Console.WriteLine($"Wrong letters positions:\n  {string.Join("\n  ", wrongLettersPositionsList)}\n" +
                              $"~~~~~~~~~~~~~~\n");

            var requiredLettersChars = (string.Concat(resolvedLettersPositionsList.SelectMany(s => s.Replace("_", ""))) +
                                        string.Concat(wrongLettersPositionsList.SelectMany(s => s.Replace("_", "")))).Distinct().ToList();
            var useRequiredLetters = requiredLettersChars is { Count: >0 };
            Console.WriteLine($"required letters: {string.Concat(requiredLettersChars)}");

            var forbiddenLettersChars = triedWordsList.SelectMany(s => s).Distinct().Except(requiredLettersChars).ToList();
            var useForbiddenLetters = forbiddenLettersChars is { Count: >0 };
            Console.WriteLine($"forbidden letters: {string.Concat(forbiddenLettersChars)}");
            Console.WriteLine();

            var result = new List<string>();
            foreach (var word in wordList.Where(w => w.Length == wordLength))
            {
                var wordChars = word.Distinct().ToList();
    
                if (useRequiredLetters && requiredLettersChars.Any(c => !wordChars.Contains(c))
                    || useForbiddenLetters && forbiddenLettersChars.Any(c => wordChars.Contains(c)))
                {
                    continue;
                }

                bool addWord = true;
                for (int i = 0; i < wordLength; i++)
                {
                    if (resolvedLettersPositionsList.Any(resolvedLetters => 
                            resolvedLetters[i] != '_' && resolvedLetters[i] != word[i] || 
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
            var m = Math.Max(1, Console.WindowHeight - 7);
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
