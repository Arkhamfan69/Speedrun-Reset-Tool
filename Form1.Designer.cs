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
        private Button addFileButton;
        private Button deleteButton;
        private Button setGameExeButton;
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
            this.addFileButton = new Button();
            this.deleteButton = new Button();
            this.setGameExeButton = new Button();
            this.checkedListBox1 = new CheckedListBox();

            this.SuspendLayout();

            // gameLabel
            this.gameLabel.Location = new Point(12, 12);
            this.gameLabel.Size = new Size(100, 20);
            this.gameLabel.Text = "Select Game:";

            // gameComboBox
            this.gameComboBox.Location = new Point(120, 12);
            this.gameComboBox.Size = new Size(150, 23);
            this.gameComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            this.gameComboBox.SelectedIndexChanged += new EventHandler(this.gameComboBox_SelectedIndexChanged);
            this.gameComboBox.KeyDown += new KeyEventHandler(this.gameComboBox_KeyDown);

            // setGameExeButton
            this.setGameExeButton.Location = new Point(280, 12);
            this.setGameExeButton.Size = new Size(80, 23);
            this.setGameExeButton.Text = "Set EXE";
            this.setGameExeButton.UseVisualStyleBackColor = true;
            this.setGameExeButton.Click += new EventHandler(this.setGameExeButton_Click);

            // addFileButton
            this.addFileButton.Location = new Point(370, 12);
            this.addFileButton.Size = new Size(120, 23);
            this.addFileButton.Text = "Add Save File";
            this.addFileButton.UseVisualStyleBackColor = true;
            this.addFileButton.Click += new EventHandler(this.addFileButton_Click);

            // deleteButton
            this.deleteButton.Location = new Point(500, 12);
            this.deleteButton.Size = new Size(120, 23);
            this.deleteButton.Text = "Delete Selected";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new EventHandler(this.deleteButton_Click);

            // checkedListBox1
            this.checkedListBox1.Location = new Point(12, 50);
            this.checkedListBox1.Size = new Size(760, 380);

            // Form1
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 450);
            this.Controls.Add(this.gameLabel);
            this.Controls.Add(this.gameComboBox);
            this.Controls.Add(this.setGameExeButton);
            this.Controls.Add(this.addFileButton);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.checkedListBox1);
            this.Text = "Speedrun Reset Tool";
            this.Visible = true;

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}