using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j1
{

    using System;
    using System.Collections.Generic;
    using System.Drawing;

    namespace j1
    {
        public class GameModel
        {
            private Random random = new Random();
            public List<string> SavedColors { get; private set; } = new List<string>(); 
            public int LevelNumber { get; private set; } = 4;
            public int ColorMatch { get; private set; } = 0;
            public int IndexMatch { get; private set; } = 0;
            public int CurrentRow { get; private set; } = 0;

            private List<string> colors = new List<string>
            {
                "Red","Blue","Green","Yellow","Purple","Orange","Pink","Brown"
            };

            public GameModel(int levelNumber = 4)
            {
                LevelNumber = levelNumber;
                GenerateRandomColors();
            }

            private void GenerateRandomColors()
            {
                for (int i = 0; i < LevelNumber; i++)
                {
                    SavedColors.Add(GetRandomColor());
                }
            }

            public string GetRandomColor() // Véletlenszerű szín generálása
            {
                int index = random.Next(colors.Count);  // Véletlenszerű szín kiválasztása a listából
                string colorName = colors[index];
                colors.RemoveAt(index);
                return colorName;
            }

            public string GetImagePath(string colorName) // Kép elérési útjának generálása a szín névből
            {
                return $"{colorName}.png";
            }

            public void Evaluate(List<string> currentRowColors)     //kiértékelés
            {
                ColorMatch = 0;
                IndexMatch = 0;

                for (int i = 0; i < currentRowColors.Count; i++)
                {
                    if (SavedColors.Contains(currentRowColors[i]))
                    {
                        ColorMatch++;
                        if (SavedColors[i] == currentRowColors[i])
                        {
                            IndexMatch++;
                        }
                    }
                }

                CurrentRow++;
            }
        }
    }


}
