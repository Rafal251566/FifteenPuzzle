using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

internal class Program
{

    private static Dictionary<char, (int row, int col, string move)> moveDictionary = new Dictionary<char, (int, int, string)>
    {
        {'U', (-1, 0, "U")},
        {'D', (1, 0, "D")},
        {'L', (0, -1, "L")},
        {'R', (0, 1, "R")}
    };

    private static Dictionary<char, char> swingMoves = new Dictionary<char, char>
    {
        {'U', 'D'},
        {'D', 'U'},
        {'L', 'R'},
        {'R', 'L'},
        {' ', ' '}
    };

    public static String NazwaPliku = "4x4_07_00201.txt";


    private static void Main(string[] args)
    {
        string filepath = Path.Combine(Directory.GetCurrentDirectory(), "4x4_07_00201.txt");

        if (!File.Exists(filepath))
        {
            Console.WriteLine("File doesn't exist!");
            return;
        }

        string[] lines = File.ReadAllLines(filepath);
        string[] size = lines[0].Split(' ');

        int x = Convert.ToInt32(size[0]);
        int y = Convert.ToInt32(size[1]);

        int[,] puzzleArray = new int[x, y];

        for (int i = 0; i < x; i++)
        {
            string[] values = lines[i + 1].Split(' ');
            for (int j = 0; j < y; j++)
            {
                puzzleArray[i, j] = Convert.ToInt32(values[j]);
            }
        }


        Console.Write("Wybierz algorytm:\n" +
                "a) BFS\n" +
                "b) DFS\n" +
                "c) A* (Hamming)\n" +
                "d) A* (Manhattan)\n");

        string[] solution = null;
        bool isSelected = false;
        do
        {
            string Response1 = Console.ReadLine();

            switch (Response1.ToUpper())
            {
                case "A":
                    Console.Write("Podaj kolejność przeszukiwania dla DFS (np. UDLR, ULDR, LDUR): ");
                    string order = Console.ReadLine().ToUpper();
                    solution = SolveBFS(puzzleArray, x, y, order);
                    SaveToFile(solution, NazwaPliku, "bfs", order.ToLower());
                    isSelected = true;
                    break;
                case "B":
                    Console.Write("Podaj kolejność przeszukiwania dla DFS (np. UDLR, ULDR, LDUR): ");
                    order = Console.ReadLine().ToUpper();
                    solution = SolveDFS(puzzleArray, x, y, order);
                    SaveToFile(solution,NazwaPliku,"dfs",order.ToLower());
                    isSelected = true;
                    break;
                case "C":
                    solution = SolveAStar(puzzleArray, x, y, "H");
                    SaveToFile(solution, NazwaPliku, "astr", "hamm");
                    isSelected = true;
                    break;
                case "D":
                    solution = SolveAStar(puzzleArray, x, y, "M");
                    SaveToFile(solution, NazwaPliku, "astr", "manh");
                    isSelected = true;
                    break;
                default:
                    Console.WriteLine("Niepoprawny wybór. Wybierz A, B, C lub D.");
                    continue;
            }
        } while (!isSelected);


        PrintPuzzle(puzzleArray, x, y);

        if (solution != null)
        {
            Console.WriteLine($"Znaleziona najszybsza droga to: {solution[solution.Length-1]}");

            string solutionFilePath = Path.Combine("..", "..",Directory.GetCurrentDirectory(), "solution.txt");
            File.WriteAllText(solutionFilePath, solution[solution.Length-1]);

            Console.Write("Czy chcesz zobaczyć wizualizację? (T/N): ");
            string response = Console.ReadLine();

            if (response?.ToUpper() == "T")
            {
                string relativePath = Path.Combine("..","..", "..", "..","FifteenView", "bin", "Debug", "net9.0-windows", "FifteenView.exe");
                Console.WriteLine(Directory.GetCurrentDirectory());
                string fullPath = Path.GetFullPath(relativePath);
                string moves = string.Join(",", solution);

                if (File.Exists(fullPath))
                {
                    Process.Start(fullPath, moves);
                }
                else
                {
                    Console.WriteLine("Nie znaleziono pliku View.exe! Sprawdź ścieżkę.");
                }
            }
        }
        else
        {
            Console.WriteLine("Brak rozwiązania.");
        }
    }


