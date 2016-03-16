using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Life
{
    public partial class MainForm : Form
    {
        private readonly ControlForm sForm;
        private readonly LifeSaver lifeSaver = new LifeSaver();

        public MainForm()
        {
            InitializeComponent();
            lifePanel.CellsUpdated += DrawPanel1OnCycleCalculated;
            lifePanel.GameModeChanged += LifePanelOnGameModeChanged;
            sForm = new ControlForm(lifePanel);
            lifePanel.GameMode = GameMode.GameOfLife;
        }

        private void LifePanelOnGameModeChanged(object sender, EventArgs eventArgs)
        {
            Text = String.Format("Life - {0}", GetTitle(lifePanel.GameMode));
        }

        private string GetTitle(GameMode gameMode)
        {
            switch (gameMode)
            {
                case GameMode.GameOfLife:
                    return "Game of Life";
                case GameMode.WireWorld:
                    return "Wires";
                default:
                    return "None";
            }
        }

        private void DrawPanel1OnCycleCalculated(object sender, EventArgs e)
        {
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            lblGenerations.Text = String.Format("Generations: {0:D6}", lifePanel.Generation);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            lifePanel.CalculateCycle();
            //UpdateLabel();
        }

        private void trackUpdate_Scroll(object sender, EventArgs e)
        {
            timer.Interval = trackUpdate.Value;
            lblUpdateRate.Text = String.Format("{0} ms", trackUpdate.Value);
        }

        private void toolStripButtonToggle_Click(object sender, EventArgs e)
        {
            timer.Enabled = !timer.Enabled;
            if (timer.Enabled)
            {
                toolStripButtonToggle.Image = global::Life.Properties.Resources.PauseHS;
            }
            else
            {
                toolStripButtonToggle.Image = global::Life.Properties.Resources.PlayHS;
            }
        }

        private void Stop()
        {
            lifePanel.Generation = 0;
            UpdateLabel();
            timer.Enabled = false;
            toolStripButtonToggle.Image = global::Life.Properties.Resources.PlayHS;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            lifePanel.CalculateCycle();
        }

        private void toolStripButtonStop_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void SaveFile()
        {            
            if (Directory.Exists(Application.StartupPath + @"\GameOfLifeSaves") == false)
            {
                Directory.CreateDirectory(Application.StartupPath + @"\GameOfLifeSaves");
            }

            String Name = Application.StartupPath + @"\GameOfLifeSaves" + "life_" + GetName(3) + ".gol";
            var Writer = new BinaryWriter(File.Open(Name, FileMode.Create));

            try
            {
                Writer.Write(lifeSaver.Save(lifePanel.Cells, lifePanel.GameMode));
            }
            finally
            {
                Writer.Close();
            }

            Clipboard.SetText(lifeSaver.Save(lifePanel.Cells, lifePanel.GameMode));
        }

        private void toolStripButtonLoad_Click(object sender, EventArgs e)
        {
            LoadLife();
        }

        public static string GetName(int x) //We use it to generate part of the file name with 3 symbols
        {
            string pass = "";
            var r = new Random();
            while (pass.Length < x)
            {
                Char c = (char)r.Next(33, 125);
                if (Char.IsLetterOrDigit(c))
                    pass += c;
            }
            return pass;
        }

        private void LoadLife()
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            String PathName = Application.StartupPath + @"\GameOfLifeSaves";
            String clipboard = Clipboard.GetText();
            
            LoadWorld(clipboard);

            openFileDialog1.InitialDirectory = PathName; //Path.Combine(Path.GetDirectoryName(Application.ExecutablePath))
            openFileDialog1.Filter = "gol files (*.gol)|*.gol|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            // Insert code to read the stream here.
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void LoadWorld(String base64Str)
        {
            GameMode gameMode;
            Dictionary<Point, Cell> cells = lifeSaver.Load(base64Str, out gameMode);
            if (cells.Count > 0)
            {
                lifePanel.GameMode = gameMode;
                lifePanel.Cells = cells;
                lifePanel.ClearDeadCells();
                lifePanel.Invalidate();
            }
        }

        private void toolStripButtonRandom_Click(object sender, EventArgs e)
        {
            if (lifePanel.GameMode == GameMode.GameOfLife)
            {
                lifePanel.RandomLife();
            }
        }

        private void ruleEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!sForm.Visible)
            {
                sForm.Show();
            }
        }

        private void toolStripButtonNewLife_Click(object sender, EventArgs e)
        {
            lifePanel.Restart();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lifePanel.Restart();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadLife();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void gameOfLifeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lifePanel.Restart();
            lifePanel.GameMode = GameMode.GameOfLife;
        }

        private void wireworldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lifePanel.Restart();
            lifePanel.GameMode = GameMode.WireWorld;
        }
    }
}