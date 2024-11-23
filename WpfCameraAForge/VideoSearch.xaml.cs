using Google.Protobuf.Compiler;
using LibVLCSharp.Shared;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.IO;
//using System.Windows.Media;
using System.Net;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Forms;

namespace WpfCameraAForge
{
    /// <summary>
    /// Interaction logic for VideoSearch.xaml
    /// </summary>
    public partial class VideoSearch : UserControl
    {
        private string connectionString = GlobalSettings.connectinstring;
        private ObservableCollection<recordings> RecordingList { get; set; } = new ObservableCollection<recordings>();
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer1;
        private MediaPlayer _mediaPlayer2;
        private MediaPlayer _mediaPlayer3;

        public VideoSearch()
        {
            InitializeComponent();
            LoadRecordingList();
            _libVLC = new LibVLC();
            _mediaPlayer1 = new MediaPlayer(_libVLC);
            _mediaPlayer2 = new MediaPlayer(_libVLC);
            _mediaPlayer3 = new MediaPlayer(_libVLC);
            this.WebcamImage1.MediaPlayer = _mediaPlayer1;
            this.WebcamImage2.MediaPlayer = _mediaPlayer2;
            this.WebcamImage3.MediaPlayer = _mediaPlayer3;
        }
        private void LoadRecordingList()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT id, RDateTime, RPath, FileType,barcode,camera,Shift,UserID FROM recordings";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    RecordingList.Clear(); // Clear the existing collection

                    while (reader.Read())
                    {
                        // Add user data to ObservableCollection
                        RecordingList.Add(new recordings
                        {
                            id = Convert.ToInt32(reader["id"]),
                            barcode = reader["barcode"].ToString(),
                            FileType = reader["FileType"].ToString(),
                            FileName = reader["RPath"].ToString(),
                            RDateTime = (DateTime)reader["RDateTime"],
                            Camera = reader["camera"].ToString(),
                            Shift = reader["Shift"].ToString(),
                            User = reader["UserID"].ToString()
                        });
                    }

                    reader.Close();
                    this.UserDataGrid.ItemsSource = RecordingList;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load user list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UserDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string recordedVideoPath="aa";
            string camno = "";
            // Path to the recorded video file
            