    private static bool SaveToFile(string[] Results,
        string NazwaPliku, string Algorytm, string HeurystykaLubKolejka)
    {
        try
        {
            String nowaNazwaPliku = NazwaPliku.Replace(".txt", $"_{Algorytm}_{HeurystykaLubKolejka}_sol.txt");

            File.WriteAllLines(nowaNazwaPliku, Results);

            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static List<int> SearchForZero(int [,] puzzle, int x , int y)
    {
        List<int> zeroCoordinates = [];
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                if (puzzle[i, j] == 0)
                {
                    zeroCoordinates.Add(i);
                    zeroCoordinates.Add(j);
                }
            }
            
        }
        return zeroCoordinates;
    }

    private static string[] SolveDFS(int[,] puzzle, int x, int y,string order)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Stack<(int[,], int, int, List<string>, int, char)> stack = new();
        HashSet<string> visited = new();

        int visitedCount = 0;
        int processedCount = 0;
        int maxDepthReached = 0;

        int emptyX = SearchForZero(puzzle, x, y)[0];
        int emptyY = SearchForZero(puzzle, x, y)[1];

        stack.Push((puzzle, emptyX, emptyY, new List<string>(), 0, ' '));
        visited.Add(GetState(puzzle));

        while (stack.Count > 0)
        {
            (int[,], int, int, List<string>, int, char) takenElement = stack.Pop();
            int[,] currentPuzzle = takenElement.Item1;
            int curX = takenElement.Item2;
            int curY = takenElement.Item3;
            List<string> path = takenElement.Item4;
            int Depth = takenElement.Item5;
            char lastMove = takenElement.Item6;

            maxDepthReached = Math.Max(maxDepthReached, Depth);

            if (IsSolved(currentPuzzle, x, y))
            {
                stopwatch.Stop();
                Console.WriteLine("Dlugość rozwiązania: " + Depth);
                Console.WriteLine("Liczba odwiedzonych stanów: " + visitedCount);
                Console.WriteLine("Liczba przetworzonych stanów: " + processedCount);
                Console.WriteLine("Maksymalna glebokosc rekursji: " + maxDepthReached);
                double czasWin = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3);
                Console.WriteLine($"DFS czas wykonania: {czasWin} (ms)");
                return [Depth.ToString(), visitedCount.ToString(), processedCount.ToString(), maxDepthReached.ToString(), czasWin.ToString(), String.Join("", path)];
            }

            if (Depth >= 20) continue;

