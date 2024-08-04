using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MasterMindGame
{
    public partial class Form1 : Form
    {
        public List<PictureBox> panel1List = new List<PictureBox>(); // 
        public List<PictureBox> panel2List = new List<PictureBox>(); //
        public List<Color> savedColors = new List<Color>(); // panel1 suffle ToDo!!! csak színek 
        public List<string> savedColorsName = new List<string>();
        public List<string> panel2Colors = new List<string>(); // panel2 csak szinek nevei
        public List<string> colors = new List<string> {
            "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Pink", "Brown" };
        public List<PictureBox> initialPanel1Images = new List<PictureBox>();

        private Random random = new Random();   // 

        public int panel1NumOfOrbs = 4;
        public int colorMatch = 0;
        public int indexMatch = 0;
        public int panel2CurrentRow = 0;
        public int level = 1;




        public Form1()
        {
            InitializeComponent();
            FormSetup();
            GeneratePictureBoxes();
            Panel2FillGray();
            NewGame();



            //}
        }

        public void FormSetup()
        {
            panel1.BackColor = Color.White; // Választható színek
            panel2.BackColor = Color.White; // 
            panel3.BackColor = Color.White; // Eredmények fehér, fekete pöttyök
            button1.Text = "Kiértékelés";
            button1.Click += Button1_Click;
            pictureBox1.AllowDrop = true;
            label1.Text = $"Szint: {level}";

        }



        private string GetRandomColorFromList()
        {
            int index = random.Next(0, colors.Count); // Véletlenszerű index kiválasztása
            string colorName = colors[index];
            colors.RemoveAt(index);

            return colorName;
        }
        private string GetImagePath(string colorName)
        {
            return $"{colorName}.png";
        }


        //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  
        // Pictureboxok Generálása
        //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //
        //  
        public void GeneratePictureBoxes()
        {
            savedColorsName.Clear();
            initialPanel1Images.Clear();
            panel1.Controls.Clear();
            for (int i = 0; i < panel1NumOfOrbs; i++)
            {
                string colorName = GetRandomColorFromList();
                PictureBox pictureBox = new PictureBox
                {
                    Size = new Size(20, 30),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    ImageLocation = GetImagePath(colorName)
                };
                pictureBox.Location = new Point(i * 40, 10); // Elhelyezés például vízszintesen
                pictureBox.MouseDown += PictureBox_MouseDown;
                panel1.Controls.Add(pictureBox);
                panel1List.Add(pictureBox);

                savedColorsName.Add(colorName); // Szín hozzáadása a listához
                initialPanel1Images.Add(pictureBox);
            }
            if (level % 4 == 0)
            {
                string extraColorName = GetRandomColorFromList();
                PictureBox extraPictureBox = new PictureBox
                {
                    Size = new Size(20, 30),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    ImageLocation = GetImagePath(extraColorName)
                };
                extraPictureBox.Location = new Point(panel1NumOfOrbs * 40, 10); // Elhelyezés például vízszintesen
                extraPictureBox.MouseDown += PictureBox_MouseDown;
                panel1.Controls.Add(extraPictureBox);
                panel1List.Add(extraPictureBox);
                initialPanel1Images.Add(extraPictureBox);
            }
        }

        public void Panel2FillGray()
        {
            string GrayImagePath = "gray.png";

            for (int i = 0; i < 10; i++)   // sorok száma
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
                    if (i == panel2CurrentRow) // csak az első sor aktív, csak oda lehet húzni panel 1-ből
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
        private void RestorePanel1Images()
        {
            panel1.Controls.Clear(); // Tisztítja a panel1-et
            panel1List.Clear(); // Tisztítja a panel1List-et

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

        //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  
        // Egér események 
        //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //  //
        //  
        private void Button1_Click(object sender, EventArgs e)
        {
            panel2Colors.Clear();
            foreach (Control control in panel2.Controls)
            {
                if (control is PictureBox pictureBox && pictureBox.Location.Y == panel2.Height - 30 - panel2CurrentRow * 30)
                {
                    string colorName = Path.GetFileNameWithoutExtension(pictureBox.ImageLocation); // .png eltávolítása
                    panel2Colors.Add(colorName);
                }
            }

            Evaulate(panel2Colors);

            MessageBox.Show($"Szín találat: {colorMatch} /4, Pozíció találat: {indexMatch} / 4");

            panel2List.Clear();
            if (colorMatch == 4 && indexMatch == 4)
            {
                MessageBox.Show("Gratulálok! Minden szín és pozíció helyes!");
                NewGame();
            }
            else
            {
                // Következő sor aktiválása
                panel2CurrentRow++;
                if (panel2CurrentRow >= 10)
                {
                    MessageBox.Show("Vége a játéknak!");
                    NewGame();
                }
                else
                {
                    //Panel2FillGray();
                    RestorePanel1Images();
                    EnableCurrentRow();
                }
            }

            PrintLists();
        }



        private void PictureBox_MouseDown(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            if (pictureBox != null)
            {
                DragDropEffects effect = pictureBox.DoDragDrop(pictureBox, DragDropEffects.Move);
                if (effect == DragDropEffects.Move)
                {
                    panel1.Controls.Remove(pictureBox);
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
            if (pictureBox != null && pictureBox.Image != null)
            {

                if (pictureBox.ImageLocation != "gray.png")
                {

                    PictureBox newPictureBox = new PictureBox
                    {
                        Size = new Size(20, 30),
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Image = pictureBox.Image,
                        ImageLocation = pictureBox.ImageLocation,
                        Margin = new Padding(10)
                    };
                    newPictureBox.MouseDown += PictureBox_MouseDown;
                    panel1.Controls.Add(newPictureBox);
                    panel1List.Add(newPictureBox);


                    pictureBox.Image = Image.FromFile("gray.png");
                    pictureBox.ImageLocation = "gray.png";
                    pictureBox.AllowDrop = true;
                }
            }
        }
        public void Evaulate(List<string> panel2CurrentRowColors)
        {
            colorMatch = 0;
            indexMatch = 0;

            if (panel2CurrentRowColors.Count != savedColorsName.Count)
            {
                MessageBox.Show("Hiba: A színek száma nem egyezik.");
                return;
            }

            for (int i = 0; i < panel2CurrentRowColors.Count; i++)
            {
                string panelColor = panel2CurrentRowColors[i];
                if (savedColorsName.Contains(panelColor))
                {
                    colorMatch++;
                    if (savedColorsName[i] == panelColor)
                    {
                        indexMatch++;
                    }
                }
            }
        }
        private void PrintLists()
        {


            MessageBox.Show("savedColorsName  string:\n" + string.Join(", ", savedColorsName)); //orange,brown,purple,red
            MessageBox.Show("panel2Colors  string:\n" + string.Join(", ", panel2Colors)); //gray,gray,gray,gray

        }

        
        private void NewGame()
        {   /*
            
            panel1.Controls.Clear(); // Tisztítja a panel1-t
            panel1List.Clear(); // Tisztítja a panel1List-et

            savedColorsName.Clear(); // Tisztítja a mentett színeket

            GeneratePictureBoxes(); // Generál új képeket

            panel2CurrentRow = 0; // Visszaállítja az aktuális sort
            //Panel2FillGray(); // Frissíti a panel2-t
            level++;

           /if (level % 4 == 0)
            {
                panel1NumOfOrbs++;
            }
        */
        }
        
        }
}

