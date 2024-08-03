using j1.j1;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace j1
{
    public partial class Form1 : Form
    {
        private List<PictureBox> pictureBoxes = new List<PictureBox>(); // Lista a PictureBox1 színek tárolására
        private List<PictureBox> pictureBoxesTryList = new List<PictureBox>(); // Lista a panel2 sorokhoz
        private GameModel gameModel;

        public Form1()
        {
            InitializeComponent();

            this.Size = new Size(800, 600);     // Form mérete
            panel1.BackColor = Color.White;     // panel1 háttérszín
            panel2.BackColor = Color.White;     // panel2 háttérszín
            button1.Text = "Kiértékelés";
            gameModel = new GameModel(); // Játék logika inicializálása
            button1.Click += button1_Click;
            //this.Controls.Add(button1);

            // PictureBox-ok létrehozása és hozzáadása a pictureBoxes listához
            for (int i = 0; i < gameModel.LevelNumber; i++)
            {
                PictureBox pictureBox = new PictureBox
                {
                    Size = new Size(30, 20),
                    Location = new Point(10 + i * 40, panel1.Height / 2 - 10),
                    ImageLocation = gameModel.GetImagePath(gameModel.GetRandomColor()),
                    SizeMode = PictureBoxSizeMode.StretchImage
                };

                pictureBox.MouseDown += PictureBox_MouseDown; // MouseDown esemény hozzáadása
                pictureBoxes.Add(pictureBox);
            }

            PlaceRandomPictureBoxesInPanel1(); // PictureBox-ok elhelyezése panel1-ben
            Panel2Fill(); // PictureBox-ok elhelyezése panel2-ben
        }

        private void PlaceRandomPictureBoxesInPanel1()
        {
            foreach (PictureBox pictureBox in pictureBoxes)
            {
                panel1.Controls.Add(pictureBox); // PictureBox hozzáadása panel1-hez
            }
        }

        private void Panel2Fill()
        {
            string grayImagePath = "gray.png";

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    PictureBox pictureBox = new PictureBox
                    {
                        Size = new Size(30, 20),
                        Location = new Point(10 + j * 40, panel2.Height - 30 - i * 30),
                        ImageLocation = grayImagePath,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Enabled = false  // PictureBox tiltása alapértelmezés szerint
                    };

                    if (i == gameModel.CurrentRow) // Csak a legalsó sor PictureBox-ait engedélyezzük
                    {
                        pictureBox.AllowDrop = true;
                        pictureBox.DragEnter += PictureBox_DragEnter;
                        pictureBox.DragDrop += PictureBox_DragDrop;
                        pictureBox.Enabled = true;
                    }

                    panel2.Controls.Add(pictureBox); // PictureBox hozzáadása panel2-höz
                }
            }
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            if (pictureBox != null)
            {
                // Tároljuk az áthúzott PictureBox-ot
                DragDropEffects effect = pictureBox.DoDragDrop(pictureBox, DragDropEffects.Move);
                if (effect == DragDropEffects.Move)
                {
                    // Az áthúzott PictureBox eltávolítása panel1-ről
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
            if (targetPictureBox != null && e.Data.GetDataPresent(typeof(PictureBox)))
            {
                PictureBox draggedPictureBox = e.Data.GetData(typeof(PictureBox)) as PictureBox;
                if (draggedPictureBox != null)
                {
                    // Ellenőrizzük, hogy a cél PictureBox már tartalmaz-e képet
                    if (targetPictureBox.ImageLocation == "gray.png") // Ez az alapértelmezett, üres állapot
                    {
                        targetPictureBox.ImageLocation = draggedPictureBox.ImageLocation;
                        targetPictureBox.AllowDrop = false; // A cél PictureBox már nem fogad el új áthúzást
                    }
                    else
                    {
                        MessageBox.Show("Ez a hely már tartalmaz egy elemet.");
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Üresítjük a pictureBoxesTryList listát, hogy új adatokat tudjunk hozzáadni
            pictureBoxesTryList.Clear();

            // Az aktuális sor PictureBox-ait összegyűjtjük a pictureBoxesTryList listába
            foreach (Control control in panel2.Controls)
            {
                if (control is PictureBox pictureBox &&
                    pictureBox.Location.Y == panel2.Height - 30 - gameModel.CurrentRow * 30)
                {
                    pictureBoxesTryList.Add(pictureBox);
                }
            }

            // A pictureBoxesTryList listából kiolvassuk a színek neveit és elmentjük
            List<string> currentRowColors = pictureBoxesTryList
                .Select(pb => pb.ImageLocation.Replace(".png", "")) // Az ImageLocation-ból eltávolítjuk a ".png" kiterjesztést
                .ToList();

            gameModel.Evaluate(currentRowColors); // Kiértékelés a játékon belül

            MessageBox.Show($"Szín találat: {gameModel.ColorMatch} / {gameModel.LevelNumber}, Pozíció találat: {gameModel.IndexMatch} / {gameModel.LevelNumber}");

            PlaceRandomPictureBoxesInPanel1();  // Újra megjelenítjük a PictureBox-okat a panel1-en

            // Következő sor engedélyezése
            if (gameModel.CurrentRow < 10)
            {
                Panel2Fill(); // Újra töltjük a panel2-t a következő sor engedélyezésével   
                EnableNextRow();       // következő sor engedélyezése
                DisablePrevRow();      // előző sor letiltása

                //  ToDo: hibaüzenet ha nincs kitöltve mind a 4 box
            }
        }

        private void EnableNextRow()    // következő sor engedélyezése
        {
            foreach (Control control in panel2.Controls)
            {
                if (control is PictureBox pictureBox && pictureBox.Location.Y == panel2.Height - 30 - gameModel.CurrentRow * 30)
                {
                    pictureBox.AllowDrop = true;
                    pictureBox.DragEnter += PictureBox_DragEnter;
                    pictureBox.DragDrop += PictureBox_DragDrop;
                    pictureBox.Enabled = true;
                }
            }
        }

        private void DisablePrevRow()   // előző sorok tiltása
        {
            foreach (Control control in panel2.Controls)
            {
                if (control is PictureBox pictureBox && pictureBox.Location.Y == panel2.Height - gameModel.CurrentRow * 30)
                {
                    pictureBox.AllowDrop = false;
                    pictureBox.BackColor = Color.LightGray;
                }
            }
        }
    }
}
