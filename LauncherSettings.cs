using Alice_v._3._2.Properties;
using Alice_v._3._1;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Alice_v._3._2
{
    public partial class LauncherSettings : Form
    {
        private static bool dev = false;
        private static List<string> mods = new List<string>();

        public LauncherSettings()
        {
            InitializeComponent();

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.json")))
            {
                string mod = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.json"));
                mods = JsonConvert.DeserializeObject<List<string>>(mod);
            }
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            textBox2.Text = Settings.Default.minRAM;
            textBox1.Text = Settings.Default.maxRAM;
            textBox4.Text = $"{Settings.Default.saveCount}";
            textBox3.Text = $"{Settings.Default.saveFreq}";
            textBox6.Text = Settings.Default.javaDirectory;
            checkBox1.Checked = Settings.Default.chatDef;
            checkBox2.Checked = Settings.Default.ipFilter;
            checkBox3.Checked = Settings.Default.clientBan;
            label8.Visible = false;
            label9.Visible = false;
            label11.Visible = false;

            checkBox2.Visible = false;
            checkBox3.Visible = false;
            button2.Visible = false;
            button4.Visible = false;
            label10.Visible = false;
            label11.Visible = false;
            textBox5.Visible = false;

            dev = false;

            if (mods.Count > 0)
            {
                foreach (var item in mods)
                {
                    textBox5.AppendText(item + Environment.NewLine);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == string.Empty || textBox2.Text == string.Empty || textBox4.Text == string.Empty || textBox3.Text == string.Empty)
            {
                MessageBox.Show("All fields must be filled.");
                return;
            }

            if (int.Parse(textBox3.Text) < 10)
            {
                MessageBox.Show("Frequency must be at least 10 minutes.\nPlease enter a value that is 10 or greater.");
                return;
            }

            Settings.Default.minRAM = textBox2.Text;
            Settings.Default.maxRAM = textBox1.Text;
            Settings.Default.saveCount = int.Parse(textBox4.Text);
            Settings.Default.saveFreq = int.Parse(textBox3.Text);

            if (dev)
            {
                string[] strings = textBox5.Text.Split('\n');
                mods.Clear();

                foreach (string s in strings)
                {
                    if (s.Length > 0)
                    {
                        mods.Add(s.Trim());
                    }
                }
            }

            string json = JsonConvert.SerializeObject(mods, Formatting.Indented);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.json"), json);
            Settings.Default.Save();

            this.Close();
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.chatDef = checkBox1.Checked;
            Settings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This setting stops players attaches IPs to usernames, essentially limiting which IP can join with that username. This can be configured per server instance.");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("List client-side mods that might be considered as 'hacks'.");
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.ipFilter = checkBox2.Checked;
            Settings.Default.Save();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox3.Checked)
            {
                label11.Visible = false;
                textBox5.Visible = false;
            }
            else
            {
                label11.Visible = true;
                textBox5.Visible = true;
            }

            Settings.Default.clientBan = checkBox3.Checked;
            Settings.Default.Save();
        }

        private void label7_DoubleClick(object sender, EventArgs e)
        {
            if (!dev)
            {
                checkBox2.Visible = true;
                checkBox3.Visible = true;
                button2.Visible = true;
                button4.Visible = true;
                label10.Visible = true;
                label8.Visible = true;
                label9.Visible = true;
                label11.Visible = true;

                //this.Size = new Size(455, 319);
                this.Size = new Size(455, 353);

                if (checkBox3.Checked)
                {
                    label11.Visible = true;
                    textBox5.Visible = true;
                }

                dev = true;
            }
        }

        private void button3_Click(object sender, EventArgs e) //BROWSE BUTTON
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select the folder containing java.exe";
                folderDialog.UseDescriptionForTitle = true;
                string? javaFile = null;

                DialogResult result = folderDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
                {
                    string folderPath = folderDialog.SelectedPath;

                    javaFile = Directory.GetFiles(folderPath, "java.exe", SearchOption.TopDirectoryOnly)
                                               .FirstOrDefault();

                    if (javaFile == null)
                    {
                        DialogResult result2 = MessageBox.Show(
                            "Folder does not contain a valid Java install.\n\nWould you like to visit the Java download page?",
                            "Information",
                            MessageBoxButtons.OKCancel);

                        if (result2 == DialogResult.OK)
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "https://www.oracle.com/java/technologies/java-se-glance.html",
                                UseShellExecute = true
                            });
                        }
                        return;
                    }
                }

                Settings.Default.javaDirectory = javaFile;
                Settings.Default.Save();
                textBox4.Text = javaFile;
            }
        }
    }
}
