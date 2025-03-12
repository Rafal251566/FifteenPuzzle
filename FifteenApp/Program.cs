using System;
using System.IO;

internal class Program
{
    private static void Main(string[] args)
    {
        string path = @"D:\Pobrane\puzzlegen\4x4_01_00001.txt";

        if (!File.Exists(path))
        {
            Console.WriteLine("File doesn't exist!");
            return;
        }

        string[] lines = File.ReadAllLines(path);
        string[] size = lines[0].Split(' ');

        int x = Convert.ToInt32(size[0]);
        int y = Convert.ToInt32(size[1]);

        int[,] puzzleArray = new int[x, y];

        int emptyX=1, emptyY=1;

        for (int i = 0; i < x; i++)
        {
            string[] values = lines[i + 1].Split(' ');
            for (int j = 0; j < y; j++)
            {
                puzzleArray[i, j] = Convert.ToInt32(values[j]);
                if (puzzleArray[i, j] == 0)
                {
                    emptyX = i;
                    emptyY = j;
                }
            }
        }

        PrintPuzzle(puzzleArray, x, y);

        int moves = 0;
        while (true)
        {
            Console.Write("Podaj ruch (U/D/L/R lub X aby wyjść): ");
            string move = Console.ReadLine().ToUpper();
            moves++;
            if (move == "X") break;

            if (Move(puzzleArray, ref emptyX, ref emptyY, move, x, y))
            {
                PrintPuzzle(puzzleArray, x, y);

                if (IsSolved(puzzleArray, x, y))
                {
                    Console.WriteLine("Udało Ci sie rozwiązać w " + moves + " ruchach!");
                    break;
                }
            }
            else
            {
                Console.WriteLine("Nie można takiego ruchu!");
            }
        }
    }

    private static bool Move(int[,] puzzle, ref int emptyX, ref int emptyY, string direction, int maxX, int maxY)
    {
        int newX = emptyX, newY = emptyY;

        switch (direction)
        {
            case "U": newX--; break;
            case "D": newX++; break;
            case "L": newY--; break;
            case "R": newY++; break;
            default: return false;
        }

        if (newX >= 0 && newX < maxX && newY >= 0 && newY < maxY)
        {
            (puzzle[emptyX, emptyY], puzzle[newX, newY]) = (puzzle[newX, newY], puzzle[emptyX, emptyY]);
            emptyX = newX;
            emptyY = newY;
            return true;
        }
        return false;
    }

    private static void PrintPuzzle(int[,] puzzle, int x, int y)
    {
        Console.Clear();
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                Console.Write(puzzle[i, j] + "\t");
            }
            Console.WriteLine();
        }
    }

    private static bool IsSolved(int[,] puzzle, int x, int y)
    {
        int expected = 1;
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                if (i == x - 1 && j == y - 1)
                {
                    return puzzle[i, j] == 0;
                }
                if (puzzle[i, j] != expected++)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
