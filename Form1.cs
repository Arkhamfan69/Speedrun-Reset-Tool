using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SpeedrunResetTool
{
    public partial class Form1 : Form
    {
        private sealed class AppConfig
        {
            public List<GameData> Games { get; set; } = new List<GameData>();
            public List<Keys> DeleteHotkeys { get; set; } = new List<Keys> { Keys.Delete };
        }

        private class GameData
        {
            public string GameName { get; set; } = "";
            public List<string> SaveFiles { get; set; } = new List<string>();
            public string ExePath { get; set; } = "";
            public bool LaunchWithSteam { get; set; }
            public string SteamAppId { get; set; } = "";
        }

        private readonly string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpeedrunResetTool", "config.json");
        private AppConfig appConfig = new AppConfig();
        private Dictionary<string, GameData> gamesSaveFiles = new Dictionary<string, GameData>();
        private System.Windows.Forms.Timer keyTimer = new System.Windows.Forms.Timer();
        private bool triggered = false;
        private bool isRecordingHotkey = false;
        private readonly List<Keys> currentRecordingKeys = new List<Keys>();

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);

        public Form1()
        {
            InitializeComponent();
            LoadConfig();

            keyTimer.Interval = 10;
            keyTimer.Tick += KeyCheck;
            keyTimer.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadConfig();
        }

        private void gameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedGame = gameComboBox.SelectedItem?.ToString() ?? "";
            if (string.IsNullOrEmpty(selectedGame))
            {
                checkedListBox1.Items.Clear();
                UpdateSettingsControls(selectedGame);
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

            UpdateSettingsControls(selectedGame);
        }

        private void manageGamesButton_Click(object sender, EventArgs e)
        {
            if (manageGroupBox == null)
                return;

            manageGroupBox.Visible = !manageGroupBox.Visible;
            manageGamesButton.Text = manageGroupBox.Visible ? "Hide Manage" : "Manage Games";
        }

        private void createGameButton_Click(object sender, EventArgs e)
        {
            string gameName = manageGameNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(gameName))
            {
                MessageBox.Show("Please enter a game name.");
                return;
            }

            if (!gamesSaveFiles.ContainsKey(gameName))
            {
                gamesSaveFiles[gameName] = new GameData { GameName = gameName };
                gameComboBox.Items.Add(gameName);
            }

            gameComboBox.SelectedItem = gameName;
            SaveConfig();
            MessageBox.Show($"Game '{gameName}' is now available for management.");
        }

        private void removeGameButton_Click(object sender, EventArgs e)
        {
            string selectedGame = gameComboBox.SelectedItem?.ToString() ?? manageGameNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(selectedGame))
            {
                MessageBox.Show("Please select or enter a game name to remove.");
                return;
            }

            if (!gamesSaveFiles.ContainsKey(selectedGame))
            {
                MessageBox.Show($"Game '{selectedGame}' is not in the list.");
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Remove '{selectedGame}' from the game list and forget its settings?",
                "Confirm Remove",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            gamesSaveFiles.Remove(selectedGame);
            gameComboBox.Items.Remove(selectedGame);

            if (gameComboBox.Items.Count > 0)
            {
                gameComboBox.SelectedIndex = 0;
            }
            else
            {
                gameComboBox.SelectedIndex = -1;
                checkedListBox1.Items.Clear();
                UpdateSettingsControls(string.Empty);
            }

            SaveConfig();
            MessageBox.Show($"Game '{selectedGame}' has been removed.");
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
                        gamesSaveFiles[gameName] = new GameData { GameName = gameName };
                    }
                    gameComboBox.SelectedItem = gameName;
                    SaveConfig();
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
                string path = dialog.FileName;

                if (AntiPiracy(path))
                {
                    MessageBox.Show("Selected executable appears to be from a pirated or unauthorized source. Please select a legitimate copy.");
                    return;
                }

                if (!gamesSaveFiles.ContainsKey(selectedGame))
                {
                    gamesSaveFiles[selectedGame] = new GameData { GameName = selectedGame };
                    gameComboBox.Items.Add(selectedGame);
                }

                gamesSaveFiles[selectedGame].ExePath = path;
                ApplySteamAutoDetect(gamesSaveFiles[selectedGame]);
                UpdateSettingsControls(selectedGame);
                SaveConfig();
                MessageBox.Show($"EXE set to: {path}");
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
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            dialog.Title = "Select Save File";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!gamesSaveFiles.ContainsKey(selectedGame))
                {
                    gamesSaveFiles[selectedGame] = new GameData { GameName = selectedGame };
                    gameComboBox.Items.Add(selectedGame);
                }

                var gameData = gamesSaveFiles[selectedGame];
                foreach (string file in dialog.FileNames)
                {
                    if (!gameData.SaveFiles.Contains(file))
                    {
                        gameData.SaveFiles.Add(file);
                        checkedListBox1.Items.Add(file);
                    }
                }

                SaveConfig();
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

            var gameData = gamesSaveFiles[selectedGame];
            var checkedIndices = checkedListBox1.CheckedIndices.Cast<int>().OrderByDescending(i => i).ToList();
            if (!checkedIndices.Any())
            {
                MessageBox.Show("Please check at least one save file to delete.");
                return;
            }

            foreach (int i in checkedIndices)
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

                gameData.SaveFiles.Remove(file);
                checkedListBox1.Items.RemoveAt(i);
            }

            SaveConfig();
            MessageBox.Show("Selected save file deleted!");
        }

        private void KeyCheck(object? sender, EventArgs e)
        {
            if (appConfig.DeleteHotkeys == null || appConfig.DeleteHotkeys.Count == 0)
            {
                triggered = false;
                return;
            }

            bool allPressed = true;
            foreach (var key in appConfig.DeleteHotkeys)
            {
                if ((GetAsyncKeyState(key) & 0x8000) == 0)
                {
                    allPressed = false;
                    break;
                }
            }

            if (allPressed)
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

            foreach (string file in gameData.SaveFiles.ToList())
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
            SaveConfig();

            if (result == DialogResult.Yes)
            {
                if (!LaunchGame(gameData))
                {
                    MessageBox.Show("No launch settings configured for this game. Set a Steam AppID or EXE path.");
                }
            }

            MessageBox.Show($"All save files for '{selectedGame}' deleted!");
        }

        private void LoadConfig()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? string.Empty);
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    appConfig = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    }) ?? new AppConfig();
                }
                else
                {
                    appConfig = new AppConfig();
                }

                gamesSaveFiles.Clear();
                gameComboBox.Items.Clear();

                foreach (var gameData in appConfig.Games)
                {
                    if (string.IsNullOrWhiteSpace(gameData.GameName))
                        continue;

                    gamesSaveFiles[gameData.GameName] = gameData;
                    if (!gameComboBox.Items.Contains(gameData.GameName))
                        gameComboBox.Items.Add(gameData.GameName);
                }

                if (gameComboBox.Items.Count > 0)
                {
                    gameComboBox.SelectedIndex = 0;
                    UpdateSettingsControls(gameComboBox.SelectedItem?.ToString() ?? string.Empty);
                }

                UpdateHotkeyText();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load config: {ex.Message}");
            }
        }

        private void SaveConfig()
        {
            try
            {
                appConfig.Games.Clear();
                foreach (var kvp in gamesSaveFiles)
                {
                    kvp.Value.GameName = kvp.Key;
                    appConfig.Games.Add(kvp.Value);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? string.Empty);
                File.WriteAllText(configPath, JsonSerializer.Serialize(appConfig, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save config: {ex.Message}");
            }
        }

        private void UpdateHotkeyText()
        {
            if (hotkeyTextBox == null)
                return;

            hotkeyTextBox.Text = appConfig.DeleteHotkeys == null || appConfig.DeleteHotkeys.Count == 0
                ? "None"
                : FormatHotkey(appConfig.DeleteHotkeys);
        }

        private string FormatHotkey(List<Keys> keys)
        {
            if (keys == null || keys.Count == 0)
                return "None";

            return string.Join(" + ", keys.Select(k => k.ToString()));
        }

        private void UpdateSettingsControls(string selectedGame)
        {
            if (gamesSaveFiles.TryGetValue(selectedGame, out var gameData))
            {
                steamAppIdTextBox.Text = gameData.SteamAppId;
                launchWithSteamCheckBox.Checked = gameData.LaunchWithSteam;
            }
            else
            {
                steamAppIdTextBox.Text = string.Empty;
                launchWithSteamCheckBox.Checked = false;
            }
        }

        private void ApplySteamAutoDetect(GameData gameData)
        {
            if (gameData == null || string.IsNullOrWhiteSpace(gameData.ExePath))
                return;

            string? detectedAppId = TryDetectSteamAppId(gameData.ExePath);
            if (!string.IsNullOrWhiteSpace(detectedAppId))
            {
                gameData.SteamAppId = detectedAppId;
                gameData.LaunchWithSteam = true;
            }
        }

        private void saveSteamButton_Click(object sender, EventArgs e)
        {
            string selectedGame = gameComboBox.SelectedItem?.ToString() ?? gameComboBox.Text.Trim();
            if (string.IsNullOrEmpty(selectedGame))
            {
                MessageBox.Show("Please select a game first.");
                return;
            }

            if (!gamesSaveFiles.ContainsKey(selectedGame))
            {
                gamesSaveFiles[selectedGame] = new GameData { GameName = selectedGame };
                if (!gameComboBox.Items.Contains(selectedGame))
                    gameComboBox.Items.Add(selectedGame);
            }

            var gameData = gamesSaveFiles[selectedGame];
            string manualAppId = steamAppIdTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(manualAppId))
            {
                ApplySteamAutoDetect(gameData);
            }
            else
            {
                gameData.SteamAppId = manualAppId;
            }

            gameData.LaunchWithSteam = launchWithSteamCheckBox.Checked;
            if (!string.IsNullOrWhiteSpace(gameData.SteamAppId))
            {
                gameData.LaunchWithSteam = true;
            }

            UpdateSettingsControls(selectedGame);
            SaveConfig();
            MessageBox.Show("Steam launch settings saved.");
        }

        private void recordHotkeyButton_Click(object sender, EventArgs e)
        {
            currentRecordingKeys.Clear();
            isRecordingHotkey = true;
            hotkeyTextBox.Text = "Press desired hotkey...";
            hotkeyTextBox.Focus();
        }

        private void hotkeyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isRecordingHotkey)
                return;

            if (e.KeyCode == Keys.Enter)
            {
                if (currentRecordingKeys.Count > 0)
                {
                    appConfig.DeleteHotkeys = new List<Keys>(currentRecordingKeys);
                    SaveConfig();
                    UpdateHotkeyText();
                }
                isRecordingHotkey = false;
                currentRecordingKeys.Clear();
                e.SuppressKeyPress = true;
                return;
            }

            if (!currentRecordingKeys.Contains(e.KeyCode) && e.KeyCode != Keys.None)
            {
                currentRecordingKeys.Add(e.KeyCode);
                hotkeyTextBox.Text = FormatHotkey(currentRecordingKeys);
            }
            e.SuppressKeyPress = true;
        }

        private void launchGameButton_Click(object sender, EventArgs e)
        {
            string selectedGame = gameComboBox.SelectedItem?.ToString() ?? "";
            if (string.IsNullOrEmpty(selectedGame) || !gamesSaveFiles.ContainsKey(selectedGame))
            {
                MessageBox.Show("Please select a game first.");
                return;
            }

            var gameData = gamesSaveFiles[selectedGame];
            if (LaunchGame(gameData))
            {
                return;
            }

            MessageBox.Show("No launch settings configured for this game. Set a Steam AppID or EXE path.");
        }

        private bool LaunchGame(GameData gameData)
        {
            ApplySteamAutoDetect(gameData);

            if (!string.IsNullOrWhiteSpace(gameData.SteamAppId))
            {
                StartProcess($"steam://rungameid/{gameData.SteamAppId}");
                return true;
            }

            if (!string.IsNullOrWhiteSpace(gameData.ExePath))
            {
                StartProcess(gameData.ExePath);
                return true;
            }

            return false;
        }

        private void StartProcess(string pathOrUrl)
        {
            try
            {
                var startInfo = new ProcessStartInfo(pathOrUrl) { UseShellExecute = true };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not launch game: {ex.Message}");
            }
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

        private bool AntiPiracy(string exePath)
        {
            string[] suspiciousKeywords = { "Steamrip", "Pirated", "Cracked", "Unlicensed" };

            foreach (var keyword in suspiciousKeywords)
            {
                if (exePath.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        private string? TryDetectSteamAppId(string exePath)
        {
            try
            {
                string fullExePath = Path.GetFullPath(exePath);
                foreach (string libraryFolder in GetSteamLibraryFolders())
                {
                    string steamAppsFolder = Path.Combine(libraryFolder, "steamapps");
                    if (!Directory.Exists(steamAppsFolder))
                        continue;

                    foreach (string manifestPath in Directory.EnumerateFiles(steamAppsFolder, "appmanifest_*.acf"))
                    {
                        if (TryReadSteamManifest(manifestPath, out string? appId, out string? installDir) &&
                            !string.IsNullOrWhiteSpace(appId) &&
                            !string.IsNullOrWhiteSpace(installDir))
                        {
                            string installedGamePath = Path.Combine(steamAppsFolder, "common", installDir);
                            if (IsPathUnderDirectory(fullExePath, installedGamePath))
                            {
                                return appId;
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private IEnumerable<string> GetSteamLibraryFolders()
        {
            var folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddFolder(string? folderPath)
            {
                if (string.IsNullOrWhiteSpace(folderPath))
                    return;

                try
                {
                    string fullPath = Path.GetFullPath(folderPath);
                    if (Directory.Exists(fullPath))
                    {
                        folders.Add(fullPath);
                    }
                }
                catch
                {
                }
            }

            AddFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"));
            AddFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam"));
            AddFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Steam"));

            var queue = folders.ToList();
            for (int i = 0; i < queue.Count; i++)
            {
                string steamFolder = queue[i];
                string libraryFoldersPath = Path.Combine(steamFolder, "steamapps", "libraryfolders.vdf");
                if (!File.Exists(libraryFoldersPath))
                    continue;

                string content = File.ReadAllText(libraryFoldersPath);
                foreach (Match match in Regex.Matches(content, "\\\"path\\\"\\s+\\\"(?<path>[^\\\"]+)\\\"", RegexOptions.IgnoreCase))
                {
                    string libraryFolder = match.Groups["path"].Value.Replace("\\\\", "\\");
                    if (folders.Add(libraryFolder))
                    {
                        queue.Add(libraryFolder);
                    }
                }
            }

            return queue;
        }

        private bool TryReadSteamManifest(string manifestPath, out string? appId, out string? installDir)
        {
            appId = null;
            installDir = null;

            try
            {
                string content = File.ReadAllText(manifestPath);
                Match appIdMatch = Regex.Match(content, "\\\"appid\\\"\\s+\\\"(?<value>\\d+)\\\"", RegexOptions.IgnoreCase);
                Match installDirMatch = Regex.Match(content, "\\\"installdir\\\"\\s+\\\"(?<value>[^\\\"]+)\\\"", RegexOptions.IgnoreCase);

                if (appIdMatch.Success)
                {
                    appId = appIdMatch.Groups["value"].Value;
                }

                if (installDirMatch.Success)
                {
                    installDir = installDirMatch.Groups["value"].Value;
                }

                return appIdMatch.Success || installDirMatch.Success;
            }
            catch
            {
                return false;
            }
        }

        private bool IsPathUnderDirectory(string path, string directory)
        {
            try
            {
                string fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string fullDirectory = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                return fullPath.StartsWith(fullDirectory, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
