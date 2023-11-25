using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Alice_v._3._1
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();

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
            else
            {
                MessageBox.Show("Templates folder not found!");
            }
        }

        private void button1_Click(object sender, EventArgs e)              //CREATE BUTTON
        {
            string selectedTemplate = comboBox1.SelectedItem?.ToString();
            string newFolderName = textBox1.Text;

            if (!string.IsNullOrEmpty(selectedTemplate) && !string.IsNullOrEmpty(newFolderName))
            {
                string executablePath = AppDomain.CurrentDomain.BaseDirectory;
                string templatesFolderPath = Path.Combine(executablePath, "templates");
                string selectedTemplateFolderPath = Path.Combine(templatesFolderPath, selectedTemplate);

                if (Directory.Exists(selectedTemplateFolderPath))
                {
                    string destinationFolderPath = Path.Combine(executablePath, "versions", newFolderName);

                    // Create the new folder
                    Directory.CreateDirectory(destinationFolderPath);

                    // Copy the contents of the selected template folder to the new folder
                    CopyFolder(selectedTemplateFolderPath, destinationFolderPath);

                    MessageBox.Show("Version created successfully..");

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Selected template folder not found!");
                }
            }
            else
            {
                MessageBox.Show("Please select a template and enter a new folder name.");
            }
        }

        private void CopyFolder(string sourceFolderPath, string destinationFolderPath)              //COPY TO FOLDER METHOD
        {
            // Create all directories if they don't exist
            if (!Directory.Exists(destinationFolderPath))
            {
                Directory.CreateDirectory(destinationFolderPath);
            }

            // Copy files
            string[] files = Directory.GetFiles(sourceFolderPath);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string destinationFilePath = Path.Combine(destinationFolderPath, fileName);
                File.Copy(file, destinationFilePath);
            }

            // Copy subdirectories recursively
            string[] subdirectories = Directory.GetDirectories(sourceFolderPath);
            foreach (string subdirectory in subdirectories)
            {
                string subdirectoryName = Path.GetFileName(subdirectory);
                string destinationSubdirectoryPath = Path.Combine(destinationFolderPath, subdirectoryName);
                CopyFolder(subdirectory, destinationSubdirectoryPath);
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ' ')
            {
                e.Handled = true; // Cancel the space character
            }
        }
    }
}