            foreach (char next in order)
            {
                if (moveDictionary.ContainsKey(next) && next != swingMoves[lastMove])
                {
                    processedCount++;
                    (int row, int col, string move) moveInfo = moveDictionary[next];
                    int newX = curX + moveInfo.row;
                    int newY = curY + moveInfo.col;
                    string move = moveInfo.move;

                    if (newX >= 0 && newX < x && newY >= 0 && newY < y)
                    {
                        int[,] newPuzzle = (int[,])currentPuzzle.Clone();
                        (newPuzzle[curX, curY], newPuzzle[newX, newY]) = (newPuzzle[newX, newY], newPuzzle[curX, curY]);

                        string stateStr = GetState(newPuzzle);
                        if (!visited.Contains(stateStr))
                        {
                            visited.Add(stateStr);
                            visitedCount++;
                            List<string> newPath = new List<string>(path);
                            newPath.Add(move);
                            stack.Push((newPuzzle, newX, newY, newPath, Depth + 1, next));
                        }
                    }
                }
            }
        }
        stopwatch.Stop();
        Console.WriteLine("Dlugość rozwiązania: -1");
        Console.WriteLine("Liczba odwiedzonych stanów: " + visitedCount);
        Console.WriteLine("Liczba przetworzonych stanów: " + processedCount);
        Console.WriteLine("Maksymalna glebokosc rekursji: " + maxDepthReached);
        double czas = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3);
        Console.WriteLine($"DFS czas wykonania: {czas} (ms)");
        return ["-1", visitedCount.ToString(), processedCount.ToString(), maxDepthReached.ToString(), czas.ToString()];

    }


    private static string[] SolveBFS(int[,] puzzle, int x, int y, string order)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Queue<(int[,], int, int, List<string>,char, int)> queue = new();
        HashSet<string> visited = new();

        int visitedCount = 0;
        int processedCount = 0;
        int maxDepthReached = 0;

        int emptyX = 0, emptyY = 0;

        emptyX = SearchForZero(puzzle, x, y)[0];
        emptyY = SearchForZero(puzzle, x, y)[1];



        queue.Enqueue((puzzle, emptyX, emptyY, new List<string>(),' ', 0));

        visited.Add(GetState(puzzle));

        while (queue.Count > 0)
        {
            (int[,], int, int, List<string>, char, int) takenElement = queue.Dequeue();
            int[,] currentPuzzle = takenElement.Item1;
            int curX = takenElement.Item2;
            int curY = takenElement.Item3;
            List<string> path = takenElement.Item4;
            char lastMove = takenElement.Item5;
            int Depth = takenElement.Item6;

            maxDepthReached = Math.Max(maxDepthReached, Depth);

            if (IsSolved(currentPuzzle, x, y))
            {
                stopwatch.Stop();
                Console.WriteLine("Dlugość rozwiązania: " + Depth);
                Console.WriteLine("Liczba odwiedzonych stanów: " + visitedCount);
                Console.WriteLine("Liczba przetworzonych stanów: " + processedCount);
                Console.WriteLine("Maksymalna glebokosc rekursji: " + maxDepthReached);
                double czasWin = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3);
                Console.WriteLine($"DFS czas wykonania: {czasWin} (ms)");
                return [Depth.ToString(), visitedCount.ToString(), processedCount.ToString(), maxDepthReached.ToString(), czasWin.ToString(), String.Join("", path)];
            }

            foreach (char next in order)
            {
                if (moveDictionary.ContainsKey(next) && next != swingMoves[lastMove])
                {
                    processedCount++;

                    (int row, int col, string move) moveInfo = moveDictionary[next];
                    int newX = curX + moveInfo.row;
                    int newY = curY + moveInfo.col;
                    string move = moveInfo.move;

                    if (newX >= 0 && newX < x && newY >= 0 && newY < y)
                    {
                        int[,] newPuzzle = (int[,])currentPuzzle.Clone();
                        (newPuzzle[curX, curY], newPuzzle[newX, newY]) = (newPuzzle[newX, newY], newPuzzle[curX, curY]);

                        string stateStr = GetState(newPuzzle);
                        if (!visited.Contains(stateStr))
                        {
                            visited.Add(stateStr);
                            visitedCount++;
                            List<string> newPath = new List<string>(path);
                            newPath.Add(move);
                            queue.Enqueue((newPuzzle, newX, newY, newPath,next, Depth + 1));
                        }
                    }
                }
            }
        }
            stopwatch.Stop();
        Console.WriteLine("Dlugość rozwiązania: -1");
        Console.WriteLine("Liczba odwiedzonych stanów: " + visitedCount);
        Console.WriteLine("Liczba przetworzonych stanów: " + processedCount);
        Console.WriteLine("Maksymalna glebokosc rekursji: " + maxDepthReached);
        double czas = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3);
        Console.WriteLine($"DFS czas wykonania: {czas} (ms)");
        return ["-1", visitedCount.ToString(), processedCount.ToString(), maxDepthReached.ToString(), czas.ToString()];
    }

    private static string[] SolveAStar(int[,] puzzle, int x, int y, string heuristicType)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        PriorityQueue<(int[,], int, int, List<string>, int), int> priorityQueue = new();
        HashSet<string> visited = new();

        int visitedCount = 0;
        int processedCount = 0;
        int maxDepthReached = 0;

        int emptyX = SearchForZero(puzzle, x, y)[0];
        int emptyY = SearchForZero(puzzle, x, y)[1];

        int h = heuristicType == "H" ? HammingDistance(puzzle, x, y) : ManhattanDistance(puzzle, x, y);
        priorityQueue.Enqueue((puzzle, emptyX, emptyY, new List<string>(), 0), h);
        visited.Add(GetState(puzzle));

        while (priorityQueue.Count > 0)
        {
            (int[,], int, int, List<string>, int g) takenElement = priorityQueue.Dequeue();
            int[,] currentPuzzle = takenElement.Item1;
            int curX = takenElement.Item2;
            int curY = takenElement.Item3;
            List<string> path = takenElement.Item4;
            int gCost = takenElement.g;

            maxDepthReached = Math.Max(maxDepthReached, gCost);

            if (IsSolved(currentPuzzle, x, y)) 
            {
                stopwatch.Stop();
                Console.WriteLine("Dlugość rozwiązania: " + gCost);
                Console.WriteLine("Liczba odwiedzonych stanów: " + visitedCount);
                Console.WriteLine("Liczba przetworzonych stanów: " + processedCount);
                Console.WriteLine("Maksymalna glebokosc rekursji: " + maxDepthReached);
                double czasWin = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3);
                Console.WriteLine($"DFS czas wykonania: {czasWin} (ms)");
                return [gCost.ToString(), visitedCount.ToString(), processedCount.ToString(), maxDepthReached.ToString(), czasWin.ToString(), String.Join("", path)];
            }
            

            int[] rows = { -1, 1, 0, 0 };
            int[] cols = { 0, 0, -1, 1 };
            string[] moves = { "U", "D", "L", "R" };

            for (int i = 0; i < 4; i++)
            {
                int newX = curX + rows[i];
                int newY = curY + cols[i];
                string move = moves[i];

                if (newX >= 0 && newX < x && newY >= 0 && newY < y)
                {
                    processedCount++;

                    int[,] newPuzzle = (int[,])currentPuzzle.Clone();
                    (newPuzzle[curX, curY], newPuzzle[newX, newY]) = (newPuzzle[newX, newY], newPuzzle[curX, curY]);

                    string stateStr = GetState(newPuzzle);
                    if (!visited.Contains(stateStr))
                    {
                        visited.Add(stateStr);
                        visitedCount++;
                        List<string> newPath = new List<string>(path) { move };
                        int newG = gCost + 1;
                        int newH = heuristicType == "H" ? HammingDistance(newPuzzle, x, y) : ManhattanDistance(newPuzzle, x, y);
                        int f = newG + newH;
                        priorityQueue.Enqueue((newPuzzle, newX, newY, newPath, newG), f);
                    }
                }
            }
        }
        stopwatch.Stop();
        Console.WriteLine("Dlugość rozwiązania: -1");
        Console.WriteLine("Liczba odwiedzonych stanów: " + visitedCount);
        Console.WriteLine("Liczba przetworzonych stanów: " + processedCount);
        Console.WriteLine("Maksymalna glebokosc rekursji: " + maxDepthReached);
        double czas = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3);
        Console.WriteLine($"DFS czas wykonania: {czas} (ms)");
        return ["-1", visitedCount.ToString(), processedCount.ToString(), maxDepthReached.ToString(), czas.ToString()];
    }


    private static string GetState(int[,] puzzle)
    {
        return string.Join(",", puzzle.Cast<int>());
    }

    private static bool IsSolved(int[,] puzzle, int x, int y)
    {
        int expected = 1;
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                if (i == x - 1 && j == y - 1) return puzzle[i, j] == 0;
                if (puzzle[i, j] != expected++) return false;
            }
        }
        return true;
    }

    private static int HammingDistance(int[,] puzzle, int x, int y)
    {
        int hamming = 0;
        int expected = 1;

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                if (puzzle[i, j] != expected && puzzle[i, j] != 0)
                    hamming++;

                expected++;
            }
        }
        return hamming;
    }

    private static int ManhattanDistance(int[,] puzzle, int x, int y)
    {
        int manhattan = 0;

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                int value = puzzle[i, j];
                if (value != 0) 
                {
                    int targetX = (value - 1) / x; 
                    int targetY = (value - 1) % y;
                    manhattan += Math.Abs(i - targetX) + Math.Abs(j - targetY);
                }
            }
        }
        return manhattan;
    }



    private static void PrintPuzzle(int[,] puzzle, int x, int y)
    {
        //Console.Clear();
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                Console.Write(puzzle[i, j] + "\t");
            }
            Console.WriteLine();
        }
    }
}