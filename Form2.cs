using System.Diagnostics;


namespace Alice_v._3._1
{
    public partial class Form2 : Form
    {
        private string selectedVersion;

        public Form2()
        {
            InitializeComponent();
            selectedVersion = Form1.comboBox1.SelectedItem?.ToString();
        }

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

            // Read all lines from the file
            string[] lines = File.ReadAllLines(filePath);

            // Update specific lines
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

            // Write the modified lines back to the file
            File.WriteAllLines(filePath, lines);

            MessageBox.Show("Settings updated successfully..");

            // Close Form3
            this.Close();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true; // Cancel the key press event
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Cancel the key press event
            }
        }
    }
}
