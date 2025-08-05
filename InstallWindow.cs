using Alice_v._3._2.Properties;
using System.Diagnostics;

namespace Alice_v._3._1
{
    public partial class InstallWindow : Form  //NEW VERSION CREATION
    {

        private Process ?Installer;
        private Process ?Pre;
        private bool agreed = false;

        public InstallWindow()
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
        private void button1_Click(object sender, EventArgs e)  //CREATE BUTTON
        {
            if (comboBox1.SelectedItem != null && !string.IsNullOrEmpty(textBox1.Text) && comboBox1.SelectedIndex >= 0)
            {
                string executablePath = AppDomain.CurrentDomain.BaseDirectory;
                string? selectedTemplate = comboBox1.SelectedItem.ToString();
                string newFolderName = textBox1.Text;

                if (selectedTemplate is null)
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

                if (Settings.Default.javaDirectory.Length < 1)
                {
                    MessageBox.Show("Unconfigured Java directory, set it up via Launcher Settings..");
                    return;
                }

                this.Enabled = false;


                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions", newFolderName);
                Directory.CreateDirectory(folder);

                try
                {
                    File.Copy(selectedTemplate, Path.Combine(folder, "installer.jar"));
                }
                catch
                {
                    this.Enabled = true;
                    MessageBox.Show("Version name already exists.");
                    return;
                }

                label4.Text = "Installing..";

                Installer = new Process();
                //Installer.StartInfo.FileName = $"{Path.Combine(executablePath, "Java", "bin", "java.exe")}";
                Installer.StartInfo.FileName = $"{Settings.Default.javaDirectory}";
                Installer.StartInfo.Arguments = $"-jar installer.jar --installServer";
                Installer.StartInfo.WorkingDirectory = folder;
                Installer.StartInfo.RedirectStandardOutput = true;
                Installer.StartInfo.UseShellExecute = false;
                Installer.StartInfo.CreateNoWindow = true;
                Installer.EnableRaisingEvents = true;
                Installer.OutputDataReceived += Process_OutputDataReceived;

                Installer.Exited += (sender, args) => //HANDLE CLOSE
                {
                    try
                    {
                        var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions", textBox1.Text);

                        string[] files = Directory.GetFiles(folder);
                        foreach (string file in files)
                        {
                            //if ((file.Contains("forge") || file.Contains("minecraft_server")) && file.Contains(".jar"))
                            //    File.Move(Path.Combine(folder, file), Path.Combine(folder, "server.jar"));

                            if (file.Contains("forge") && file.Contains(".jar"))
                                File.Move(Path.Combine(folder, file), Path.Combine(folder, "server.jar"));
                        }

                        Invoke(() =>
                        {
                            label4.Text = "Installed Successfully";
                            label4.Text = "Running prerequisite (1/2)";
                        });

                        PrerequisiteInit(folder);
                    }
                    catch (Exception ex) 
                    {
                        label4.Text = "Install Failed: " + ex.ToString();
                        this.Enabled = true;
                        return;
                    }
                };

                Installer.Start();
                Installer.BeginOutputReadLine();

                Directory.CreateDirectory(Path.Combine(folder, "mods"));
                agreed = false;
            }
            else
            {
                MessageBox.Show("Please import a jar file and enter a new folder name.");
                return;
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)  //NAME TEXT BOX
        {
            if (e.KeyChar == ' ')
            {
                e.Handled = true;
            }

            if (e.KeyChar == (char)Keys.Enter)
            {
                button1_Click(sender, e);
            }
        }

        private void button3_Click(object sender, EventArgs e) //SITE REDIRECT
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

        private void button2_Click(object sender, EventArgs e) //FILE SELECTOR
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
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e) //CONSOLE WRITER
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                textBox2.Invoke(new Action(() => textBox2.AppendText(e.Data + Environment.NewLine)));

                if (e.Data.Contains("An error report file"))
                {
                    Invoke(() =>
                    {
                        MessageBox.Show("Installation Unsuccessful..");
                        this.Enabled = true;
                    });
                }
                
