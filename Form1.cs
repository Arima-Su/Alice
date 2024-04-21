using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Alice_v._3._2;
using Alice_v._3._2.Properties;
using RconSharp;
using Newtonsoft.Json;
using System;

namespace Alice_v._3._1
{
    public partial class Form1 : Form
    {
        #region Variables
        private string selectedLaunchType = Settings.Default.StartType;
        public static string? selectedVersion = Settings.Default.Version;
        public static Process? Launcher;
        private bool Started = false;
        private string Alice = "";
        private int playerCount = 0;
        private ImageList? imageList;
        private Dictionary<string, string> UID;
        #endregion

        //MAIN MENU
        public Form1()
        {
            InitializeComponent();
            label5.Visible = false;
            label6.Visible = false;
            textBox1.Visible = false;
            listBox1.Visible = false;
            RefreshComboBox1();
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            //LAUNCH TYPE COMBO BOX
            comboBox2.Items.Add("Push Start");
            comboBox2.Items.Add("Silent Start");

            comboBox2.SelectedIndex = comboBox2.Items.IndexOf(Settings.Default.StartType);

            //PRESS START
            label3.Text = "Start";

            //INITIALIZE WEBHOOK
            if (File.Exists("webhook.txt"))
            {
                string[] key = File.ReadAllLines("webhook.txt");
                if (key.Length <= 0)
                {
                    MessageBox.Show("Webhook not initialized, in-game discord music control unavailable.");
                    return;
                }

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
                    return;
                }

                foreach (string folderName in folderNames)
                {
                    comboBox1.Items.Add(Path.GetFileName(folderName));
                }

                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(Settings.Default.Version);
            }
            catch
            {
                MessageBox.Show("Missing necessary files, redownload and unzip properly.");
                this.Close();
                return;
            }
        }

        //CONSOLE WRITER
        private async void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                consoleOutput.Invoke(new Action(() => consoleOutput.AppendText(e.Data + Environment.NewLine)));

                #region Detectors
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
                if (e.Data.Contains("logged in with entity id") && e.Data.Contains("PlayerList"))
                {
                    //Match match = Regex.Match(e.Data, @"\]: (\w+)(?=\[)");
                    ////[18:00:52] [Server thread/INFO] [net.minecraft.server.management.PlayerList]: Celery25[/25.17.12.253:54440] logged in with entity id 347 at (1313.8571287736231, 96.0, 1519.1311725135174)
                    //// Check if the match was successful
                    //if (match.Success)
                    //{
                    //    // Extract the player name from the match
                    //    string playerName = match.Groups[1].Value;

                    //    // Print the result
                    //    listBox1.Items.Add(playerName);
                    //    playerCount++;
                    //    label4.Text = $"Players: {playerCount}";
                    //}
                    // Define the regex pattern to match "Celery25" and "25.17.12.253"
                    string pattern = @"\b(\w+)(?:\[\/)(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})";

                    // Create a regex object
                    Regex regex = new Regex(pattern);

                    // Match the pattern in the input string
                    Match match = regex.Match(e.Data);

                    // Check if the match is successful
                    if (match.Success)
                    {
                        // Get the username and IP address from the match groups
                        string username = match.Groups[1].Value;
                        string ipAddress = match.Groups[2].Value;

                        listBox1.Items.Add(username);
                        playerCount++;
                        label4.Text = $"Players: {playerCount}";

                        if (UID.ContainsKey(username))
                        {
                            if (UID[username] != ipAddress)
                            {
                                await RconExecute($"/kick {username} You do not have access to this account.");
                            }
                        }
                        else
                        {
                            UID.Add(ipAddress, username);
                        }
                        
                        Console.WriteLine("Username: " + username);
                        Console.WriteLine("IP Address: " + ipAddress);
                    }
                    else
                    {
                        Console.WriteLine("No match found.");
                    }
                }
                if (e.Data.Contains("lost connection"))
                {
                    Match match = Regex.Match(e.Data, @"]: (\w+)(?= lost connection)");

                    // Check if the match was successful
                    if (match.Success)
                    {
                        // Extract the player name from the match
                        string playerName = match.Groups[1].Value;

                        // Print the result
                        if (listBox1.Items.Contains(playerName))
                        {
                            listBox1.Items.Remove(playerName);
                            playerCount--;
                            label4.Text = $"Players: {playerCount}";
                        }
                    }
                }
                #endregion

