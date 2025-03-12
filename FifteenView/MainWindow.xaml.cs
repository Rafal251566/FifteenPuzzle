﻿using System;
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

        public MainWindow()
        {
            InitializeComponent();

            string path = @"D:\Pobrane\puzzlegen\4x4_01_00001.txt"; //na ten moment statycznie do zmiany ofc
                puzzleLoader = new PuzzleLoader(path);
                gridSize = puzzleLoader.x;
                GenerateGrid(gridSize);
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

        //TODO logika przycisku
    }
}
