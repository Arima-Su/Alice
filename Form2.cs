using Alice_v._3._2.Properties;
using System.Diagnostics;

namespace Alice_v._3._1
{
    public partial class Form2 : Form
    {
        private string ?selectedVersion;

        public Form2(string select)
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
                    textBox1.Text = Form1.GetServerIp(lines);
                    textBox2.Text = GetMaxPlayers(lines);
                    textBox3.Text = GetMotd(lines);

                }
                else
                {
                    MessageBox.Show($"Server Properties file not found in {filePath}");
                }
            }
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
    }
}
