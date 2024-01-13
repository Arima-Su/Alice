namespace Alice_v._3._1
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = Path.Combine(executablePath, "versions");
            string java = Path.Combine(executablePath, "Java");

            if(!Directory.Exists(folderPath))   //VERSIONS FOLDER CHECK
            {
                Directory.CreateDirectory(folderPath);
            }

            if(!Directory.Exists(java))    //JAVA CHECK
            {
                MessageBox.Show("Missing Java package, please redownload and unzip properly.");
                return;
            }

            if(!File.Exists(Path.Combine(executablePath, "Alice v.3.2.dll.config")) || 
               !File.Exists(Path.Combine(executablePath, "D3DCompiler_47_cor3.dll")) ||
               !File.Exists(Path.Combine(executablePath, "PenImc_cor3.dll")) ||
               !File.Exists(Path.Combine(executablePath, "PresentationNative_cor3.dll")) ||
               !File.Exists(Path.Combine(executablePath, "vcruntime140_cor3.dll")) ||
               !File.Exists(Path.Combine(executablePath, "wpfgfx_cor3.dll"))
               )
            {
                MessageBox.Show("Missing drivers, please redownload and unzip properly.");
                return;
            }

            try
            {
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}