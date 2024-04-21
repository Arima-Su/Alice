using RconSharp;
using Alice_v._3._1;

namespace Alice_v._3._2
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }

        #region Controls
        private async void button1_Click(object sender, EventArgs e)
        {
            string message = textBox1.Text;

            string executablePath = AppDomain.CurrentDomain.BaseDirectory;

            if (message == null)
            {
                MessageBox.Show("Please input a command first");
                return;
            }

            if (Form1.Launcher == null)
            {
                MessageBox.Show("Java has not started yet..");
                return;
            }
            else
            {
                StreamWriter writer = Form1.Launcher.StandardInput;
                if (writer != null)
                {
                    writer.WriteLine(message);
                    textBox1.Clear();
                }
                else
                {
                    MessageBox.Show("Standard input redirection is not enabled.");
                }
            }

            return;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                button1_Click(sender, e);
            }
        }
        #endregion
    }
}
