using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using Alice_v._3._2;
using RconSharp;

namespace Alice_v._3._1
{
    public partial class Form1 : Form
    {
        //VARIABLES
        private string selectedLaunchType;
        public static string? selectedVersion;
        private Process Launcher;
        private bool Started = false;
        private string Alice = "";

        //MAIN MENU
        public Form1()
        {
            InitializeComponent();
            //INITIALIZE KEY
            if (File.Exists("webhook.txt"))
            {
                string[] key = File.ReadAllLines("webhook.txt");
                if (key.First() != string.Empty)
                {
                    Alice = key.First();
                }
                else
                {
                    MessageBox.Show("Webhook not initialized, in-game discord music control unavailable.");
                }
                
            }
            else
            {
                MessageBox.Show("Webhook not initialized, in-game discord music control unavailable.");
            }

            //VERSIONS COMBO BOX
            try
            {
                string executablePath = AppDomain.CurrentDomain.BaseDirectory;
                string folderPath = Path.Combine(executablePath, "versions");
                string[] folderNames = Directory.GetDirectories(folderPath);

                if (folderNames.Length == 0)
                {
                    MessageBox.Show("Missing necessary files, contact _Arimasu for details..\nERROR CODE: 002");
                    this.Close();
                }

                foreach (string folderName in folderNames)
                {
                    comboBox1.Items.Add(Path.GetFileName(folderName));
                }

                //LAUNCH TYPE COMBO BOX
                comboBox2.Items.Add("Push Start");
                comboBox2.Items.Add("Silent Start");

                //PRESS START
                label3.Text = "Start";
            }
            catch
            {
                MessageBox.Show("Missing necessary files, contact _Arimasu for details..");
                this.Close();
            }
        }

        //CONSOLE WRITER
        private async void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                consoleOutput.Invoke(new Action(() => consoleOutput.AppendText(e.Data + Environment.NewLine)));

