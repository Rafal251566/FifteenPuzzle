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

        List<string> solution = null;
        bool isSelected = false;
        do
        {
            string Response1 = Console.ReadLine();

            switch (Response1.ToUpper())
            {
                case "A":
                    Console.Write("Podaj kolejność przeszukiwania dla DFS (np. UDLR, ULDR, LDUR): ");
                    string order = Console.ReadLine().ToUpper();
                    solution = SolveBFS(puzzleArray, x, y,order);
                    isSelected = true;
                    break;
                case "B":
                    Console.Write("Podaj kolejność przeszukiwania dla DFS (np. UDLR, ULDR, LDUR): ");
                    order = Console.ReadLine().ToUpper();
                    solution = SolveDFS(puzzleArray, x, y,order);
                    isSelected = true;
                    break;
                case "C":
                    solution = SolveAStar(puzzleArray, x, y, "H");
                    isSelected = true;
                    break;
                case "D":
                    solution = SolveAStar(puzzleArray, x, y, "M");
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
            Console.WriteLine("Znaleziona najszybsza droga to: " + string.Join(" -> ", solution));

            string solutionFilePath = Path.Combine("..", "..",Directory.GetCurrentDirectory(), "solution.txt");
            File.WriteAllLines(solutionFilePath, solution);

            Console.Write("Czy chcesz zobaczyć wizualizację? (T/N): ");
            string response = Console.ReadLine();

            if (response?.ToUpper() == "T")
            {
                string relativePath = Path.Combine("..", "..","..",".." ,"FifteenView", "bin", "Debug", "net9.0-windows", "FifteenView.exe");
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

    private static List<String> SolveDFS(int[,] puzzle, int x, int y,string order)
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
                Console.WriteLine($"DFS czas wykonania: {stopwatch.ElapsedMilliseconds} (ms)");
                Console.WriteLine("Glebokosc: " + Depth);
                Console.WriteLine("Maksymalna glebokosc rekursji: " + maxDepthReached);
                Console.WriteLine("Liczba odwiedzonych stanów: " + visitedCount);
                Console.WriteLine("Liczba przetworzonych stanów: " + processedCount);
                return path;
            }

            if (Depth >= 20) continue;
            processedCount++;

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
                            stack.Push((newPuzzle, newX, newY, newPath, Depth + 1, next));
                        }
                    }
                }
            }
        }
        stopwatch.Stop();
        Console.WriteLine($"DFS czas wykonania: {stopwatch.ElapsedMilliseconds} (ms)");
        Console.WriteLine("Liczba odwiedzonych stanów: " + visitedCount);
        Console.WriteLine("Liczba przetworzonych stanów: " + processedCount);
        return null;

    }


    private static List<string> SolveBFS(int[,] puzzle, int x, int y, string order)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Queue<(int[,], int, int, List<string>,char)> queue = new();
        HashSet<string> visited = new();

        int emptyX = 0, emptyY = 0;

        emptyX = SearchForZero(puzzle, x, y)[0];
        emptyY = SearchForZero(puzzle, x, y)[1];



        queue.Enqueue((puzzle, emptyX, emptyY, new List<string>(),' '));

        visited.Add(GetState(puzzle));

        while (queue.Count > 0)
        {
            (int[,], int, int, List<string>, char) takenElement = queue.Dequeue();
            int[,] currentPuzzle = takenElement.Item1;
            int curX = takenElement.Item2;
            int curY = takenElement.Item3;
            List<string> path = takenElement.Item4;
            char lastMove = takenElement.Item5;

            if (IsSolved(currentPuzzle, x, y))
            {
                stopwatch.Stop();
                Console.WriteLine($"BFS czas wykonania: {stopwatch.ElapsedMilliseconds} (ms)");
                return path;
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
                            List<string> newPath = new List<string>(path);
                            newPath.Add(move);
                            queue.Enqueue((newPuzzle, newX, newY, newPath,next));
                        }
                    }
                }
            }
        }
            stopwatch.Stop();
            return null;      
    }

    private static List<string> SolveAStar(int[,] puzzle, int x, int y, string heuristicType)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        PriorityQueue<(int[,], int, int, List<string>, int), int> priorityQueue = new();
        HashSet<string> visited = new();

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

            if (IsSolved(currentPuzzle, x, y)) 
            {
                stopwatch.Stop();
                Console.WriteLine($"SolveAStar ({heuristicType}) czas wykonania: {stopwatch.ElapsedMilliseconds} (ms)");
                return path;
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
                    int[,] newPuzzle = (int[,])currentPuzzle.Clone();
                    (newPuzzle[curX, curY], newPuzzle[newX, newY]) = (newPuzzle[newX, newY], newPuzzle[curX, curY]);

                    string stateStr = GetState(newPuzzle);
                    if (!visited.Contains(stateStr))
                    {
                        visited.Add(stateStr);
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
        return null;
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