                #region Commands
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
                #endregion

                #region Moments
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
                #endregion

                #region In-game responses
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
                #endregion
            }
        }

        #region Controls
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)  //VERSION COMBO BOX
        {
            selectedVersion = comboBox1.SelectedItem.ToString();
            if (selectedVersion is not "")
            {
                Settings.Default.Version = selectedVersion;
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) //LAUNCH TYPE COMBO BOX
        {
            selectedLaunchType = comboBox2.SelectedItem.ToString();
            Settings.Default.StartType = selectedLaunchType;
        }



        private async void pictureBox7_Click(object sender, EventArgs e)   //START BUTTON
        {
            if (selectedLaunchType == null || selectedVersion == null || comboBox1.SelectedIndex < 0 || comboBox2.SelectedIndex < 0)
            {
                MessageBox.Show("Please specify a version and launch type..");
                return;
            }

            if (!FilledUp())
            {
                MessageBox.Show("Please configure Server IP..");
                return;
            }

            Settings.Default.Version = comboBox1.SelectedItem.ToString();
            Settings.Default.StartType = comboBox2.SelectedItem.ToString();
            Settings.Default.Save();

            if (Started == false)
            {
                listView1.Visible = false;
                listBox1.Visible = true;
                label4.Text = "Players:";
                label5.Visible = true;
                label6.Visible = true;
                Started = true;
                label3.Text = "Stop";
                consoleOutput.Clear();

                if (selectedLaunchType == "Push Start")                  //PUSH START
                {
                    await AliceSend("The server is starting yo..");
                }

                string executablePath = AppDomain.CurrentDomain.BaseDirectory;
                string server = Path.Combine(executablePath, "versions", selectedVersion);
                string java = Path.Combine(executablePath, "Java", "bin", "java.exe");

                Launcher = new Process();
                Launcher.StartInfo.FileName = $"{java}";
                Launcher.StartInfo.Arguments = $"-Xmx4G -Xms2G -XX:+UseG1GC -jar server.jar --nogui --world sekai";
                Launcher.StartInfo.WorkingDirectory = server;
                Launcher.StartInfo.RedirectStandardOutput = true;
                Launcher.StartInfo.RedirectStandardInput = true;
                Launcher.StartInfo.UseShellExecute = false;
                Launcher.StartInfo.CreateNoWindow = true;
                Launcher.OutputDataReceived += Process_OutputDataReceived;

                Launcher.Start();
                Launcher.BeginOutputReadLine();

                //VALIDATE UIDS
                var IDs = Path.Combine(executablePath, "versions", selectedVersion, "UID.json");

                if (File.Exists(IDs))
                {
                    string UIDs = File.ReadAllText(IDs);
                    UID = JsonConvert.DeserializeObject<Dictionary<string, string>>(UIDs);
                }
                else
                {
                    UID = new Dictionary<string, string>();
                }
            }
            else
            {
                try
                {
                    await RconExecute("/stop");
                    Started = false;
                    listView1.Visible = true;
                    listBox1.Visible = false;
                    label4.Text = "Settings:";
                    label5.Visible = false;
                    label6.Visible = false;

                    string json = JsonConvert.SerializeObject(UID, Formatting.Indented);
                    File.WriteAllText("UID.json", json);
                }
                catch
                {
                    MessageBox.Show("Not again..");
                }
            }

        }

        private void pictureBox5_Click(object sender, EventArgs e)    //SETTINGS BUTTON
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Please pick a version first..");
                return;
            }
            else
            {
                string selectedFolder = comboBox1.SelectedItem.ToString();
                if (selectedFolder != null)
                {
                    Form2 secondForm = new Form2(selectedFolder);
                    secondForm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Please pick a version first..");
                }
            }
        }

        private void pictureBox6_Click(object sender, EventArgs e)    //VERSIONS FOLDER BUTTON
        {
            if (selectedVersion != null)
            {
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions", selectedVersion);

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
            if (selectedVersion != null && comboBox1.SelectedIndex >= 0)
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

        private void button1_Click(object sender, EventArgs e)    //CONSOLE BUTTON
        {
            Form4 fourthForm = new Form4();
            fourthForm.ShowDialog();
        }

        private void comboBox1_Click(object sender, EventArgs e)    //VERSIONS COMBO BOX
        {
            RefreshComboBox1();
        }

        private async void label6_Click(object sender, EventArgs e)     //KICK BUTTON
        {
            if (listBox1.SelectedItems.Count < 1)
            {
                return;
            }

            var player = listBox1.SelectedItem.ToString();

            try
            {
                await RconExecute("/kick " + player);
                label6.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There was an error kicking {player} \n\n{ex}");
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)    //PLAYER LIST
        {
            if (comboBox1.SelectedIndex < 0)
            {
                label6.Enabled = false;
            }
            label6.Enabled = true;
        }

        private async void listView1_SelectedIndexChanged(object sender, EventArgs e)    //PLAYER LIST
        {
            if (listView1.SelectedItems.Count < 1)
            {
                return;
            }

            if (!(listView1.SelectedItems[0].Index < 0))
            {
                comboBox1.SelectedIndex = listView1.SelectedItems[0].Index;
            }
            else
            {
                comboBox1.SelectedIndex = -1;
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)    //PLAYER LIST
        {
            if (Started)
            {
                listView1.Visible = false;
            }
        }

        private void consoleOutput_DoubleClick(object sender, EventArgs e)   //CONSOLE SCREEN
        {
            if (Started)
            {
                listView1.Visible = true;
            }
        }

        private void label7_Click(object sender, EventArgs e)   //ICON BUTTON
        {
            if (selectedVersion is null || comboBox1.SelectedIndex < 0)
            {
                MessageBox.Show("Please pick a version first.");
                return;
            }

            imageList.Dispose();
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Select a File";
            openFileDialog.Filter = "PNG files (*.png)|*.png|JPEG files (*.jpeg;*.jpg)|*.jpeg;*.jpg|All Files (*.*)|*.*";

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string selectedFile = Path.Combine(Path.GetDirectoryName(openFileDialog.FileName), openFileDialog.FileName);

                if (!selectedFile.Contains(".png") && !selectedFile.Contains(".jpg") && !selectedFile.Contains(".jpeg"))
                {
                    MessageBox.Show("File not supported. Must be a png, jpg, or jpeg file.");
                    return;
                }

                File.Copy(selectedFile, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions", selectedVersion, "icon.png"), true);
            }

            RefreshComboBox1();
        }

        private void label8_Click(object sender, EventArgs e)   //RENAME BUTTON
        {
            if (selectedVersion is null || comboBox1.SelectedIndex < 0)
            {
                MessageBox.Show("Please pick a version first.");
                return;
            }

            label8.Enabled = false;
            textBox1.Visible = true;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)    //RENAME BUTTON
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox1.Text.Length < 1)
                {
                    MessageBox.Show("Field can't be empty");
                    return;
                }

                if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions", textBox1.Text)))
                {
                    MessageBox.Show("That version already exists, please pick a different name..");
                    textBox1.Text = "";
                    return;
                }

                try
                {
                    // Rename the folder
                    Directory.Move(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions", selectedVersion), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions", textBox1.Text));

                    label8.Enabled = true;
                    textBox1.Text = "";
                    textBox1.Visible = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error renaming folder: {ex.Message}");
                    label8.Enabled = true;
                    textBox1.Text = "";
                    textBox1.Visible = false;
                }

                RefreshComboBox1();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                label8.Enabled = true;
                textBox1.Visible = false;
            }
        }

        private void label9_Click(object sender, EventArgs e)   //DELETE BUTTON
        {
            if (selectedVersion is null || comboBox1.SelectedIndex < 0)
            {
                MessageBox.Show("Please pick a version first.");
                return;
            }

            DialogResult result = MessageBox.Show(
            $"Do you really wish to delete {selectedVersion} version?",
            "Delete",
            MessageBoxButtons.OKCancel);

            // Check the result
            if (result == DialogResult.OK)
            {
                try
                {
                    Directory.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions", selectedVersion), true);
                    RefreshComboBox1();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error renaming folder: {ex.Message}");
                }
            }
        }
        #endregion

        #region Events
        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)  //CONSOLE CLOSER
        {
            if (Started)
            {
                await RconExecute("/stop");
            }
        }

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;
            MessageBox.Show("Unhandled exception: " + exception);
        }
        #endregion

        #region Mouse Event VFX
        private void pictureBox5_MouseEnter(object sender, EventArgs e)
        {
            pictureBox5.Image = Resources.SettingDown;
        }

        private void pictureBox5_MouseLeave(object sender, EventArgs e)
        {
            pictureBox5.Image = Resources.Setting;
        }

        private void pictureBox6_MouseEnter(object sender, EventArgs e)
        {
            pictureBox6.Image = Resources.FolderDown;
        }

        private void pictureBox6_MouseLeave(object sender, EventArgs e)
        {
            pictureBox6.Image = Resources.Folder;
        }

        private void pictureBox4_MouseEnter(object sender, EventArgs e)
        {
            pictureBox4.Image = Resources.AddDown;
        }

        private void pictureBox4_MouseLeave(object sender, EventArgs e)
        {
            pictureBox4.Image = Resources.Add;
        }

        private void pictureBox3_MouseEnter(object sender, EventArgs e)
        {
            pictureBox3.Image = Resources.OtherDown;
        }

        private void pictureBox3_MouseLeave(object sender, EventArgs e)
        {
            pictureBox3.Image = Resources.Other;
        }

        private void pictureBox7_MouseEnter(object sender, EventArgs e)
        {
            pictureBox7.Image = Resources.PlayBlankDown;
            label3.BackColor = Color.FromArgb(199, 164, 56);
        }

        private void pictureBox7_MouseLeave(object sender, EventArgs e)
        {
            pictureBox7.Image = Resources.PlayBlank;
            label3.BackColor = Color.FromArgb(236, 193, 63);
        }

        private void label6_MouseEnter(object sender, EventArgs e)
        {
            label6.BackColor = Color.SteelBlue;
        }

        private void label6_MouseLeave(object sender, EventArgs e)
        {
            label6.BackColor = Color.SkyBlue;
        }

        private void label5_MouseEnter(object sender, EventArgs e)
        {
            label5.BackColor = Color.SteelBlue;
        }

        private void label5_MouseLeave(object sender, EventArgs e)
        {
            label5.BackColor = Color.SkyBlue;
        }

        private void label7_MouseEnter(object sender, EventArgs e)
        {
            label7.BackColor = Color.SteelBlue;
        }

        private void label7_MouseLeave(object sender, EventArgs e)
        {
            label7.BackColor = Color.SkyBlue;
        }
        private void label8_MouseEnter(object sender, EventArgs e)
        {
            label8.BackColor = Color.SteelBlue;
        }

        private void label8_MouseLeave(object sender, EventArgs e)
        {
            label8.BackColor = Color.SkyBlue;
        }
        private void label9_MouseEnter(object sender, EventArgs e)
        {
            label9.BackColor = Color.IndianRed;
        }

        private void label9_MouseLeave(object sender, EventArgs e)
        {
            label9.BackColor = Color.SkyBlue;
        }
        #endregion

        #region Other Functions
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

        public void RefreshComboBox1()    //REFRESH METHOD
        {
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = Path.Combine(executablePath, "versions");
            string[] folderNames = Directory.GetDirectories(folderPath);

            comboBox1.Items.Clear();
            listView1.Items.Clear();
            comboBox1.Text = string.Empty;
            comboBox2.Text = string.Empty;
            Settings.Default.StartType = string.Empty;
            Settings.Default.Version = string.Empty;
            Settings.Default.Save();

            imageList = new ImageList();
            imageList.ImageSize = new Size(50, 50);
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            listView1.LargeImageList = imageList; // You can also use LargeImageList

            // Set up columns
            listView1.Columns.Add("Column 1", 500); // Adjust the width as needed
            listView1.Columns.Add("Column 2", 500); // Adjust the width as needed
            listView1.Columns.Add("Column 3", 500); // Adjust the width as needed
            int i = 0;

            foreach (string folderName in folderNames)
            {
                comboBox1.Items.Add(Path.GetFileName(folderName));

                // Load the image and add it to the ImageList
                try
                {
                    using (Image iconImage = Image.FromFile(Path.Combine(folderName, "icon.png")))
                    {
                        imageList.Images.Add($"icon{i}", new Bitmap(iconImage));
                    }
                }
                catch
                {
                    MessageBox.Show("One or more versions have missing icons, please fix and try again..\nOr actually just delete em..");
                    break;
                }

                // Create ListViewItem
                ListViewItem item1 = new ListViewItem(Path.GetFileName(folderName));
                item1.SubItems.Add("Desc 1");
                item1.ImageKey = $"icon{i}"; // Set the key of the image in the ImageList
                listView1.Items.Add(item1);

                i++;
            }

            // Dispose of the images in the ImageList
            foreach (Image img in imageList.Images)
            {
                img.Dispose();
            }
        }

        private bool FilledUp()
        {
            string config = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions", selectedVersion), "server.properties");
            string[] configs = File.ReadAllLines(config);
            string key = "";

            for (int i = 0; i < configs.Length; i++)
            {
                if (configs[i].StartsWith("server-ip="))
                {
                    key = configs[i];
                }
            }

            if (!(key.Length > 10))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private Task RconExecute(string message)
        {
            if (Launcher == null)
            {
                MessageBox.Show("Java has not started yet..");
                return Task.CompletedTask;
            }
            else
            {
                StreamWriter writer = Launcher.StandardInput;
                if (writer != null)
                {
                    writer.WriteLine(message);
                }
                else
                {
                    MessageBox.Show("Standard input redirection is not enabled.");
                }
            }

            return Task.CompletedTask;
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
        #endregion

        private async void Form1_Load(object sender, EventArgs e)
        {
            string releasesUrl = $"https://api.github.com/repos/Arima-Su/Alice-v.3.2/releases/latest";
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "GitHubReleaseDownloader");

            HttpResponseMessage response = await httpClient.GetAsync(releasesUrl);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            GitHubReleaseInfo? releaseInfo = JsonConvert.DeserializeObject<GitHubReleaseInfo>(responseBody);

            if (releaseInfo == null)
            {
                MessageBox.Show("Update encountered an error.");
                return;
            }

            string latestVersion = releaseInfo.tag_name;
            string[] parts = latestVersion.Split('.');
            int intVersion = int.Parse(string.Concat(parts));
            int installIndex = -1;

            if (intVersion <= Settings.Default.LauncherVersion)
            {
                return;
            }

            DialogResult consent = MessageBox.Show($"Update available, install now?", latestVersion, MessageBoxButtons.OKCancel);

            if (consent == DialogResult.OK)
            {
                for (int i = 0; i < releaseInfo.assets.Count(); i++)
                {
                    if (releaseInfo.assets[i].name == "update.exe")
                    {
                        installIndex = i;
                        break;
                    }
                }

                if (installIndex == -1)
                {
                    MessageBox.Show("Update could not be found in the repo, contact @Arimasu for details.");
                    return;
                }

                string downloadUrl = releaseInfo.assets[installIndex].browser_download_url;

                // Download the asset
                HttpResponseMessage assetResponse = await httpClient.GetAsync(downloadUrl);
                assetResponse.EnsureSuccessStatusCode();

                byte[] assetBytes = await assetResponse.Content.ReadAsByteArrayAsync();

                // Save the asset to a file
                File.WriteAllBytes(releaseInfo.assets[installIndex].name, assetBytes);

                DialogResult result = MessageBox.Show($"Update ready, install now?", "", MessageBoxButtons.OK);

                if (result == DialogResult.OK)
                {
                    Settings.Default.LauncherVersion = intVersion;
                    Process.Start(releaseInfo.assets[installIndex].name);
                    await Task.Delay(500);
                    Program.Die();
                }
            }
        }
    }

    public class GitHubReleaseInfo
    {
        public string tag_name { get; set; }
        public GitHubReleaseAsset[] assets { get; set; }
    }

    public class GitHubReleaseAsset
    {
        public string name { get; set; }
        public string browser_download_url { get; set; }
    }
}
