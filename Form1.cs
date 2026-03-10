using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SpeedrunResetTool
{
    public partial class Form1 : Form
    {
        private class GameData
        {
            public List<string> SaveFiles { get; set; } = new List<string>();
            public string ExePath { get; set; } = "";
        }

        private Dictionary<string, GameData> gamesSaveFiles = new Dictionary<string, GameData>();
        private System.Windows.Forms.Timer keyTimer = new System.Windows.Forms.Timer();
        private bool triggered = false;

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);

        public Form1()
        {
            InitializeComponent();

            keyTimer.Interval = 10;
            keyTimer.Tick += KeyCheck;
            keyTimer.Start();
        }

        private void gameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedGame = gameComboBox.SelectedItem?.ToString() ?? "";
            if (string.IsNullOrEmpty(selectedGame))
            {
                checkedListBox1.Items.Clear();
                return;
            }

            if (gamesSaveFiles.ContainsKey(selectedGame))
            {
                checkedListBox1.Items.Clear();
                foreach (string file in gamesSaveFiles[selectedGame].SaveFiles)
                {
                    checkedListBox1.Items.Add(file);
                }
            }
            else
            {
                checkedListBox1.Items.Clear();
            }
        }

        private void gameComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.Handled = true;
                string gameName = gameComboBox.Text.Trim();
                
                if (!string.IsNullOrEmpty(gameName) && !gameComboBox.Items.Contains(gameName))
                {
                    gameComboBox.Items.Add(gameName);
                    if (!gamesSaveFiles.ContainsKey(gameName))
                    {
                        gamesSaveFiles[gameName] = new GameData();
                    }
                    gameComboBox.SelectedItem = gameName;
                }
            }
        }

        private void setGameExeButton_Click(object sender, EventArgs e)
        {
            string selectedGame = gameComboBox.Text.Trim();
            if (string.IsNullOrEmpty(selectedGame))
            {
                MessageBox.Show("Please select or type a game name first!");
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
            dialog.Title = $"Select .exe for {selectedGame}";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!gamesSaveFiles.ContainsKey(selectedGame))
                {
                    gamesSaveFiles[selectedGame] = new GameData();
                    gameComboBox.Items.Add(selectedGame);
                }

                gamesSaveFiles[selectedGame].ExePath = dialog.FileName;
                MessageBox.Show($"EXE set to: {dialog.FileName}");
            }
        }

        private void addFileButton_Click(object sender, EventArgs e)
        {
            string selectedGame = gameComboBox.Text.Trim();
            if (string.IsNullOrEmpty(selectedGame))
            {
                MessageBox.Show("Please select or type a game name first!");
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!gamesSaveFiles.ContainsKey(selectedGame))
                {
                    gamesSaveFiles[selectedGame] = new GameData();
                    gameComboBox.Items.Add(selectedGame);
                }

                foreach (string file in dialog.FileNames)
                {
                    gamesSaveFiles[selectedGame].SaveFiles.Add(file);
                    checkedListBox1.Items.Add(file);
                }
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            string selectedGame = gameComboBox.SelectedItem?.ToString() ?? "";
            if (string.IsNullOrEmpty(selectedGame) || !gamesSaveFiles.ContainsKey(selectedGame))
            {
                MessageBox.Show("Please select a game first!");
                return;
            }

            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    string file = checkedListBox1.Items[i]?.ToString() ?? "";
                    try
                    {
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Could not delete {file}: {ex.Message}");
                    }

                    checkedListBox1.SetItemChecked(i, false);
                }
            }

            MessageBox.Show("Selected save files deleted!");
        }

        private void KeyCheck(object? sender, EventArgs e)
        {
            bool pPressed = (GetAsyncKeyState(Keys.P) & 0x8000) != 0;
            bool lPressed = (GetAsyncKeyState(Keys.L) & 0x8000) != 0;

            if (pPressed && lPressed)
            {
                if (!triggered)
                {
                    triggered = true;
                    DeleteAllForGame();
                }
            }
            else
            {
                triggered = false;
            }
        }

        private void DeleteAllForGame()
        {
            string selectedGame = gameComboBox.SelectedItem?.ToString() ?? "";
            if (string.IsNullOrEmpty(selectedGame) || !gamesSaveFiles.ContainsKey(selectedGame))
            {
                return;
            }

            GameData gameData = gamesSaveFiles[selectedGame];

            DialogResult result = MessageBox.Show(
                $"Does '{selectedGame}' need to be closed for save files to restore?",
                "Close Game?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (!string.IsNullOrEmpty(gameData.ExePath))
                {
                    CloseGameProcess();
                    System.Threading.Thread.Sleep(500);
                }
            }

            foreach (string file in gameData.SaveFiles)
            {
                try
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                catch { }
            }

            gameData.SaveFiles.Clear();
            checkedListBox1.Items.Clear();

            if (result == DialogResult.Yes && !string.IsNullOrEmpty(gameData.ExePath))
            {
                try
                {
                    _ = Process.Start(gameData.ExePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not relaunch game: {ex.Message}");
                }
            }

            MessageBox.Show($"All save files for '{selectedGame}' deleted!");
        }

        private void CloseGameProcess()
        {
            string selectedGame = gameComboBox.SelectedItem?.ToString() ?? "";
            if (string.IsNullOrEmpty(selectedGame) || !gamesSaveFiles.ContainsKey(selectedGame))
                return;

            string exePath = gamesSaveFiles[selectedGame].ExePath;
            if (string.IsNullOrEmpty(exePath))
                return;

            string exeName = Path.GetFileNameWithoutExtension(exePath) ?? "";
            if (string.IsNullOrEmpty(exeName))
                return;

            Process[] processes = Process.GetProcessesByName(exeName);

            foreach (Process process in processes)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch { }
            }
        }
    }

}
