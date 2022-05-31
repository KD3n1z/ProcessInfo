﻿using Microsoft.Win32;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace ProcessInfo
{
    public partial class Pref : Form
    {
        RegistryKey alReg;

        string themes = Path.Combine(Program.generalPath, "themes");
        public Pref()
        {
            InitializeComponent();
        }

        string themeFile = Path.Combine(Program.settings, "theme.txt");
        private void Pref_Load(object sender, EventArgs e)
        {
            try
            {
                alReg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", true);
            }
            catch { }

            if(alReg != null)
            {
                checkBox1.Checked = alReg.GetValue("ProcessInfo") != null;
            }
            else
            {
                checkBox1.Text = "start up as administrator";
            }

            if (!Directory.Exists(themes))
            {
                Directory.CreateDirectory(themes);
                try
                {
                    string dPath = Path.Combine(themes, "themes.zip");

                    new WebClient().DownloadFile("https://github.com/KD3n1z/kd3n1z-com/raw/main/themes.zip", dPath);

                    ZipFile.ExtractToDirectory(dPath, themes);

                    File.Delete(dPath);
                }
                catch { }
                File.WriteAllText(themeFile, "venus");
            }

            foreach(string f in Directory.GetFiles(themes).OrderByDescending(t => Path.GetFileNameWithoutExtension(t).StartsWith("def") || Path.GetFileNameWithoutExtension(t).StartsWith("light")))
            {
                if (f.EndsWith(".pit"))
                {
                    comboBox1.Items.Add(Path.GetFileNameWithoutExtension(f));
                }
            }
            comboBox1.Text = File.ReadAllText(themeFile);

            textCB.BackColor = Program.foreColor;
            backCB.BackColor = Program.backColor;
            dbackCB.BackColor = Program.darkBackColor;
            selCB.BackColor = Program.selColor;
            label1.Text += Program.build;

            switch (Program.ub)
            {
                case UpdateBehaviour.Always:
                    radioButton1.Checked = true;
                    break;
                case UpdateBehaviour.Never:
                    radioButton2.Checked = true;
                    break;
                case UpdateBehaviour.Ask:
                    radioButton3.Checked = true;
                    break;
            }

            button5.Text = Program.UpdateKey.ToString();
            button6.Text = Program.KillKey.ToString();
            button8.Text = Program.ShowKey.ToString();

            trackBar1.Value = Program.radius;
            trackBar1_Scroll(this, null);

            MarkUpdateBtn();

            loaded = true;
        }

        void MarkUpdateBtn()
        {
            if (Program.latest > Program.build)
            {
                button4.Text = "Update (github=b" + Program.latest + "; local=b" + Program.build + ")";
            }
        }

        private void changeFG(object sender, EventArgs e)
        {
            Control c = sender as Control;

            c.ForeColor = c.BackColor.R + c.BackColor.G + c.BackColor.B > 382 ? Color.Black : Color.White;
        }

        private void changeColor(object sender, EventArgs e)
        {
            Button b = (Button)sender;

            colorDialog1.Color = b.BackColor;

            if(colorDialog1.ShowDialog() == DialogResult.OK)
            {
                b.BackColor = colorDialog1.Color;
            }

            switch (b.Tag.ToString())
            {
                case "text":
                    Program.foreColor = b.BackColor;
                    break;
                case "bg":
                    Program.backColor = b.BackColor;
                    break;
                case "dbg":
                    Program.darkBackColor = b.BackColor;
                    break;
                case "sel":
                    Program.selColor = b.BackColor;
                    break;
                default:
                    break;
            }

            Program.mainForm.LoadTheme();
        }

        private void Pref_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (radioButton1.Checked)
            {
                Program.ub = UpdateBehaviour.Always;
            }
            else if (radioButton2.Checked)
            {
                Program.ub = UpdateBehaviour.Never;
            }
            else
            {
                Program.ub = UpdateBehaviour.Ask;
            }

            File.WriteAllText(themeFile, comboBox1.Text);

            Program.Save();

            try
            {
                if (checkBox1.Checked)
                {
                    alReg.SetValue("ProcessInfo", "\"" + Process.GetCurrentProcess().MainModule.FileName + "\" -hidden");
                }
                else if (alReg.GetValue("ProcessInfo") != null)
                {
                    alReg.DeleteValue("ProcessInfo");
                }
            }
            catch { }

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://kd3n1z.com/index.php?app=ProcessInfo");
        }

        private void button5_KeyDown(object sender, KeyEventArgs e)
        {
            Program.UpdateKey = e.KeyCode;
            button5.Text = Program.UpdateKey.ToString();
        }

        private void button6_KeyDown(object sender, KeyEventArgs e)
        {
            Program.KillKey = e.KeyCode;
            button6.Text = Program.KillKey.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Program.latest = Program.GetLatestVersion();

            if (Program.latest <= Program.build)
            {
                button4.Text = "Updates not found (github=b" + Program.latest + "; local=b" + Program.build + ")";
            }
            else if (Program.latest > Program.build)
            {
                MarkUpdateBtn();
                Program.AskForUpdate();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", "\"" + Program.path + "\"");
        }

        private void saveThemeButton_Click(object sender, EventArgs e)
        {
            if(File.Exists(Path.Combine(themes, comboBox1.Text + ".pit")))
            {
                if (File.ReadAllText(Path.Combine(themes, comboBox1.Text + ".pit")).StartsWith("onlyread"))
                {
                    MessageBox.Show("you can't edit this theme (" + comboBox1.Text + "), enter another name to save it");
                    return;
                }
                else
                {
                    SaveTheme();

                    if (!comboBox1.Items.Contains(comboBox1.Text))
                    {
                        comboBox1.Items.Add(comboBox1.Text);
                    }
                }
            }
            else
            {
                SaveTheme();

                if (!comboBox1.Items.Contains(comboBox1.Text))
                {
                    comboBox1.Items.Add(comboBox1.Text);
                }
            }
        }

        void SaveTheme()
        {
            File.WriteAllText(Path.Combine(themes, comboBox1.Text + ".pit"),
                "custom\n"
                + textCB.BackColor.ToArgb().ToString() + "\n"
                + backCB.BackColor.ToArgb().ToString() + "\n"
                + dbackCB.BackColor.ToArgb().ToString() + "\n"
                + selCB.BackColor.ToArgb().ToString() + "\n"
                );
        }


        bool loaded = false;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loaded)
            {
                LoadPIT(comboBox1.Text);
            }
        }

        void LoadPIT(string pitName)
        {
            string[] vals = File.ReadAllText(Path.Combine(themes, pitName + ".pit")).Split('\n');
            textCB.BackColor = Program.foreColor = Color.FromArgb(int.Parse(vals[1]));
            backCB.BackColor = Program.backColor =  Color.FromArgb(int.Parse(vals[2]));
            dbackCB.BackColor = Program.darkBackColor = Color.FromArgb(int.Parse(vals[3]));
            try
            {
                selCB.BackColor = Program.selColor = Color.FromArgb(int.Parse(vals[4]));
            }
            catch { }


            Program.mainForm.LoadTheme();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button8_KeyDown(object sender, KeyEventArgs e)
        {
            Program.ShowKey = e.KeyCode;
            button8.Text = Program.ShowKey.ToString();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label6.Text = "radius: " + trackBar1.Value + "px";

            Program.radius = trackBar1.Value;

            Program.mainForm.LoadTheme();
        }
    }
}
