using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifteenApp
{
    public class PuzzleLoader
    {
        public int[,] PuzzleArray { get; private set; }
        public int x { get; private set; }
        public int y { get; private set; }

        public PuzzleLoader(string path)
        {
            LoadPuzzle(path);
        }

        private void LoadPuzzle(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("File doenst exist!");
                return;
            }

            string[] lines = File.ReadAllLines(path);

            string[] sizeValues = lines[0].Split(' ');

            x = Convert.ToInt32(sizeValues[0]);
            y = Convert.ToInt32(sizeValues[1]);

            PuzzleArray = new int[x, y];

            for (int i = 0; i < x; i++)
            {
                string[] values = lines[i + 1].Split(' ');

                for (int j = 0; j < y; j++)
                {
                    PuzzleArray[i, j] = Convert.ToInt32(values[j]);
                }
            }
        }
    }
}