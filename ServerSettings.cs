using Alice_v._3._2.Properties;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Alice_v._3._1
{
    public partial class ServerSettings : Form
    {
        private string? selectedVersion;
        private Dictionary<string, string> UID = new Dictionary<string, string>();

        public ServerSettings(string select)
        {
            InitializeComponent();
            selectedVersion = select;
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;

            if (selectedVersion != null)
            {
                string filePath = Path.Combine(executablePath, "versions", selectedVersion, "server.properties");
                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);
                    textBox1.Text = LauncherMain.GetServerIp(lines);
                    textBox2.Text = GetMaxPlayers(lines);
                    textBox3.Text = GetMotd(lines);

                }
                else
                {
                    MessageBox.Show($"Server Properties file not found in {filePath}");
                }

                if (File.Exists(Path.Combine(executablePath, "versions", selectedVersion, "UID.json")))
                {
                    string UIDs = File.ReadAllText(Path.Combine(executablePath, "versions", selectedVersion, "UID.json"));
                    UID = JsonConvert.DeserializeObject<Dictionary<string, string>>(UIDs);

                    if (UID.Count > 0)
                    {
                        foreach (var item in UID)
                        {
                            listBox1.Items.Add(item.Value);
                        }
                    }
                }
            }

            listBox1.Visible = false;
            listBox2.Visible = false;
            label4.Visible = false;
            button3.Visible = false;
            label5.Visible = false;
            label6.Visible = false;
        }

        #region Functions
        public static string GetMaxPlayers(string[] lines)
        {
            foreach (string line in lines)
            {
                if (line.StartsWith("max-players="))
                {
                    return line.Substring("max-players=".Length).Trim();
                }
            }

            return null;
        }

        public static string GetMotd(string[] lines)
        {
            foreach (string line in lines)
            {
                if (line.StartsWith("motd="))
                {
                    return line.Substring("motd=".Length).Trim();
                }
            }

            return null;
        }
        #endregion

        #region Events
        private void button1_Click(object sender, EventArgs e)     //OPEN SERVER CONFIG BUTTON
        {
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(executablePath, "versions", selectedVersion, "server.properties");

            if (File.Exists(filePath))
            {
                Process.Start("notepad.exe", filePath);
            }
            else
            {
                MessageBox.Show($"Server Properties file not found in {filePath}");
            }
        }

        private void button2_Click(object sender, EventArgs e)   //APPLY SETTINGS BUTTON
        {
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(executablePath, "versions", selectedVersion, "server.properties");

            string[] lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("server-ip="))
                {
                    lines[i] = "server-ip=" + textBox1.Text;
                }
                else if (lines[i].StartsWith("max-players="))
                {
                    lines[i] = "max-players=" + textBox2.Text;
                }
                else if (lines[i].StartsWith("motd="))
                {
                    lines[i] = "motd=" + textBox3.Text;
                }
            }

            File.WriteAllLines(filePath, lines);

            MessageBox.Show("Settings updated successfully..");

            Settings.Default.IP = textBox1.Text;
            Settings.Default.Players = textBox2.Text;
            Settings.Default.Motd = textBox3.Text;

            string json = JsonConvert.SerializeObject(UID, Formatting.Indented);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions", selectedVersion, "UID.json"), json);

            this.Close();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        #endregion

        private void label7_DoubleClick(object sender, EventArgs e)
        {
            listBox1.Visible = true;
            listBox2.Visible = true;
            label4.Visible = true;
            button3.Visible = true;
            label5.Visible = true;
            label6.Visible = true;

            this.Size = new Size(441, 378);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string IP = listBox1.SelectedItem.ToString();
            List<string> user = new List<string>();
            listBox2.Items.Clear();

            foreach (var item in UID)
            {
                if (item.Value == IP)
                {
                    user.Add(item.Key);
                }
            }

            foreach (var item in user)
            {
                listBox2.Items.Add(item);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex != null)
            {
                UID.Remove(listBox2.SelectedItem.ToString());

                listBox1.Items.Clear();
                listBox2.Items.Clear();

                if (UID.Count > 0)
                {
                    foreach (var item in UID)
                    {
                        listBox1.Items.Add(item.Value);
                    }
                }
            }
        }
    }
}