                //DETECTORS
                if (e.Data.Contains("For help, type"))
                {
                    if (selectedLaunchType == "Push Start")
                    {
                        await AliceSend("The server is up yo..");

                        string executablePath = AppDomain.CurrentDomain.BaseDirectory;

                        if (selectedVersion != null)
                        {
                            string filePath = Path.Combine(executablePath, "versions", selectedVersion, "server.properties");
                            if (File.Exists(filePath))
                            {
                                string[] lines = File.ReadAllLines(filePath);
                                string serverctxIp = GetServerIp(lines);

                                if (!string.IsNullOrEmpty(serverctxIp))
                                {
                                    await AliceSend("IP: " + serverctxIp);
                                }
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                if (e.Data.Contains("[Rcon: Stopping the server]"))
                {
                    label3.Text = "Start";
                    consoleOutput.AppendText("SERVER STOPPED");
                    Started = false;

                    if (selectedLaunchType == "Push Start")
                    {
                        await AliceSend("The server is down yo..");
                    }
                    else
                    {
                        return;
                    }
                }
                if (e.Data.Contains("FAILED TO BIND TO PORT"))
                {
                    label3.Text = "Start";
                    consoleOutput.AppendText("SERVER STOPPED");
                    MessageBox.Show("Error Binding to port, make sure Hamachi is on.");
                    Started = false;

                    if (selectedLaunchType == "Push Start")
                    {
                        await AliceSend("The server is down yo..");
                    }
                    else
                    {
                        return;
                    }
                }

                // COMMANDS
                if (e.Data.Contains("alice!play"))                                                    
                {
                    int startIndex = e.Data.IndexOf("alice!play") + "alice!play".Length;
                    string remainingString = e.Data.Substring(startIndex).Trim();

                    await AliceSend($"alice!play {remainingString}");
                    await RconExecute($"/tellraw @a {{\"text\":\"Alice: I told bocchi to play {remainingString}..\",\"color\":\"dark_purple\",\"bold\":false}}");
                }
                if (e.Data.Contains("alice!np"))
                {
                    await AliceSend("alice!np");
                    await RconExecute("/tellraw @a {\"text\":\"Alice: I asked bocchi for the current song..\",\"color\":\"dark_purple\",\"bold\":false}}");
                }
                if (e.Data.Contains("alice!skip"))
                {
                    await AliceSend("alice!skip");
                    await RconExecute("/tellraw @a {\"text\":\"Alice: I told bocchi to skip..\",\"color\":\"dark_purple\",\"bold\":false}}");
                }
                if (e.Data.Contains("alice!q"))
                {
                    await RconExecute("/tellraw @a {\"text\":\"Alice: I asked bocchi for the queue..\",\"color\":\"dark_purple\",\"bold\":false}}");
                    await AliceSend("alice!q");
                }
                if (e.Data.Contains("alice!ps"))
                {
                    int startIndex = e.Data.IndexOf("alice!ps") + "alice!ps".Length;
                    string remainingString = e.Data.Substring(startIndex).Trim();

                    await AliceSend($"alice!ps {remainingString}");
                    await RconExecute($"/tellraw @a {{\"text\":\"Alice: I told bocchi to playskip {remainingString}..\",\"color\":\"dark_purple\",\"bold\":false}}");
                }
                if (e.Data.Contains("alice!load"))
                {
                    int startIndex = e.Data.IndexOf("alice!load") + "alice!load".Length;
                    string remainingString = e.Data.Substring(startIndex).Trim();

                    await AliceSend($"alice!load {remainingString}");
                    await RconExecute($"/tellraw @a {{\"text\":\"Alice: I told bocchi to load {remainingString}..\",\"color\":\"dark_purple\",\"bold\":false}}");
                }

                // BRUH MOMENTS
                if (e.Data.Contains("alice1"))
                {
                    await RconExecute("/tellraw @a {\"text\":\"Alice: Buddy..\",\"color\":\"dark_purple\",\"bold\":false}}");
                    await RconExecute("/tellraw @a {\"text\":\"Bocchi: Baka~\",\"color\":\"light_purple\",\"bold\":false}}");
                }
                if (e.Data.Contains("bocchi!"))
                {
                    await RconExecute("/tellraw @a {\"text\":\"Alice: Uhhh..\",\"color\":\"dark_purple\",\"bold\":false}}");
                    await RconExecute("/tellraw @a {\"text\":\"Bocchi: Buddy's confused..\",\"color\":\"light_purple\",\"bold\":false}}");
                }

                // INGAME RESPONDS
                if (e.Data.Contains("Brother", StringComparison.OrdinalIgnoreCase))
                {
                    await RconExecute($"/tellraw @a {{\"text\":\"Bocchi: Sister even..\",\"color\":\"light_purple\",\"bold\":false}}");
                }
                if (e.Data.Contains("Buddy", StringComparison.OrdinalIgnoreCase))
                {
                    await RconExecute($"/tellraw @a {{\"text\":\"Bocchi: Baka~\",\"color\":\"light_purple\",\"bold\":false}}");
                }
                if (e.Data.Contains("Hello Bocchi", StringComparison.OrdinalIgnoreCase))
                {
                    await RconExecute($"/tellraw @a {{\"text\":\"Bocchi: Hi :D\",\"color\":\"light_purple\",\"bold\":false}}");
                }
                if (e.Data.Contains("Bad Bocchi", StringComparison.OrdinalIgnoreCase))
                {
                    await RconExecute($"/tellraw @a {{\"text\":\"Bocchi: :(\",\"color\":\"light_purple\",\"bold\":false}}");
                }
                if (e.Data.Contains("Sup Alice", StringComparison.OrdinalIgnoreCase))
                {
                    await RconExecute($"/tellraw @a {{\"text\":\"Alice: Sup.\",\"color\":\"dark_purple\",\"bold\":false}}");
                }
            }
        }

        //CONTROLS
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)  //VERSION COMBO BOX
        {
            selectedVersion = comboBox1.SelectedItem?.ToString();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) //LAUNCH TYPE COMBO BOX
        {
            selectedLaunchType = comboBox2.SelectedItem.ToString();
        }

        private async void label3_Click(object sender, EventArgs e)       //START LABEL
        {
            if (selectedLaunchType == null || selectedVersion == null)
            {
                MessageBox.Show("Please specify a version and launch type..");
                return;
            }

            if (Started == false)
            {
                Started = true;
                label3.Text = "Stop";
                consoleOutput.Clear();
                if (selectedLaunchType == "Push Start")                  //PUSH START
                {
                    await AliceSend("The server is starting yo..");

                    string executablePath = AppDomain.CurrentDomain.BaseDirectory;
                    string server = Path.Combine(executablePath, "versions", selectedVersion);

                    string java = Path.Combine(executablePath, "Java", "bin", "java.exe");

                    Launcher = new Process();
                    Launcher.StartInfo.FileName = $"{java}";
                    Launcher.StartInfo.Arguments = $"-Xmx4G -Xms2G -XX:+UseG1GC -jar server.jar --nogui --world sekai";
                    Launcher.StartInfo.WorkingDirectory = server;
                    Launcher.StartInfo.RedirectStandardOutput = true;
                    Launcher.StartInfo.UseShellExecute = false;
                    Launcher.StartInfo.CreateNoWindow = true;
                    Launcher.OutputDataReceived += Process_OutputDataReceived;

                    Launcher.Start();
                    Launcher.BeginOutputReadLine();
                }
                else                                                     //SILENT START
                {
                    string executablePath = AppDomain.CurrentDomain.BaseDirectory;
                    string server = Path.Combine(executablePath, "versions", selectedVersion);
                    string java = Path.Combine(executablePath, "Java", "bin", "java.exe");

                    Launcher = new Process();
                    Launcher.StartInfo.FileName = $"{java}";
                    Launcher.StartInfo.Arguments = $"-Xmx4G -Xms2G -XX:+UseG1GC -jar server.jar --nogui --world sekai";
                    Launcher.StartInfo.WorkingDirectory = server;
                    Launcher.StartInfo.RedirectStandardOutput = true;
                    Launcher.StartInfo.UseShellExecute = false;
                    Launcher.StartInfo.CreateNoWindow = true;
                    Launcher.OutputDataReceived += Process_OutputDataReceived;

                    Launcher.Start();
                    Launcher.BeginOutputReadLine();
                }
            }
            else
            {
                try
                {
                    await RconExecute("/stop");
                    Started = false;
                }
                catch
                {
                    MessageBox.Show("Not again..");
                }
            }

        }

        private async void pictureBox7_Click(object sender, EventArgs e)   //START BUTTON
        {
            if (selectedLaunchType == null || selectedVersion == null)
            {
                MessageBox.Show("Please specify a version and launch type..");
                return;
            }

            if (Started == false)
            {
                Started = true;
                label3.Text = "Stop";
                consoleOutput.Clear();
                if (selectedLaunchType == "Push Start")                  //PUSH START
                {
                    await AliceSend("The server is starting yo..");
                    
                    string executablePath = AppDomain.CurrentDomain.BaseDirectory;
                    string server = Path.Combine(executablePath, "versions", selectedVersion);
                    string java = Path.Combine(executablePath, "Java", "bin", "java.exe");

                    Launcher = new Process();
                    Launcher.StartInfo.FileName = $"{java}";
                    Launcher.StartInfo.Arguments = $"-Xmx4G -Xms2G -XX:+UseG1GC -jar server.jar --nogui --world sekai";
                    Launcher.StartInfo.WorkingDirectory = server;
                    Launcher.StartInfo.RedirectStandardOutput = true;
                    Launcher.StartInfo.UseShellExecute = false;
                    Launcher.StartInfo.CreateNoWindow = true;
                    Launcher.OutputDataReceived += Process_OutputDataReceived;

                    Launcher.Start();
                    Launcher.BeginOutputReadLine();
                }
                else                                                     //SILENT START
                {
                    string executablePath = AppDomain.CurrentDomain.BaseDirectory;
                    string server = Path.Combine(executablePath, "versions", selectedVersion);
                    string java = Path.Combine(executablePath, "Java", "bin", "java.exe");

                    Launcher = new Process();
                    Launcher.StartInfo.FileName = $"{java}";
                    Launcher.StartInfo.Arguments = $"-Xmx4G -Xms2G -XX:+UseG1GC -jar server.jar --nogui --world sekai";
                    Launcher.StartInfo.WorkingDirectory = server;
                    Launcher.StartInfo.RedirectStandardOutput = true;
                    Launcher.StartInfo.UseShellExecute = false;
                    Launcher.StartInfo.CreateNoWindow = true;
                    Launcher.OutputDataReceived += Process_OutputDataReceived;

                    Launcher.Start();
                    Launcher.BeginOutputReadLine();
                }
            }
            else
            {
                try
                {
                    await RconExecute("/stop");
                    Started = false;
                }
                catch
                {
                    MessageBox.Show("Not again..");
                }
            }

        }

        private void pictureBox5_Click(object sender, EventArgs e)    //SETTINGS BUTTON
        {
            string selectedFolder = comboBox1.SelectedItem?.ToString();
            if (selectedFolder != null)
            {
                Form2 secondForm = new Form2();
                secondForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please pick a version first..");
            }

        }

        private void pictureBox6_Click(object sender, EventArgs e)    //VERSIONS FOLDER BUTTON
        {
            if (selectedVersion != null)
            {
                string executablePath = AppDomain.CurrentDomain.BaseDirectory;
                string folderPath = Path.Combine(executablePath, "versions", selectedVersion);

                Process.Start("explorer.exe", folderPath);
            }
            else
            {
                MessageBox.Show("Pick a version first..");
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)   //ADD BUTTON
        {
            Form3 thirdForm = new Form3();

            thirdForm.ShowDialog();
        }

        private void pictureBox3_Click(object sender, EventArgs e)   //MODS FOLDER BUTTON
        {
            if (selectedVersion != null)
            {
                string executablePath = AppDomain.CurrentDomain.BaseDirectory;
                string folderPath = Path.Combine(executablePath, "versions", selectedVersion, "mods");

                Process.Start("explorer.exe", folderPath);
            }
            else
            {
                MessageBox.Show("Pick a version first..");
            }
        }

        private void pictureBox8_Click(object sender, EventArgs e)  //REFRESH BUTTON
        {
            RefreshComboBox1();
        }

        private void RefreshComboBox1()    //REFRESH METHOD
        {
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = Path.Combine(executablePath, "versions");
            string[] folderNames = Directory.GetDirectories(folderPath);

            comboBox1.Items.Clear();

            foreach (string folderName in folderNames)
            {
                comboBox1.Items.Add(Path.GetFileName(folderName));
            }
        }

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)  //CONSOLE CLOSER
        {
            await RconExecute("/stop");

        }

        private async Task AliceSend(string message)
        {
            string webhookUrl = Alice;
            string content3 = $"{message}";
            string payload3 = $"{{\"content\": \"{content3}\"}}";
            using (HttpClient httpClient = new HttpClient())
            {
                HttpContent httpContent3 = new StringContent(payload3, Encoding.UTF8, "application/json");
                httpClient.PostAsync(webhookUrl, httpContent3).Wait();
            }
        }

        private async Task RconExecute(string message)
        {
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;

            if (selectedVersion != null)
            {
                string filePath = Path.Combine(executablePath, "versions", selectedVersion, "server.properties");
                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);
                    string serverctxIp = GetServerIp(lines);

                    if (!string.IsNullOrEmpty(serverctxIp))
                    {
                        string serverIp = $"{serverctxIp}";
                        int serverPort = 25575;

                        var client = RconClient.Create($"{serverIp}", serverPort);

                        // Open the connection
                        await client.ConnectAsync();

                        // Send a RCON packet with type AUTH and the RCON password for the target server
                        var authenticated = await client.AuthenticateAsync("727");
                        if (authenticated)
                        {
                            await client.ExecuteCommandAsync($"{message}");
                        }
                    }
                    else
                    {
                        // The server IP value was not found in the file
                        MessageBox.Show("Server IP value not found in server.properties");
                    }
                }
                else
                {
                    MessageBox.Show($"Server Properties file not found in {filePath}");
                }
            }
        }

        public static string GetServerIp(string[] lines)
        {
            foreach (string line in lines)
            {
                if (line.StartsWith("server-ip="))
                {
                    return line.Substring("server-ip=".Length).Trim();
                }
            }

            return null; // Server IP value not found
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form4 fourthForm = new Form4();

            fourthForm.ShowDialog();
        }
    }
}
