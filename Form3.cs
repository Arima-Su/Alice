using Alice_v._3._2.Properties;
using System.Diagnostics;

namespace Alice_v._3._1
{
    public partial class Form3 : Form
    {

        private Process ?Installer;
        private Process ?Pre;

        public Form3()
        {
            InitializeComponent();
            this.Enabled = true;
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;                          //VERSIONS COMBO BOX OPTIONS SETTER
            string templatesFolderPath = Path.Combine(executablePath, "templates");

            if (Directory.Exists(templatesFolderPath))
            {
                string[] templateFolders = Directory.GetDirectories(templatesFolderPath);

                foreach (string folder in templateFolders)
                {
                    string folderName = Path.GetFileName(folder);
                    comboBox1.Items.Add(folderName);
                }
            }
        }

        #region Controls
        private void button1_Click(object sender, EventArgs e)              //CREATE BUTTON
        {
            if (comboBox1.SelectedItem != null && !string.IsNullOrEmpty(textBox1.Text) && comboBox1.SelectedIndex >= 0)
            {
                string executablePath = AppDomain.CurrentDomain.BaseDirectory;
                string? selectedTemplate = comboBox1.SelectedItem.ToString();
                string newFolderName = textBox1.Text;

                if(selectedTemplate is null)
                {
                    MessageBox.Show("Import error, please try that again..");
                    return;
                }

                if (Directory.Exists(Path.Combine(executablePath, newFolderName)))
                {
                    MessageBox.Show("That version already exists, please pick a different name..");
                    textBox1.Text = "";
                    return;
                }

                this.Enabled = false;
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions", newFolderName);
                Directory.CreateDirectory(folder);
                File.Copy(selectedTemplate, Path.Combine(folder, "installer.jar"));

                label4.Text = "Installing..";
                Installer = new Process();
                Installer.StartInfo.FileName = $"{Path.Combine(executablePath, "Java", "bin", "java.exe")}";
                Installer.StartInfo.Arguments = $"-jar installer.jar --installServer";
                Installer.StartInfo.WorkingDirectory = folder;
                Installer.StartInfo.RedirectStandardOutput = true;
                Installer.StartInfo.UseShellExecute = false;
                Installer.StartInfo.CreateNoWindow = true;
                Installer.OutputDataReceived += Process_OutputDataReceived;

                Installer.Start();
                Installer.BeginOutputReadLine();

                Directory.CreateDirectory(Path.Combine(folder, "mods"));
            }
            else
            {
                MessageBox.Show("Please import a jar file and enter a new folder name.");
                return;
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ' ')
            {
                e.Handled = true; // Cancel the space character
            }

            if (e.KeyChar == (char)Keys.Enter)
            {
                button1_Click(sender, e);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
            "Get new versions from the Forge site, pick the \"Installer\" option. \n\nWould you like to visit it now?",
            "Information",
            MessageBoxButtons.OKCancel);

            // Check the result
            if (result == DialogResult.OK)
            {
                // Open the URL in the default web browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://files.minecraftforge.net/net/minecraftforge/forge/",
                    UseShellExecute = true
                });
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Select a File";
            openFileDialog.Filter = "All Files (*.*)|*.*";

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string? FolderName = Path.GetDirectoryName(openFileDialog.FileName);

                if (FolderName == null)
                {
                    MessageBox.Show("Something went wrong, please try that again..");
                    return;
                }

                string selectedFile = Path.Combine(FolderName, openFileDialog.FileName);

