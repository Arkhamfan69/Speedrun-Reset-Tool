using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SpeedrunResetTool
{
    public partial class Form1 : Form
    {
        private class GameData
        {
            public List<string> SaveFiles { get; set; } = new List<string>();
            public string ExePath { get; set; } = "";
        }

        public class GameDataSerializable
        {
            public required string Name { get; set; }
            public required string ExePath { get; set; }
            public required List<string> SaveFiles { get; set; }
        }

        private Dictionary<string, GameData> gamesSaveFiles = new Dictionary<string, GameData>();
        private System.Windows.Forms.Timer keyTimer = new System.Windows.Forms.Timer();
        private bool triggered = false;

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);

        public Form1()
        {
            InitializeComponent();
            LoadGameData();
            keyTimer.Interval = 10;
            keyTimer.Tick += KeyCheck;
            keyTimer.Start();
        }

        private void LoadGameData()
        {
            if (!File.Exists("gamedata.json"))
                return;
            try
            {
                string json = File.ReadAllText("gamedata.json");
                var list = JsonSerializer.Deserialize<List<GameDataSerializable>>(json);
                if (list == null)
                    return;
                gamesSaveFiles.Clear();
                foreach (var item in list)
                {
                    gamesSaveFiles[item.Name] = new GameData
                    {
                        ExePath = item.ExePath,
                        SaveFiles = item.SaveFiles ?? new List<string>()
                    };
                }
            }
            catch { }
        }

        private void SaveGameData()
        {
            var list = new List<GameDataSerializable>();
            foreach (var kvp in gamesSaveFiles)
            {
                list.Add(new GameDataSerializable
                {
                    Name = kvp.Key,
                    ExePath = kvp.Value.ExePath,
                    SaveFiles = kvp.Value.SaveFiles
                });
            }
            try
            {
                string json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("gamedata.json", json);
            }
            catch { }
        }

        private void gameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedGame = gameComboBox.SelectedItem?.ToString() ?? "";
            checkedListBox1.Items.Clear();
            if (string.IsNullOrEmpty(selectedGame))
                return;
            if (gamesSaveFiles.ContainsKey(selectedGame))
            {
                foreach (string file in gamesSaveFiles[selectedGame].SaveFiles)
                    checkedListBox1.Items.Add(file);
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
                        gamesSaveFiles[gameName] = new GameData();
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
            string steamCommonDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Steam", "steamapps", "common"
            );
            if (Directory.Exists(steamCommonDir))
                dialog.InitialDirectory = steamCommonDir;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!gamesSaveFiles.ContainsKey(selectedGame))
                {
                    gamesSaveFiles[selectedGame] = new GameData();
                    if (!gameComboBox.Items.Contains(selectedGame))
                        gameComboBox.Items.Add(selectedGame);
                }
                gamesSaveFiles[selectedGame].ExePath = dialog.FileName;
                SaveGameData();
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
            string localAppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (Directory.Exists(localAppDataDir))
                dialog.InitialDirectory = localAppDataDir;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!gamesSaveFiles.ContainsKey(selectedGame))
                {
                    gamesSaveFiles[selectedGame] = new GameData();
                    if (!gameComboBox.Items.Contains(selectedGame))
                        gameComboBox.Items.Add(selectedGame);
                }
                foreach (string file in dialog.FileNames)
                {
                    gamesSaveFiles[selectedGame].SaveFiles.Add(file);
                    checkedListBox1.Items.Add(file);
                }
                SaveGameData();
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
                    try { if (File.Exists(file)) File.Delete(file); } catch { }
                    checkedListBox1.SetItemChecked(i, false);
                }
            }
            SaveGameData();
            MessageBox.Show("Selected save files deleted!");
        }

        private void KeyCheck(object sender, EventArgs e)
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
                return;
            var gameData = gamesSaveFiles[selectedGame];
            var result = MessageBox.Show(
                $"Does '{selectedGame}' need to be closed for save files to restore?",
                "Close Game?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                if (!string.IsNullOrEmpty(gameData.ExePath))
                {
                    CloseGameProcess(gameData.ExePath);
                    System.Threading.Thread.Sleep(500);
                }
            }
            foreach (string file in gameData.SaveFiles)
            {
                try { if (File.Exists(file)) File.Delete(file); }
                catch { }
            }
            gameData.SaveFiles.Clear();
            checkedListBox1.Items.Clear();
            if (result == DialogResult.Yes && !string.IsNullOrEmpty(gameData.ExePath))
            {
                try { Process.Start(gameData.ExePath); }
                catch { }
            }
            MessageBox.Show($"All save files for '{selectedGame}' deleted!");
        }

        private void CloseGameProcess(string exePath)
        {
            string exeName = Path.GetFileNameWithoutExtension(exePath);
            var processes = Process.GetProcessesByName(exeName);
            foreach (var p in processes)
            {
                try
                {
                    p.Kill();
                    p.WaitForExit();
                }
                catch { }
            }
        }
    }
}
