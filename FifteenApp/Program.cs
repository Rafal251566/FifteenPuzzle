﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

internal class Program
{
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

        int emptyX = 1, emptyY = 1;

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

        List<string> solution = SolveBFS(puzzleArray, x, y);

        if (solution != null)
        {
            Console.WriteLine("Znaleziona najszybsza droga to: " + string.Join(" -> ", solution));
        }
        else
        {
            Console.WriteLine("Brak rozwiązania.");
        }
    }

    private static List<string> SolveBFS(int[,] puzzle, int x, int y)
    {
        Queue<(int[,], int, int, List<string>)> queue = new();
        HashSet<string> visited = new();

        int emptyX = 0, emptyY = 0;

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                if (puzzle[i, j] == 0)
                {
                    emptyX = i;
                    emptyY = j;
                }
            }
        }

        queue.Enqueue((puzzle, emptyX, emptyY, new List<string>()));
        visited.Add(GetState(puzzle));

        while (queue.Count > 0)
        {
            (int[,], int, int, List<string>) takenElement = queue.Dequeue();
            int[,] currentPuzzle = takenElement.Item1;
            int curX = takenElement.Item2;
            int curY = takenElement.Item3;
            List<string> path = takenElement.Item4;   //wydaje mi sie ze da sie to jakos lepiej/prosciej zrobic ale nie wiem na ten moment jak :D

            if (IsSolved(currentPuzzle, x, y))
                return path;

            int[] rows = { -1, 1, 0, 0 };
            int[] cols = { 0, 0, -1, 1 };
            string[] moves = { "U", "D", "L", "R" }; //to rozwiązanie nie pozwoli na wypisywanie użytkownikowi porządku przeszukiwania DO ZMIANY

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
                       // List<string> newPath = [.. path, move]; metoda zaproponowana przez VS
                        List<string> newPath = new List<string>(path);
                        newPath.Add(move);
                        queue.Enqueue((newPuzzle, newX, newY, newPath));
                    }
                }
            }
        }
        return null;
    }

    private static string GetState(int[,] puzzle)
    {
        return string.Join(",", puzzle.Cast<int>()); //To zwraca stringa z kolejnoscia liczb
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
}