                if (!openFileDialog.FileName.Contains("installer") || !openFileDialog.FileName.Contains("forge") || !openFileDialog.FileName.Contains(".jar"))
                {
                    DialogResult result2 = MessageBox.Show(
                        "File not supported. Must be a an installer jar file from minecraftforge.net. \n\nWould you like to visit it now?",
                        "Information",
                        MessageBoxButtons.OKCancel);

                    // Check the result
                    if (result2 == DialogResult.OK)
                    {
                        // Open the URL in the default web browser
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "https://files.minecraftforge.net/net/minecraftforge/forge/",
                            UseShellExecute = true
                        });
                    }
                    return;
                }

                comboBox1.Items.Clear();
                comboBox1.Items.Add(selectedFile);
                comboBox1.SelectedIndex = 0;
            }
        }
        #endregion

        #region Functions
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e) //CONSOLE QRITER
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                textBox2.Invoke(new Action(() => textBox2.AppendText(e.Data + Environment.NewLine)));

                if (e.Data.Contains("You can delete this installer file now if you wish"))
                {
                    var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions", textBox1.Text);

                    string[] files = Directory.GetFiles(folder);
                    foreach (string file in files)
                    {
                        if (file.Contains("forge"))
                            File.Move(Path.Combine(folder, file), Path.Combine(folder, "server.jar"));
                    }

                    label4.Text = "Installed Successfully";
                    label4.Text = "Running prerequisite (1/2)";

                    PrerequisiteInit(folder);
                }

                if (e.Data.Contains("You need to agree to the EULA in order to run the server."))
                {
                    var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions", textBox1.Text);

                    string eula = Path.Combine(folder, "eula.txt");

                    Thread.Sleep(3000);

                    string[] eulas = File.ReadAllLines(eula);

                    for (int i = 0; i < eulas.Length; i++)
                    {
                        if (eulas[i].StartsWith("eula="))
                        {
                            eulas[i] = "eula=" + "true";
                        }
                    }

                    File.WriteAllLines(eula, eulas);

                    label4.Text = "EULA set up..";
                    label4.Text = "Running prerequisite (2/2)";
                    PrerequisiteInit(folder);
                }

                if (e.Data.Contains("For help, type \"help\""))
                {
                    var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions", textBox1.Text);

                    if (Pre is null)
                    {
                        return;
                    }

                    try
                    {
                        Pre.Kill(true);
                        Pre.Dispose();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                        return;
                    }

                    Thread.Sleep(3000);

                    bool rconFound = false;

                    string config = Path.Combine(folder, "server.properties");
                    string[] configs = File.ReadAllLines(config);
                    for (int i = 0; i < configs.Length; i++)
                    {
                        if (configs[i].StartsWith("server-ip="))
                        {
                            configs[i] = "server-ip=" + $"{Settings.Default.IP}";
                        }
                        else if (configs[i].StartsWith("max-players="))
                        {
                            configs[i] = "max-players=" + $"{Settings.Default.Players}";
                        }
                        else if (configs[i].StartsWith("motd="))
                        {
                            configs[i] = "motd=" + $"{Settings.Default.Motd}";
                        }
                        else if (configs[i].StartsWith("rcon.password=")) //RCON STUFF
                        {
                            configs[i] = "rcon.password=" + "727";
                            rconFound = true;
                        }
                        else if (configs[i].StartsWith("enable-rcon=")) //RCON STUFF
                        {
                            configs[i] = "enable-rcon=" + "true";
                        }
                        else if (configs[i].StartsWith("online-mode=")) //ONLINE STUFF
                        {
                            configs[i] = "online-mode=" + "false";
                        }
                    }

                    if (!rconFound)
                    {
                        configs[configs.Length-1] = $"{configs[configs.Length-1]}\nrcon.password=" + "727";
                    }

                    File.WriteAllLines(config, configs);
                    label4.Text = "Creating icon..";    //ICON STUFF

                    using (Bitmap bitmap = new Bitmap(50, 50))
                    {
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            // Fill the image with a bluank background (heh get it?)
                            g.Clear(Color.FromArgb(0,0,64));
                        }

                        bitmap.Save(Path.Combine(folder, "icon.png"), System.Drawing.Imaging.ImageFormat.Png);
                    }

                    Directory.Delete(Path.Combine(folder, "world"), true);
                    label4.Text = "Done..";
                    MessageBox.Show("Installation Successful");

                    this.Close();
                }

                if (e.Data.Contains("An error report file"))
                {
                    MessageBox.Show("Installation Unsuccessful..");
                    this.Close();
                }
            }
        }

        private void PrerequisiteInit(string folder)
        {
            Pre = new Process();
            Pre.StartInfo.FileName = $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Java", "bin", "java.exe")}";
            Pre.StartInfo.Arguments = $"-Xmx2G -Xms1G -XX:+UseG1GC -jar server.jar --nogui --world sekai";
            Pre.StartInfo.WorkingDirectory = folder;
            Pre.StartInfo.RedirectStandardOutput = true;
            Pre.StartInfo.UseShellExecute = false;
            Pre.StartInfo.CreateNoWindow = true;
            Pre.OutputDataReceived += Process_OutputDataReceived;

            Pre.Start();
            Pre.BeginOutputReadLine();
        }
        #endregion
    }
}
