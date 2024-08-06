using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MasterMindGame
{
    public partial class Form1 : Form
    {
        public List<PictureBox> panel1List = new List<PictureBox>();
        public List<PictureBox> panel2List = new List<PictureBox>();
        public List<string> savedColorsName = new List<string>();
        public List<string> panel2Colors = new List<string>();
        public List<string> colors = new List<string> {
            "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Pink", "Brown" };
        public List<PictureBox> initialPanel1Images = new List<PictureBox>();
        public List<PictureBox> tempPanel1Images = new List<PictureBox>(); // Új ideiglenes lista

        private Random random = new Random();

        public int panel1NumOfOrbs = 4; // panel1 gömbök száma, ToDo: szintlépésnél növekszik
        public int colorMatch = 0;
        public int indexMatch = 0;
        public int panel2CurrentRow = 0;
        public int level = 1;
        int offset = 1; // panel2ből panel1be visszaállítás
        public Form1()
        {
            InitializeComponent();
            FormSetup();
            GeneratePictureBoxes();
            Panel2FillGray();
        }

        public void FormSetup()
        {
            panel1.BackColor = Color.White;
            panel2.BackColor = Color.White;
            panel3.BackColor = Color.White;
            button1.Text = "Kiértékelés";
            button1.Click += Button1_Click;
            label1.Text = $"Szint: {level}";
        }

        private string GetRandomColorFromList()
        {
            if (colors.Count == 0)
            {
                throw new InvalidOperationException("No more colors available.");
            }
            int index = random.Next(0, colors.Count);
            string colorName = colors[index];
            colors.RemoveAt(index);
            return colorName;
        }

        private string GetImagePath(string colorName)
        {
            return $"{colorName}.png";
        }

        public void GeneratePictureBoxes()
        {
            savedColorsName.Clear();
            initialPanel1Images.Clear();
            panel1.Controls.Clear();
            panel1List.Clear();
            tempPanel1Images.Clear(); // Ideiglenes lista ürítése

            for (int i = 0; i < panel1NumOfOrbs; i++)
            {
                string colorName = GetRandomColorFromList();
                PictureBox pictureBox = new PictureBox
                {
                    Size = new Size(20, 30),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    ImageLocation = GetImagePath(colorName),
                    Location = new Point(i * 40, 10)
                };
                pictureBox.MouseDown += PictureBox_MouseDown;
                panel1.Controls.Add(pictureBox);
                panel1List.Add(pictureBox);
                savedColorsName.Add(colorName);
                initialPanel1Images.Add(pictureBox);
                tempPanel1Images.Add(pictureBox); // Hozzáadjuk az ideiglenes listához is
            }

            if (level % 4 == 0)
            {
                string extraColorName = GetRandomColorFromList();
                PictureBox extraPictureBox = new PictureBox
                {
                    Size = new Size(20, 30),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    ImageLocation = GetImagePath(extraColorName),
                    Location = new Point(panel1NumOfOrbs * 40, 10)
                };
                extraPictureBox.MouseDown += PictureBox_MouseDown;
                panel1.Controls.Add(extraPictureBox);
                panel1List.Add(extraPictureBox);
                initialPanel1Images.Add(extraPictureBox);
                tempPanel1Images.Add(extraPictureBox); // Hozzáadjuk az ideiglenes listához is
            }
        }

        public void Panel2FillGray()
        {
            string GrayImagePath = "gray.png";
            panel2.Controls.Clear();
            panel2List.Clear();

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    PictureBox pictureBox = new PictureBox
                    {
                        Size = new Size(20, 30),
                        Location = new Point(10 + j * 40, panel2.Height - 30 - i * 30),
                        ImageLocation = GrayImagePath,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Enabled = (i == panel2CurrentRow)
                    };
                    if (i == panel2CurrentRow)
                    {
                        pictureBox.AllowDrop = true;
                        pictureBox.DragEnter += PictureBox_DragEnter;
                        pictureBox.DragDrop += PictureBox_DragDrop;
                        pictureBox.Click += PictureBox_Click;
                    }
                    panel2.Controls.Add(pictureBox);
                    panel2List.Add(pictureBox);
                }
            }
        }

        private void RestorePanel1Images() // Ha nem talált el mindent, visszaállítja a gömböket
        {
            panel1.Controls.Clear();
            panel1List.Clear();

            foreach (var pictureBox in initialPanel1Images)
            {
                PictureBox newPictureBox = new PictureBox
                {
                    Size = pictureBox.Size,
                    SizeMode = pictureBox.SizeMode,
                    ImageLocation = pictureBox.ImageLocation,
                    Location = pictureBox.Location
                };
                newPictureBox.MouseDown += PictureBox_MouseDown;
                panel1.Controls.Add(newPictureBox);
                panel1List.Add(newPictureBox);
            }
        }



        private void EnableCurrentRow()
        {
            foreach (Control control in panel2.Controls)
            {
                if (control is PictureBox pictureBox && pictureBox.Location.Y == panel2.Height - 30 - panel2CurrentRow * 30)
                {
                    pictureBox.Enabled = true;
                    pictureBox.AllowDrop = true;
                    pictureBox.DragEnter += PictureBox_DragEnter;
                    pictureBox.DragDrop += PictureBox_DragDrop;
                    pictureBox.Click += PictureBox_Click;
                }
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            panel2Colors.Clear();
            foreach (Control control in panel2.Controls)
            {
                if (control is PictureBox pictureBox && pictureBox.Location.Y == panel2.Height - 30 - panel2CurrentRow * 30)
                {
                    string colorName = Path.GetFileNameWithoutExtension(pictureBox.ImageLocation);
                    panel2Colors.Add(colorName);
                }
            }

            Evaulate(panel2Colors);

            MessageBox.Show($"Szín találat: {colorMatch} / 4, Pozíció találat: {indexMatch} / 4");

            if (colorMatch == 4 && indexMatch == 4)
            {
                MessageBox.Show("Gratulálok! Minden szín és pozíció helyes!");
                NewGame();
            }
            else
            {
                panel2CurrentRow++;
                if (panel2CurrentRow >= 10)
                {
                    MessageBox.Show("Vége a játéknak!");
                    NewGame();
                }
                else
                {
                    RestorePanel1Images();
                    EnableCurrentRow();
                }
            }
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            if (pictureBox != null)
            {
                DragDropEffects effect = pictureBox.DoDragDrop(pictureBox, DragDropEffects.Move);
                if (effect == DragDropEffects.Move)
                {
                    panel1.Controls.Remove(pictureBox);
                    panel1List.Remove(pictureBox);
                }
            }
        }

        private void PictureBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(PictureBox)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void PictureBox_DragDrop(object sender, DragEventArgs e)
        {
            PictureBox targetPictureBox = sender as PictureBox;
            PictureBox draggedPictureBox = e.Data.GetData(typeof(PictureBox)) as PictureBox;

            if (targetPictureBox != null && draggedPictureBox != null)
            {
                targetPictureBox.Image = draggedPictureBox.Image;
                targetPictureBox.ImageLocation = draggedPictureBox.ImageLocation;
                panel1.Controls.Remove(draggedPictureBox);
                panel1List.Remove(draggedPictureBox);
                targetPictureBox.AllowDrop = false;
            }
        }

        private void PictureBox_Click(object sender, EventArgs e)
        {

            PictureBox pictureBox = sender as PictureBox;
            if (pictureBox != null && pictureBox.ImageLocation != "gray.png")
            {
                PictureBox newPictureBox = new PictureBox
                {
                    Size = new Size(20, 30),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = pictureBox.Image,
                    ImageLocation = pictureBox.ImageLocation,
                    Location = new Point(10 + offset  * 40, 10),
                   
                };
                newPictureBox.MouseDown += PictureBox_MouseDown;
                panel1.Controls.Add(newPictureBox);
                panel1List.Add(newPictureBox);
                pictureBox.Image = Image.FromFile("gray.png");
                pictureBox.ImageLocation = "gray.png";
                pictureBox.AllowDrop = true;
                
            }
            offset++;
            if (offset >= panel1NumOfOrbs)
            {
                offset = 1;
            }
           
        }

        public void Evaulate(List<string> panel2CurrentRowColors)
        {
            colorMatch = 0;
            indexMatch = 0;
            if (level < 4)
                if (panel2CurrentRowColors.Count != savedColorsName.Count)
                {
                    MessageBox.Show("Hiba: A színek száma nem egyezik.");
                    return;
                }
            for (int i = 0; i < panel2CurrentRowColors.Count; i++)
            {
                if (savedColorsName.Contains(panel2CurrentRowColors[i]))
                {
                    colorMatch++;
                    if (savedColorsName[i] == panel2CurrentRowColors[i])
                    {
                        indexMatch++;
                    }
                }
            }
        }

        private void PrintLists()
        {
            MessageBox.Show("savedColorsName string:\n" + string.Join(", ", savedColorsName));
            MessageBox.Show("panel2Colors string:\n" + string.Join(", ", panel2Colors));
        }

        private void NewGame()
        {
            level++;
            label1.Text = $"Szint: {level}";

            colorMatch = 0;
            indexMatch = 0;
            colors = new List<string>
            {
                "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Pink", "Brown"
            };
            panel2CurrentRow = 0;
            ListClear();
            GeneratePictureBoxes();
            Panel2FillGray();

            if (level % 4 == 0)
            {
                panel1NumOfOrbs++;
            }
        }

        public void ListClear()
        {
            panel1.Controls.Clear();
            panel1List.Clear();
            panel2List.Clear();
            savedColorsName.Clear();
            panel2Colors.Clear();
            initialPanel1Images.Clear();
            tempPanel1Images.Clear(); // Ideiglenes lista ürítése
            panel2.Controls.Clear();
        }
    }
}
