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



    private static void Main(string[] args)
    {
        if (args.Length < 3 || (args[0].ToLower() == "astr" && args.Length < 3))
        {
            Console.WriteLine("Użycie: program <ALGORYTM> [HEURYSTYKA] <KOLEJNOSC> <PLIK_WEJSCIOWY>");
            return;
        }

        string algorithm = args[0].ToUpper();
        string heuristic = "";
        string order = "";
        int index = 1;

        if (algorithm == "ASTR")
        {
            heuristic = args[1].ToUpper();
            index++;
        }else
        {
            order = args[1].ToUpper();
            index++;
        }

            string inputFile = args[2];

        if (!File.Exists(inputFile))
        {
            Console.WriteLine("Plik wejściowy nie istnieje!");
            return;
        }


        string filepath = Path.Combine(Directory.GetCurrentDirectory(), inputFile);

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

        string[] solution = null;

            switch (algorithm.ToLower())
            {
                case "bfs":
                    solution = SolveBFS(puzzleArray, x, y, order);
                    SaveToFiles(solution, inputFile, "bfs", order.ToLower());
                    break;
                case "dfs":
                    solution = SolveDFS(puzzleArray, x, y, order);
                    SaveToFiles(solution, inputFile, "dfs",order.ToLower());
                    break;
                case "astr":
                    solution = SolveAStar(puzzleArray, x, y, heuristic);
                    SaveToFiles(solution, inputFile, "astr", heuristic.ToLower());
                    break;
                break;
            }


        PrintPuzzle(puzzleArray, x, y);

        if (solution != null)
        {
            Console.WriteLine($"Znaleziona najszybsza droga to: {solution[solution.Length-1]}");

            string solutionFilePath = Path.Combine("..", "..",Directory.GetCurrentDirectory(), "solution.txt");
            File.WriteAllText(solutionFilePath, solution[solution.Length-1]);

            Console.Write("Czy chcesz zobaczyć wizualizację? (T/N): ");
            string response = "N";

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


    private static bool SaveToFiles(string[] Results,
        string NazwaPliku, string Algorytm, string HeurystykaLubKolejka)
    {
        string[] newResults = new string[2];
        newResults[0] = Results[0];
        newResults[1] = Results[Results.Length - 1];
        try
        {
            String nowaNazwaPliku = NazwaPliku.Replace(".txt", $"_{Algorytm}_{HeurystykaLubKolejka}_stats.txt");

            string nowaNazwaPliku2 = NazwaPliku.Replace(".txt", $"_{Algorytm}_{HeurystykaLubKolejka}_sol.txt");

            if (newResults[0] == "-1")
            {

                File.WriteAllLines(nowaNazwaPliku, Results);
                File.WriteAllText(nowaNazwaPliku2, newResults[0]);

            } else {

                File.WriteAllLines(nowaNazwaPliku, Results.Take(Results.Length - 1));
                File.WriteAllLines(nowaNazwaPliku2, newResults);
            }
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

    private static string[] SolveDFS(int[,] puzzle, int x, int y, string order)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Stack<(int[,], int, int, List<string>, int, char)> stack = new();
        Dictionary<string, int> visited = new Dictionary<string, int>();

        int visitedCount = 0;
        int processedCount = 0;
        int maxDepthReached = 0;

        (int emptyX, int emptyY) = (SearchForZero(puzzle, x, y)[0], SearchForZero(puzzle, x, y)[1]);

        string initialState = GetState(puzzle);
        stack.Push((puzzle, emptyX, emptyY, new List<string>(), 0, ' '));
        visited[initialState] = 0;

        while (stack.Count > 0)
        {
            (int[,], int, int, List<string>, int, char) takenElement = stack.Pop();
            int[,] currentPuzzle = takenElement.Item1;
            int curX = takenElement.Item2;
            int curY = takenElement.Item3;
            List<string> path = takenElement.Item4;
            int depth = takenElement.Item5;
            char lastMove = takenElement.Item6;

            processedCount++;
            maxDepthReached = Math.Max(maxDepthReached, depth);

            if (IsSolved(currentPuzzle, x, y))
            {
                stopwatch.Stop();
                double czasWin = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3);
                return [depth.ToString(),visitedCount.ToString(),processedCount.ToString(),maxDepthReached.ToString(),czasWin.ToString(),string.Join("", path)];
            }

            if (depth >= 20) continue;

            foreach (char next in order)
            {
                if (moveDictionary.ContainsKey(next) && next != swingMoves[lastMove])
                {
                    var (dx, dy, moveStr) = moveDictionary[next];
                    int newX = curX + dx;
                    int newY = curY + dy;

                    if (newX >= 0 && newX < x && newY >= 0 && newY < y)
                    {
                        int[,] newPuzzle = (int[,])currentPuzzle.Clone();
                        (newPuzzle[curX, curY], newPuzzle[newX, newY]) = (newPuzzle[newX, newY], newPuzzle[curX, curY]);

                        string newState = GetState(newPuzzle);
                        int newDepth = depth + 1;

                        if (!visited.ContainsKey(newState) || visited[newState] > newDepth)
                        {
                            visited[newState] = newDepth;
                            visitedCount++;

                            List<string> newPath = new List<string>(path);
                            newPath.Add(moveStr);
                            stack.Push((newPuzzle, newX, newY, newPath, newDepth, next));
                        }
                    }
                }
            }
        }

        stopwatch.Stop();
        double czas = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3);
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
            processedCount++;

            maxDepthReached = Math.Max(maxDepthReached, Depth);

            if (IsSolved(currentPuzzle, x, y))
            {
                stopwatch.Stop();
                double czasWin = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3);
                return [Depth.ToString(), visitedCount.ToString(), processedCount.ToString(), maxDepthReached.ToString(), czasWin.ToString(), String.Join("", path)];
            }

            foreach (char next in order)
            {
                if (moveDictionary.ContainsKey(next) && next != swingMoves[lastMove])
                {

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
        double czas = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3);
        return ["-1", visitedCount.ToString(), processedCount.ToString(), maxDepthReached.ToString(), czas.ToString()];
    }

    private static string[] SolveAStar(int[,] puzzle, int x, int y, string heuristicType)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        PriorityQueue<(int[,], int, int, List<string>, int,char), int> priorityQueue = new();
        HashSet<string> visited = new();

        int visitedCount = 0;
        int processedCount = 0;
        int maxDepthReached = 0;

        int emptyX = SearchForZero(puzzle, x, y)[0];
        int emptyY = SearchForZero(puzzle, x, y)[1];

        int h = heuristicType.ToLower() == "hamm" ? HammingDistance(puzzle, x, y) : ManhattanDistance(puzzle, x, y);
        priorityQueue.Enqueue((puzzle, emptyX, emptyY, new List<string>(), 0,' '), h);
        visited.Add(GetState(puzzle));

        while (priorityQueue.Count > 0)
        {
            (int[,], int, int, List<string>, int g, char) takenElement = priorityQueue.Dequeue();
            int[,] currentPuzzle = takenElement.Item1;
            int curX = takenElement.Item2;
            int curY = takenElement.Item3;
            List<string> path = takenElement.Item4;
            int gCost = takenElement.g;
            char lastMove = takenElement.Item6;
            processedCount++;


            maxDepthReached = Math.Max(maxDepthReached, gCost);

            if (IsSolved(currentPuzzle, x, y))
            {
                stopwatch.Stop();
                double czasWin = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3);
                return [gCost.ToString(), visitedCount.ToString(), processedCount.ToString(), maxDepthReached.ToString(), czasWin.ToString(), String.Join("", path)];
            }

            string order = "UDLR";
            foreach (char next in order)
            {
                if (moveDictionary.ContainsKey(next) && next != swingMoves[lastMove])
                {

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
                            List<string> newPath = new List<string>(path) { move };
                            int newG = gCost + 1;
                            int newH = heuristicType.ToLower() == "hamm" ? HammingDistance(newPuzzle, x, y) : ManhattanDistance(newPuzzle, x, y);
                            int f = newG + newH;
                            priorityQueue.Enqueue((newPuzzle, newX, newY, newPath, newG, next), f);
                        }

                    }
                }
            }
        }
            stopwatch.Stop();
            double czas = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3);
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