                if (e.Data.Contains("is not recognized as an internal or external command"))
                {
                    Invoke(() =>
                    {
                        MessageBox.Show("Installation Unsuccessful. (Missing Java)");
                        this.Enabled = true;
                    });
                }
            }
        }

        private void PrerequisiteInit(string folder)
        {
            try
            {
                Pre = new Process();

                if (File.Exists(Path.Combine(folder, "server.jar")))  //FOR OLD IMPLEMENTATION
                {
                    //Pre.StartInfo.FileName = $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Java", "bin", "java.exe")}";
                    Pre.StartInfo.FileName = $"{Settings.Default.javaDirectory}";
                    Pre.StartInfo.Arguments = $"-Xmx{Settings.Default.maxRAM}G -Xms{Settings.Default.minRAM}G -XX:+UseG1GC -jar server.jar --nogui --world sekai";
                }
                else  //FOR NEW IMPLEMENTATION
                {
                    Pre.StartInfo.FileName = $"cmd.exe";

                    string[] lines = File.ReadAllLines(Path.Combine(folder, "run.bat"));
                    var cleanedLines = lines.Where(line => !string.Equals(line.Trim(), "pause", StringComparison.OrdinalIgnoreCase)).ToArray();
                    File.WriteAllLines(Path.Combine(folder, "run.bat"), cleanedLines);

                    string jvmArgsPath = Path.Combine(folder, "user_jvm_args.txt");
                    var jvmArgs = File.ReadAllLines(jvmArgsPath).ToList();

                    jvmArgs.RemoveAll(arg => arg.StartsWith("-Xmx") || arg.StartsWith("-Xms"));

                    jvmArgs.Add($"-Xmx{Settings.Default.maxRAM}G");
                    jvmArgs.Add($"-Xms{Settings.Default.minRAM}G");

                    if (!jvmArgs.Any(arg => arg.Equals("-XX:+UseG1GC", StringComparison.OrdinalIgnoreCase)))
                    {
                        jvmArgs.Add("-XX:+UseG1GC");
                    }

                    File.WriteAllLines(jvmArgsPath, jvmArgs);

                    string forgeRoot = Path.Combine(folder, "libraries", "net", "minecraftforge", "forge");
                    string versionFolder = Directory.GetDirectories(forgeRoot).FirstOrDefault();

                    string winArgsPath = Path.Combine(versionFolder, "win_args.txt");

                    Console.WriteLine("Resolved path:");
                    Console.WriteLine(winArgsPath);

                    string progArgsPath = Path.Combine(winArgsPath);
                    var progArgs = File.ReadAllLines(progArgsPath).ToList();

                    if (!progArgs.Contains("--nogui"))
                    {
                        progArgs.Add("--nogui");
                    }

                    if (!progArgs.Contains("--world") && !progArgs.Contains("sekai"))
                    {
                        progArgs.Add("--world");
                        progArgs.Add("sekai");
                    }

                    File.WriteAllLines(progArgsPath, progArgs);

                    Pre.StartInfo.Arguments = $"/c run.bat";
                }

                Pre.StartInfo.WorkingDirectory = folder;
                Pre.StartInfo.RedirectStandardOutput = true;
                Pre.StartInfo.RedirectStandardInput = true;
                Pre.StartInfo.UseShellExecute = false;
                Pre.StartInfo.CreateNoWindow = true;
                Pre.EnableRaisingEvents = true;
                Pre.OutputDataReceived += Process_OutputDataReceived;
                Pre.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        if (args.Data.Contains("For help, type \"help\""))
                        {
                            using (StreamWriter writer = Pre.StandardInput)
                            {
                                writer.WriteLine("/stop");
                            }
                        }
                    }
                };

                Pre.Exited += (sender, args) => //HANDLE CLOSE
                {
                    var eula = Path.Combine(folder, "eula.txt");
                    string config = Path.Combine(folder, "server.properties");

                    Thread.Sleep(3000);

                    if (File.Exists(eula) && !agreed)
                    {
                        string[] eulas = File.ReadAllLines(eula);

                        for (int i = 0; i < eulas.Length; i++)
                        {
                            if (eulas[i].StartsWith("eula="))
                            {
                                if (eulas[i] != "eula=true")
                                {
                                    eulas[i] = "eula=" + "true";
                                }
                                else
                                {
                                    agreed = true;
                                }
                            }
                        }

                        File.WriteAllLines(eula, eulas);

                        Invoke(() =>
                        {
                            label4.Text = "EULA set up..";
                            label4.Text = "Running prerequisite (2/2)";
                        });

                        PrerequisiteInit(folder);
                    }
                    else if (File.Exists(config))
                    {
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
                            else if (configs[i].StartsWith("online-mode=")) //ONLINE STUFF
                            {
                                configs[i] = "online-mode=" + "false";
                            }
                        }

                        File.WriteAllLines(config, configs);
                        Invoke(() =>
                        {
                            label4.Text = "Creating icon..";    //ICON STUFF
                        });

                        using (Bitmap bitmap = new Bitmap(50, 50))
                        {
                            using (Graphics g = Graphics.FromImage(bitmap))
                            {
                                // Fill the image with a bluank background (heh get it?)
                                g.Clear(Color.FromArgb(0, 0, 64));
                            }

                            bitmap.Save(Path.Combine(folder, "icon.png"), System.Drawing.Imaging.ImageFormat.Png);
                        }

                        if (File.Exists(Path.Combine(folder, "world")))
                        {
                            Directory.Delete(Path.Combine(folder, "world"), true);
                        }

                        Invoke(() =>
                        {
                            label4.Text = "Installation successful.";
                            this.Enabled = true;
                        });
                    }
                };

                Pre.Start();
                Pre.BeginOutputReadLine();
            }
            catch (Exception ex)
            {
                Invoke(() =>
                {
                    label4.Text = "Installation unsuccessful.\n " + ex.ToString();
                    this.Enabled = true;
                });

                return;
            }
        }
        #endregion
    }
}
