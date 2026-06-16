using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SpeedrunResetTool
{
    partial class Form1
    {
        private IContainer components = null;
        private Label gameLabel;
        private ComboBox gameComboBox;
        private Button manageGamesButton;
        private GroupBox manageGroupBox;
        private Label manageGameNameLabel;
        private TextBox manageGameNameTextBox;
        private Button createGameButton;
        private Button removeGameButton;
        private Button setGameExeButton;
        private Button addFileButton;
        private Label steamLabel;
        private TextBox steamAppIdTextBox;
        private CheckBox launchWithSteamCheckBox;
        private Button saveSteamButton;
        private Button deleteButton;
        private Button launchGameButton;
        private Label hotkeyLabel;
        private TextBox hotkeyTextBox;
        private Button recordHotkeyButton;
        private CheckedListBox checkedListBox1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.gameLabel = new Label();
            this.gameComboBox = new ComboBox();
            this.manageGamesButton = new Button();
            this.launchGameButton = new Button();
            this.deleteButton = new Button();
            this.hotkeyLabel = new Label();
            this.hotkeyTextBox = new TextBox();
            this.recordHotkeyButton = new Button();
            this.checkedListBox1 = new CheckedListBox();
            this.manageGroupBox = new GroupBox();
            this.manageGameNameLabel = new Label();
            this.manageGameNameTextBox = new TextBox();
            this.createGameButton = new Button();
            this.removeGameButton = new Button();
            this.setGameExeButton = new Button();
            this.addFileButton = new Button();
            this.steamLabel = new Label();
            this.steamAppIdTextBox = new TextBox();
            this.launchWithSteamCheckBox = new CheckBox();
            this.saveSteamButton = new Button();

            this.SuspendLayout();

            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.ClientSize = new Size(960, 600);
            this.MinimumSize = new Size(900, 560);
            this.StartPosition = FormStartPosition.CenterScreen;

            this.gameLabel.Location = new Point(16, 16);
            this.gameLabel.Size = new Size(100, 20);
            this.gameLabel.Text = "Select Game:";
            this.gameLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            this.gameComboBox.Location = new Point(120, 12);
            this.gameComboBox.Size = new Size(360, 23);
            this.gameComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this.gameComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.gameComboBox.SelectedIndexChanged += new EventHandler(this.gameComboBox_SelectedIndexChanged);
            this.gameComboBox.KeyDown += new KeyEventHandler(this.gameComboBox_KeyDown);

            this.manageGamesButton.Location = new Point(794, 12);
            this.manageGamesButton.Size = new Size(150, 23);
            this.manageGamesButton.Text = "Manage Games";
            this.manageGamesButton.UseVisualStyleBackColor = true;
            this.manageGamesButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.manageGamesButton.Click += new EventHandler(this.manageGamesButton_Click);

            this.launchGameButton.Location = new Point(530, 50);
            this.launchGameButton.Size = new Size(120, 23);
            this.launchGameButton.Text = "Launch Game";
            this.launchGameButton.UseVisualStyleBackColor = true;
            this.launchGameButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this.launchGameButton.Click += new EventHandler(this.launchGameButton_Click);

            this.deleteButton.Location = new Point(660, 50);
            this.deleteButton.Size = new Size(170, 23);
            this.deleteButton.Text = "Delete Selected Save File";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.deleteButton.Click += new EventHandler(this.deleteButton_Click);

            this.hotkeyLabel.Location = new Point(16, 54);
            this.hotkeyLabel.Size = new Size(100, 20);
            this.hotkeyLabel.Text = "Delete Hotkey:";
            this.hotkeyLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            this.hotkeyTextBox.Location = new Point(120, 50);
            this.hotkeyTextBox.Size = new Size(240, 23);
            this.hotkeyTextBox.ReadOnly = true;
            this.hotkeyTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.hotkeyTextBox.KeyDown += new KeyEventHandler(this.hotkeyTextBox_KeyDown);

            this.recordHotkeyButton.Location = new Point(370, 50);
            this.recordHotkeyButton.Size = new Size(120, 23);
            this.recordHotkeyButton.Text = "Record Hotkey";
            this.recordHotkeyButton.UseVisualStyleBackColor = true;
            this.recordHotkeyButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this.recordHotkeyButton.Click += new EventHandler(this.recordHotkeyButton_Click);

            this.checkedListBox1.Location = new Point(16, 96);
            this.checkedListBox1.Size = new Size(610, 472);
            this.checkedListBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.checkedListBox1.IntegralHeight = false;

            this.manageGroupBox.Location = new Point(640, 96);
            this.manageGroupBox.Size = new Size(304, 472);
            this.manageGroupBox.Text = "Manage Games";
            this.manageGroupBox.Visible = false;
            this.manageGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;

            this.manageGameNameLabel.Location = new Point(10, 20);
            this.manageGameNameLabel.Size = new Size(100, 20);
            this.manageGameNameLabel.Text = "Game Name:";

            this.manageGameNameTextBox.Location = new Point(10, 45);
            this.manageGameNameTextBox.Size = new Size(280, 23);
            this.manageGameNameTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;


            this.createGameButton.Location = new Point(10, 75);
            this.createGameButton.Size = new Size(130, 23);
            this.createGameButton.Text = "Create / Select";
            this.createGameButton.UseVisualStyleBackColor = true;
            this.createGameButton.Click += new EventHandler(this.createGameButton_Click);

            this.removeGameButton.Location = new Point(150, 75);
            this.removeGameButton.Size = new Size(130, 23);
            this.removeGameButton.Text = "Remove Game";
            this.removeGameButton.UseVisualStyleBackColor = true;
            this.removeGameButton.Click += new EventHandler(this.removeGameButton_Click);

            this.setGameExeButton.Location = new Point(10, 110);
            this.setGameExeButton.Size = new Size(130, 23);
            this.setGameExeButton.Text = "Set EXE";
            this.setGameExeButton.UseVisualStyleBackColor = true;
            this.setGameExeButton.Click += new EventHandler(this.setGameExeButton_Click);

            this.addFileButton.Location = new Point(150, 110);
            this.addFileButton.Size = new Size(130, 23);
            this.addFileButton.Text = "Select Save File";
            this.addFileButton.UseVisualStyleBackColor = true;
            this.addFileButton.Click += new EventHandler(this.addFileButton_Click);

            this.steamLabel.Location = new Point(10, 150);
            this.steamLabel.Size = new Size(100, 20);
            this.steamLabel.Text = "Steam AppID:";

            this.steamAppIdTextBox.Location = new Point(10, 175);
            this.steamAppIdTextBox.Size = new Size(280, 23);
            this.steamAppIdTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            this.launchWithSteamCheckBox.Location = new Point(10, 205);
            this.launchWithSteamCheckBox.Size = new Size(120, 23);
            this.launchWithSteamCheckBox.Text = "Launch via Steam";
            this.launchWithSteamCheckBox.UseVisualStyleBackColor = true;

            this.saveSteamButton.Location = new Point(10, 240);
            this.saveSteamButton.Size = new Size(130, 23);
            this.saveSteamButton.Text = "Save Steam App ID";
            this.saveSteamButton.UseVisualStyleBackColor = true;
            this.saveSteamButton.Click += new EventHandler(this.saveSteamButton_Click);

            this.manageGroupBox.Controls.Add(this.manageGameNameLabel);
            this.manageGroupBox.Controls.Add(this.manageGameNameTextBox);
            this.manageGroupBox.Controls.Add(this.createGameButton);
            this.manageGroupBox.Controls.Add(this.removeGameButton);
            this.manageGroupBox.Controls.Add(this.setGameExeButton);
            this.manageGroupBox.Controls.Add(this.addFileButton);
            this.manageGroupBox.Controls.Add(this.steamLabel);
            this.manageGroupBox.Controls.Add(this.steamAppIdTextBox);
            this.manageGroupBox.Controls.Add(this.launchWithSteamCheckBox);
            this.manageGroupBox.Controls.Add(this.saveSteamButton);

            this.AutoScroll = true;
            this.Controls.Add(this.gameLabel);
            this.Controls.Add(this.gameComboBox);
            this.Controls.Add(this.manageGamesButton);
            this.Controls.Add(this.launchGameButton);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.hotkeyLabel);
            this.Controls.Add(this.hotkeyTextBox);
            this.Controls.Add(this.recordHotkeyButton);
            this.Controls.Add(this.checkedListBox1);
            this.Controls.Add(this.manageGroupBox);
            this.Text = "Speedrun Reset Tool";
            this.Load += new EventHandler(this.Form1_Load);
            this.Visible = true;

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