            try
            {
                if (this.UserDataGrid.SelectedItem is recordings selectedFile)
                {
                    recordedVideoPath = selectedFile.FileName;  // Store the selected file path
                    camno= selectedFile.Camera.ToString();
                }

                // Ensure the file exists
                if (!System.IO.File.Exists(recordedVideoPath))
                {
                    MessageBox.Show("Video file not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // Create a new media with the video file path
                using (var media = new Media(_libVLC, recordedVideoPath, FromType.FromPath))
                {
                    // Play the video
                    if(camno=="Camera1")
                    _mediaPlayer1.Play(media);
                    if (camno == "Camera2")
                        _mediaPlayer2.Play(media);
                    if (camno == "Camera3" || camno == "Insta360")
                        _mediaPlayer3.Play(media);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error playing video: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT id, RDateTime, RPath, FileType,barcode,camera,Shift,UserID FROM recordings where barcode like '" + this.txtSearch.Text + "%'";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    RecordingList.Clear(); // Clear the existing collection

                    while (reader.Read())
                    {
                        // Add user data to ObservableCollection
                        RecordingList.Add(new recordings
                        {
                            id = Convert.ToInt32(reader["id"]),
                            barcode = reader["barcode"].ToString(),
                            FileType = reader["FileType"].ToString(),
                            FileName = reader["RPath"].ToString(),
                            RDateTime = (DateTime)reader["RDateTime"],
                            Camera = reader["camera"].ToString(),
                            Shift = reader["Shift"].ToString(),
                            User = reader["UserID"].ToString()
                        });
                    }

                    reader.Close();
                    this.UserDataGrid.ItemsSource = RecordingList;

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load Recording list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SearchDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchDate.SelectedDate.HasValue)
            {
                DateTime DateSelected=SearchDate.SelectedDate.Value;
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = @"SELECT id, RDateTime, RPath, FileType,barcode,camera,Shift,UserID FROM recordings where DATE(RDateTime)=@DateSelected";
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@DateSelected", DateSelected.Date);
                        MySqlDataReader reader = cmd.ExecuteReader();

                        RecordingList.Clear(); // Clear the existing collection

                        while (reader.Read())
                        {
                            // Add user data to ObservableCollection
                            RecordingList.Add(new recordings
                            {
                                id = Convert.ToInt32(reader["id"]),
                                barcode = reader["barcode"].ToString(),
                                FileType = reader["FileType"].ToString(),
                                FileName = reader["RPath"].ToString(),
                                RDateTime = (DateTime)reader["RDateTime"],
                                Camera = reader["camera"].ToString(),
                                Shift = reader["Shift"].ToString(),
                                User = reader["UserID"].ToString()
                            });
                        }

                        reader.Close();
                        this.UserDataGrid.ItemsSource = RecordingList;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to load Recording list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

            }

        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            string selectedPath = "";
            try
            {
                using (System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    folderDialog.Description = "Select the destination folder";
                    folderDialog.ShowNewFolderButton = true;

                    // Show the dialog and get the result
                    System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
                    {
                        selectedPath = folderDialog.SelectedPath;
                        MessageBox.Show($"Selected folder: {selectedPath}");

                        // You can now use the selectedPath for saving files or any other purpose.
                    }
                    else
                    {
                        MessageBox.Show("No folder was selected.");
                    }
                }
                if (this.UserDataGrid.SelectedItems.Count > 0)
                {
                    this.downLoadProgress.Maximum = this.UserDataGrid.SelectedItems.Count;
                    this.downLoadProgress.Value = 0;
                    foreach (var item in this.UserDataGrid.SelectedItems)
                    {
                        var selectedItem = item as recordings;
                        if (selectedItem != null)
                        {
                            //MessageBox.Show(selectedItem.FileName);
                            string destinationFilePath = System.IO.Path.Combine(selectedPath, System.IO.Path.GetFileName(selectedItem.FileName));
                            File.Copy(selectedItem.FileName, destinationFilePath, true);
                            this.downLoadProgress.Value++;
                        }
                    }
                    MessageBox.Show($"Finished", "File Copy", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var ans = MessageBox.Show("No File Selected, Do You Want to Download the List?", "Download", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (ans == MessageBoxResult.Yes)
                    {
                        this.downLoadProgress.Maximum = this.UserDataGrid.Items.Count;
                        this.downLoadProgress.Value = 0;
                        foreach (var item in this.UserDataGrid.Items)
                        {
                            var selectedItem = item as recordings;
                            if (selectedItem != null)
                            {
                                string destinationFilePath = System.IO.Path.Combine(selectedPath, System.IO.Path.GetFileName(selectedItem.FileName));
                                File.Copy(selectedItem.FileName, destinationFilePath, true);
                                this.downLoadProgress.Value++;
                            }
                        }
                        MessageBox.Show($"Finished", "File Copy", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"File/Directory Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                if (MessageBox.Show("Are You Sure You Want to Delete the Selected Items?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    string ftpUrl = ((App)Application.Current).Globalftpurl;
                    string username = ((App)Application.Current).Globalftpuid;
                    string password = ((App)Application.Current).Globalftppwd;
                    this.downLoadProgress.IsIndeterminate=true;
                    this.downLoadProgress.Value = 0;
                    if (this.UserDataGrid.SelectedItems.Count > 0)
                    {
                        this.downLoadProgress.Maximum = this.UserDataGrid.SelectedItems.Count;
                        foreach (var item in this.UserDataGrid.SelectedItems)
                        {
                            var selectedItem = item as recordings;
                            UploadFileToFtp(ftpUrl, selectedItem.FileName, username, password, selectedItem.barcode);
                            this.downLoadProgress.Value++;
                        }
                    }
                    else
                    {
                        this.downLoadProgress.Maximum = this.UserDataGrid.Items.Count;
                        foreach (var item in this.UserDataGrid.Items)
                        {
                            var selectedItem = item as recordings;
                            UploadFileToFtp(ftpUrl, selectedItem.FileName, username, password, selectedItem.barcode);
                            this.downLoadProgress.Value++;
                        }
                    }
                    this.downLoadProgress.IsIndeterminate = false;
                    MessageBox.Show("File uploaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to upload file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.downLoadProgress.IsIndeterminate = false;
            }
        }
        // Function to upload a file to FTP server
        private void UploadFileToFtp(string ftpServerIp, string filePath, string username, string password, string barcode)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            // Folder path for the current barcode
            /*string folderUri = $"ftp://{ftpServerIp}/{barcode}";

            // Check if the folder exists, if not, create it
            if (!FtpDirectoryExists(folderUri, username, password))
            {
                CreateFtpDirectory(folderUri, username, password);
            }

            // Full path including folder and file name for the upload
            string ftpFullUrl = $"{folderUri}/{fileInfo.Name}";*/
            string ftpFullUrl = $"ftp://{ftpServerIp}/{barcode}_{fileInfo.Name}";
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFullUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // Set the credentials
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                request.Credentials = new NetworkCredential(username, password);
            }

            // Set the transfer mode
            request.UseBinary = true;
            request.UsePassive = true;
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
        public static bool FtpDirectoryExists(string folderUri, string username, string password)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(folderUri);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(username, password);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    return true;  // Folder exists
                }
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response != null && response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    // Folder does not exist
                    return false;
                }
                else
                {
                    throw;  // Some other error occurred
                }
            }
        }
        public static void CreateFtpDirectory(string folderUri, string username, string password)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(folderUri);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.Credentials = new NetworkCredential(username, password);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                // Directory created successfully
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.UserDataGrid.SelectedItems.Count > 0)
                {
                    if (MessageBox.Show("Are You Sure You Want to Delete the Selected Items?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        var selecteditems = this.UserDataGrid.SelectedItems;
                        var itemstobedeleted = new ObservableCollection<recordings>();
                        foreach (recordings item in selecteditems)
                        {
                            itemstobedeleted.Add(item);
                        }
                        foreach (recordings item in itemstobedeleted)
                        {
                            DeleteFileAndRecord(item.FileName, item.id);
                            RecordingList.Remove(item);
                        }
                        MessageBox.Show("Record Deletion Finished.");
                    }
                }
                else
                {
                    if (MessageBox.Show("Are You Sure You Want to Delete the Listed Items?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        var selecteditems = this.UserDataGrid.Items;
                        var itemstobedeleted = new ObservableCollection<recordings>();
                        foreach (recordings item in selecteditems)
                        {
                            itemstobedeleted.Add(item);
                        }
                        foreach (recordings item in itemstobedeleted)
                        {
                            DeleteFileAndRecord(item.FileName, item.id);
                            RecordingList.Remove(item);
                        }
                        MessageBox.Show("Record Deletion Finished.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to Delete the record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DeleteFileAndRecord(string filePath, int recordId)
        {
            try
            {
                // Step 1: Delete the file
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    
                }
                else
                {
                    MessageBox.Show("File not found.");
                }

                // Step 2: Delete the record from MySQL
                DeleteMySqlRecord(recordId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        private void DeleteMySqlRecord(int recordId)
        {
            
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM recordings WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", recordId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            //MessageBox.Show("Record deleted successfully.");
                        }
                        else
                        {
                            MessageBox.Show("No record found with the given ID.");
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"MySQL error: {ex.Message}");
                }
            }
        }
    }
    public class recordings
    {
        public int id {  get; set; }
        public string barcode { get; set; }
        public string FileType { get; set; }
        public DateTime RDateTime { get; set; }
        public String FileName { get; set; }
        public String Camera { get; set; }
        public String User { get; set; }
        public String Shift { get; set; }

    }
}
