using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using FifteenApp;

namespace FifteenView
{
    public partial class MainWindow : Window
    {
        private int gridSize;
        private Button[,] buttons;
        private PuzzleLoader puzzleLoader;
        private string[] solutionMoves;

        public MainWindow()
        {
            InitializeComponent();

            string filepath = Path.Combine(Directory.GetCurrentDirectory(), "4x4_07_00201.txt");
            puzzleLoader = new PuzzleLoader(filepath);
            gridSize = puzzleLoader.x;
            GenerateGrid(gridSize);
            string solutionFilePath = Path.Combine(Directory.GetCurrentDirectory(), "solution.txt");

            // Poprawiona inicjalizacja
            solutionMoves = File.ReadAllLines(solutionFilePath);
            SolveAnimation();
        }

        private async void SolveAnimation()
        {
            foreach (string move in solutionMoves)
            {
                await Task.Delay(500);
                MakeMove(move);
            }
        }

        private void GenerateGrid(int size)
        {
            buttons = new Button[size, size];
            Grid puzzleGrid = new Grid { Margin = new Thickness(10) };

            for (int i = 0; i < size; i++)
            {
                puzzleGrid.RowDefinitions.Add(new RowDefinition());
                puzzleGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Button button = new Button
                    {
                        FontSize = 24,
                        Width = 60,
                        Height = 60
                    };

                    int value = puzzleLoader.PuzzleArray[i, j];

                    if (value != 0)
                    {
                        button.Content = value.ToString();
                    }
                    else
                    {
                        button.Visibility = Visibility.Hidden;
                    }

                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    puzzleGrid.Children.Add(button);
                    buttons[i, j] = button;
                }
            }

            this.Content = puzzleGrid;
        }

        private void MakeMove(string move)
        {
            int emptyRow = -1, emptyCol = -1;
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (puzzleLoader.PuzzleArray[i, j] == 0)
                    {
                        emptyRow = i;
                        emptyCol = j;
                        break;
                    }
                }
                if (emptyRow != -1) break;
            }

            switch (move)
            {
                case "U":
                    if (emptyRow > 0)
                    {
                        Swap(emptyRow, emptyCol, emptyRow - 1, emptyCol);
                    }
                    break;

                case "D": // Dół
                    if (emptyRow < gridSize - 1)
                    {
                        Swap(emptyRow, emptyCol, emptyRow + 1, emptyCol);
                    }
                    break;

                case "L": // Lewo
                    if (emptyCol > 0)
                    {
                        Swap(emptyRow, emptyCol, emptyRow, emptyCol - 1);
                    }
                    break;

                case "R": // Prawo
                    if (emptyCol < gridSize - 1)
                    {
                        Swap(emptyRow, emptyCol, emptyRow, emptyCol + 1);
                    }
                    break;
            }

            UpdateUI();
        }

        private void Swap(int row1, int col1, int row2, int col2)
        {
            int temp = puzzleLoader.PuzzleArray[row1, col1];
            puzzleLoader.PuzzleArray[row1, col1] = puzzleLoader.PuzzleArray[row2, col2];
            puzzleLoader.PuzzleArray[row2, col2] = temp;
        }

        private void UpdateUI()
        {
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    Button button = buttons[i, j];
                    int value = puzzleLoader.PuzzleArray[i, j];

                    if (value != 0)
                    {
                        button.Content = value.ToString();
                        button.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        button.Visibility = Visibility.Hidden;
                    }
                }
            }
        }
    }
}