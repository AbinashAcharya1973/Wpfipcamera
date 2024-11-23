using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Net;

namespace WpfCameraAForge
{
    /// <summary>
    /// Interaction logic for DBManage.xaml
    /// </summary>
    public partial class DBManage : UserControl
    {
       private string connectionString = GlobalSettings.connectinstring;
        private string currentdumpfile;
        public DBManage()
        {
            InitializeComponent();
            
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string location = GlobalSettings.Location;
            string dumpfilename = GetDumpFilePath("cameradb");
            GenerateMySqlDump("localhost", "root", "pass09876", "cameradb", dumpfilename);
        }
        // Function to generate file path with current date and time
        private string GetDumpFilePath(string databaseName)
        {
            // Get the current date and time
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            // Construct the file name based on database name and current date-time
            string fileName = $"{databaseName}_backup_{dateTime}.sql";

            // Choose the default folder (you can modify it)
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // Default to Desktop

            // Combine folder path with the file name
            string filePath = System.IO.Path.Combine(folderPath, fileName);

            return filePath;
        }
        private void GenerateMySqlDump(string server, string username, string password, string database, string filePath)
        {
            // mysqldump command with necessary parameters
            string arguments = $"-h {server} -u {username} -p{password} {database} --result-file=\"{filePath}\" --routines --triggers --events";
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "mysqldump",  // Path to mysqldump, you can specify the full path if needed
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit();

                    // Capture any error output from the process
                    string error = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception(error);
                    }
                }
                currentdumpfile=filePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ", mysqldump Is Not in The System Path, Set mysqldump To System Path","Error",MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(((App)Application.Current).Globalftpurl) || string.IsNullOrEmpty(((App)Application.Current).Globalftpurl))
            {
                MessageBox.Show("FTP URL Not Set for uploading.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                string ftpUrl = ((App)Application.Current).Globalftpurl;
                string username = ((App)Application.Current).Globalftpuid;
                string password = ((App)Application.Current).Globalftppwd;

                //this.downLoadProgress.Value = 0;
                if (string.IsNullOrEmpty(currentdumpfile) || string.IsNullOrWhiteSpace(currentdumpfile)) 
                {
                    //this.downLoadProgress.Maximum = this.UserDataGrid.SelectedItems.Count;
                    
                        
                        UploadFileToFtp(ftpUrl, currentdumpfile, username, password);
                        //this.downLoadProgress.Value++;
                    
                }
                
                MessageBox.Show("File uploaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to upload file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Function to upload a file to FTP server
        private void UploadFileToFtp(string ftpServerIp, string filePath, string username, string password)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string ftpFullUrl = $"ftp://{ftpServerIp}/{fileInfo.Name}";
            //MessageBox.Show(ftpFullUrl);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFullUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // Set the credentials (if required)
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                request.Credentials = new NetworkCredential(username, password);
            }

            // Set the transfer mode
            request.UseBinary = true;
            request.ContentLength = fileInfo.Length;

            // Read the file into a byte array
            byte[] fileContents;
            using (FileStream fileStream = fileInfo.OpenRead())
            {
                fileContents = new byte[fileInfo.Length];
                fileStream.Read(fileContents, 0, fileContents.Length);
            }

            // Write the file contents to the FTP request stream
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(fileContents, 0, fileContents.Length);
            }

            // Get the FTP server response
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                Console.WriteLine($"Upload completed: {response.StatusDescription}");
            }
        }
    }
}
