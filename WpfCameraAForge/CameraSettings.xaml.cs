using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace WpfCameraAForge
{
    /// <summary>
    /// Interaction logic for CameraSettings.xaml
    /// </summary>
    public partial class CameraSettings : UserControl
    {
        private string connectionString = GlobalSettings.connectinstring;
        private ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

        public CameraSettings()
        {
            InitializeComponent();
            LoadCameraSettings();
            LoadRecordingSettings();
            LoadUserList();
            LoadFtpDetails();
        }

        private void LoadCameraSettings()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM camerasettings";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    int cameraIndex = 1;

                    while (reader.Read())
                    {
                        string ip = reader["IPAddress"].ToString();
                        string protocol = reader["Protocol"].ToString();
                        string userId = reader["UserID"].ToString();
                        string password = reader["Password"].ToString();
                        string port = reader["Port"].ToString();
                        string url = reader["URL"].ToString();

                        if (cameraIndex == 1)
                        {
                            // Populate fields for Camera 1
                            Camera1IPTextBox.Text = ip;
                            Camera1ProtocolTextBox.Text = protocol;
                            Camera1UserIDTextBox.Text = userId;
                            Camera1PasswordBox.Password = password;
                            Camera1PortTextBox.Text = port;
                            //UpdateURL(Camera1ProtocolTextBox.Text, Camera1UserIDTextBox.Text, Camera1PasswordBox.Password, Camera1IPTextBox.Text, Camera1PortTextBox.Text, Camera1URLTextBox);
                            Camera1URLTextBox.Text = url;
                        }
                        else if (cameraIndex == 2)
                        {
                            // Populate fields for Camera 2
                            Camera2IPTextBox.Text = ip;
                            Camera2ProtocolTextBox.Text = protocol;
                            Camera2UserIDTextBox.Text = userId;
                            Camera2PasswordBox.Password = password;
                            Camera2PortTextBox.Text = port;
                            //UpdateURL(Camera2ProtocolTextBox.Text, Camera2UserIDTextBox.Text, Camera2PasswordBox.Password, Camera2IPTextBox.Text, Camera2PortTextBox.Text, Camera2URLTextBox);
                            Camera2URLTextBox.Text = url;
                        }
                        else if (cameraIndex == 3)
                        {
                            // Populate fields for Camera 3
                            Camera3IPTextBox.Text = ip;
                            Camera3ProtocolTextBox.Text = protocol;
                            Camera3UserIDTextBox.Text = userId;
                            Camera3PasswordBox.Password = password;
                            Camera3PortTextBox.Text = port;
                            //UpdateURL(Camera3ProtocolTextBox.Text, Camera3UserIDTextBox.Text, Camera3PasswordBox.Password, Camera3IPTextBox.Text, Camera3PortTextBox.Text, Camera3URLTextBox);
                            Camera3URLTextBox.Text = url;
                        }

                        cameraIndex++;
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load camera settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UpdateURL(string protocol, string userId, string password, string ip, string port, TextBox urlTextBox)
        {
            // Generate and display the URL
            urlTextBox.Text = $"{protocol}://{userId}:{password}@{ip}:{port}";
        }

        private void UpdateCamera1Settings_Click(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Update Camera 1 Settings
                    UpdateCameraSettings(connection, "Camera 1", Camera1IPTextBox.Text, Camera1ProtocolTextBox.Text, Camera1UserIDTextBox.Text, Camera1PasswordBox.Password, Camera1URLTextBox.Text,this.Camera1PortTextBox.Text);

                    // Update Recording Settings
                    UpdateRecordingSettings(connection);

                    MessageBox.Show("Camera 1 settings updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    var camToUpdate = GlobalSettings.Cameras.FirstOrDefault(item => item.id == "1");
                    if (camToUpdate != null)
                    {
                        camToUpdate.IPAddress=this.Camera1IPTextBox.Text;
                        camToUpdate.Port=this.Camera1PortTextBox.Text;
                        camToUpdate.Protocol=this.Camera1ProtocolTextBox.Text;
                        camToUpdate.Password=this.Camera1PasswordBox.Password;
                        camToUpdate.UserID=this.Camera1UserIDTextBox.Text;
                        camToUpdate.URL=this.Camera1URLTextBox.Text;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to update settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void UpdateCamera2Settings_Click(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Update Camera 2 Settings
                    UpdateCameraSettings(connection, "Camera 2", Camera2IPTextBox.Text, Camera2ProtocolTextBox.Text, Camera2UserIDTextBox.Text, Camera2PasswordBox.Password, Camera2URLTextBox.Text,this.Camera2PortTextBox.Text);

                    // Update Recording Settings
                    UpdateRecordingSettings(connection);

                    MessageBox.Show("Camera 2 settings updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    var camToUpdate = GlobalSettings.Cameras.FirstOrDefault(item => item.id == "2");
                    if (camToUpdate != null)
                    {
                        camToUpdate.IPAddress = this.Camera2IPTextBox.Text;
                        camToUpdate.Port = this.Camera2PortTextBox.Text;
                        camToUpdate.Protocol = this.Camera2ProtocolTextBox.Text;
                        camToUpdate.Password = this.Camera2PasswordBox.Password;
                        camToUpdate.UserID = this.Camera2UserIDTextBox.Text;
                        camToUpdate.URL = this.Camera2URLTextBox.Text;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to update settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void UpdateCamera3Settings_Click(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Update Camera 2 Settings
                    UpdateCameraSettings(connection, "Camera 3", Camera3IPTextBox.Text, Camera3ProtocolTextBox.Text, Camera3UserIDTextBox.Text, Camera3PasswordBox.Password, Camera3URLTextBox.Text,Camera3PortTextBox.Text);

                    // Update Recording Settings
                    UpdateRecordingSettings(connection);

                    MessageBox.Show("Camera 3 settings updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    var camToUpdate = GlobalSettings.Cameras.FirstOrDefault(item => item.id == "3");
                    if (camToUpdate != null)
                    {
                        camToUpdate.IPAddress = this.Camera3IPTextBox.Text;
                        camToUpdate.Port = this.Camera3PortTextBox.Text;
                        camToUpdate.Protocol = this.Camera3ProtocolTextBox.Text;
                        camToUpdate.Password = this.Camera3PasswordBox.Password;
                        camToUpdate.UserID = this.Camera3UserIDTextBox.Text;
                        camToUpdate.URL = this.Camera3URLTextBox.Text;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to update settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UpdateCameraSettings(MySqlConnection connection, string camera, string ip, string protocol, string userId, string password, string url,string port)
        {
            string query = "UPDATE camerasettings SET IPAddress=@ip, Protocol=@protocol, UserID=@userID, Password=@password, URL=@url,Port=@port WHERE Cameras=@camera";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@camera", camera);
            cmd.Parameters.AddWithValue("@ip", ip);
            cmd.Parameters.AddWithValue("@protocol", protocol);
            cmd.Parameters.AddWithValue("@userID", userId);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.Parameters.AddWithValue("@url", url);
            cmd.Parameters.AddWithValue("@port", port);
            cmd.ExecuteNonQuery();
        }

        private void UpdateRecordingSettings(MySqlConnection connection)
        {
            try
            {
                string query = "UPDATE recordingsettings SET RecordingInt=@recordingInt, ImagePS=@imagePS, MaxRecordT=@maxRecordT, Location=@location WHERE id=1";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@recordingInt", RecordingIntervalTextBox.Text);
                cmd.Parameters.AddWithValue("@imagePS", ImagesPerSetTextBox.Text);
                cmd.Parameters.AddWithValue("@maxRecordT", MaxRecordTimeTextBox.Text);
                cmd.Parameters.AddWithValue("@location", LocationTextBox.Text);
                cmd.ExecuteNonQuery();
                int inttime, imgpersec, maxrectime;
                bool cs = int.TryParse(this.RecordingIntervalTextBox.Text, out inttime);
                if (cs)
                {
                    GlobalSettings.RecordingInterval = inttime;
                }
                bool cs1 = int.TryParse(this.ImagesPerSetTextBox.Text, out imgpersec);
                if (cs1)
                {
                    GlobalSettings.ImagesPerSet = imgpersec;
                }
                bool cs2 = int.TryParse(this.MaxRecordTimeTextBox.Text, out maxrectime);
                if (cs2)
                {
                    GlobalSettings.MaxRecordTime = maxrectime;
                }
                GlobalSettings.Location = LocationTextBox.Text;
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                
            }
        }

        private void LoadRecordingSettings()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM recordingsettings WHERE id=1";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        RecordingIntervalTextBox.Text = reader["RecordingInt"].ToString();
                        ImagesPerSetTextBox.Text = reader["ImagePS"].ToString();
                        MaxRecordTimeTextBox.Text = reader["MaxRecordT"].ToString();
                        LocationTextBox.Text = reader["Location"].ToString();
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load recording settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadUserList()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT id, UserID, UserName, UserType FROM users";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    Users.Clear(); // Clear the existing collection

                    while (reader.Read())
                    {
                        // Add user data to ObservableCollection
                        Users.Add(new User
                        {
                            id = Convert.ToInt32(reader["id"]),
                            UserID = reader["UserID"].ToString(),
                            UserName = reader["UserName"].ToString(),
                            UserType = reader["UserType"].ToString()
                        });
                    }

                    reader.Close();
                    this.UserDataGrid.ItemsSource = Users;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load user list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        // Other methods for loading camera and recording settings...
        private void UpdateRecordingSettings_Click(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Update Recording Settings
                    UpdateRecordingSettings(connection);

                    MessageBox.Show("Recording settings updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to update recording settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            string userName = NewUserNameTextBox.Text;
            string userId = NewUserIDTextBox.Text;
            string password = NewUserPasswordBox.Password;
            string userType = (UserTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(userType))
            {
                MessageBox.Show("Please fill all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO users (UserName, UserID, Password, UserType) VALUES (@userName, @userId, @password, @userType)";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@userName", userName);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@userType", userType);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("User added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Clear the fields
                    NewUserNameTextBox.Clear();
                    NewUserIDTextBox.Clear();
                    NewUserPasswordBox.Clear();
                    UserTypeComboBox.SelectedIndex = -1;

                    // Reload the user list after addition
                    LoadUserList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to add user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Camera1IPTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Camera1URLTextBox.Text = this.Camera1ProtocolTextBox.Text + "://" + this.Camera1UserIDTextBox.Text + ":" + this.Camera1PasswordBox.Password+"@"+this.Camera1IPTextBox.Text+":"+this.Camera1PortTextBox.Text;
            //{camera1.Protocol}://{camera1.UserID}:{camera1.Password}@{camera1.IPAddress}:{camera1.Port}";
        }

        private void Camera1PortTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Camera1URLTextBox.Text = this.Camera1ProtocolTextBox.Text + "://" + this.Camera1UserIDTextBox.Text + ":" + this.Camera1PasswordBox.Password + "@" + this.Camera1IPTextBox.Text + ":" + this.Camera1PortTextBox.Text;
        }

        private void Camera1ProtocolTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Camera1URLTextBox.Text = this.Camera1ProtocolTextBox.Text + "://" + this.Camera1UserIDTextBox.Text + ":" + this.Camera1PasswordBox.Password + "@" + this.Camera1IPTextBox.Text + ":" + this.Camera1PortTextBox.Text;
        }

        private void Camera1UserIDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Camera1URLTextBox.Text = this.Camera1ProtocolTextBox.Text + "://" + this.Camera1UserIDTextBox.Text + ":" + this.Camera1PasswordBox.Password + "@" + this.Camera1IPTextBox.Text + ":" + this.Camera1PortTextBox.Text;
        }
        

        private void Camera1Password_Changed(object sender, RoutedEventArgs e)
        {
            Camera1URLTextBox.Text = this.Camera1ProtocolTextBox.Text + "://" + this.Camera1UserIDTextBox.Text + ":" + this.Camera1PasswordBox.Password + "@" + this.Camera1IPTextBox.Text + ":" + this.Camera1PortTextBox.Text;
        }

        private void Camera2IPTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Camera2URLTextBox.Text = this.Camera2ProtocolTextBox.Text + "://" + this.Camera2UserIDTextBox.Text + ":" + this.Camera2PasswordBox.Password + "@" + this.Camera2IPTextBox.Text + ":" + this.Camera2PortTextBox.Text;
        }

        private void Camera2PortTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Camera2URLTextBox.Text = this.Camera2ProtocolTextBox.Text + "://" + this.Camera2UserIDTextBox.Text + ":" + this.Camera2PasswordBox.Password + "@" + this.Camera2IPTextBox.Text + ":" + this.Camera2PortTextBox.Text;
        }

        private void Camera2ProtocolTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Camera2URLTextBox.Text = this.Camera2ProtocolTextBox.Text + "://" + this.Camera2UserIDTextBox.Text + ":" + this.Camera2PasswordBox.Password + "@" + this.Camera2IPTextBox.Text + ":" + this.Camera2PortTextBox.Text;
        }

        private void Camera2UserIDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Camera2URLTextBox.Text = this.Camera2ProtocolTextBox.Text + "://" + this.Camera2UserIDTextBox.Text + ":" + this.Camera2PasswordBox.Password + "@" + this.Camera2IPTextBox.Text + ":" + this.Camera2PortTextBox.Text;
        }

        private void Camera2PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Camera2URLTextBox.Text = this.Camera2ProtocolTextBox.Text + "://" + this.Camera2UserIDTextBox.Text + ":" + this.Camera2PasswordBox.Password + "@" + this.Camera2IPTextBox.Text + ":" + this.Camera2PortTextBox.Text;
        }

        private void Camera3IPTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Camera3URLTextBox.Text = this.Camera3ProtocolTextBox.Text + "://" + this.Camera3UserIDTextBox.Text + ":" + this.Camera3PasswordBox.Password + "@" + this.Camera3IPTextBox.Text + ":" + this.Camera3PortTextBox.Text;
        }

        private void Camera3PortTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Camera3URLTextBox.Text = this.Camera3ProtocolTextBox.Text + "://" + this.Camera3UserIDTextBox.Text + ":" + this.Camera3PasswordBox.Password + "@" + this.Camera3IPTextBox.Text + ":" + this.Camera3PortTextBox.Text;
        }

        private void Camera3ProtocolTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Camera3URLTextBox.Text = this.Camera3ProtocolTextBox.Text + "://" + this.Camera3UserIDTextBox.Text + ":" + this.Camera3PasswordBox.Password + "@" + this.Camera3IPTextBox.Text + ":" + this.Camera3PortTextBox.Text;
        }

        private void Camera3UserIDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Camera3URLTextBox.Text = this.Camera3ProtocolTextBox.Text + "://" + this.Camera3UserIDTextBox.Text + ":" + this.Camera3PasswordBox.Password + "@" + this.Camera3IPTextBox.Text + ":" + this.Camera3PortTextBox.Text;
        }

        private void Camera3PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Camera3URLTextBox.Text = this.Camera3ProtocolTextBox.Text + "://" + this.Camera3UserIDTextBox.Text + ":" + this.Camera3PasswordBox.Password + "@" + this.Camera3IPTextBox.Text + ":" + this.Camera3PortTextBox.Text;
        }

        private void btnftpUpdate_Click(object sender, RoutedEventArgs e)
        {
            // Get the values from the TextBoxes and PasswordBox
            string ftpUrl = this.FtpIPTextBox.Text;
            string username = this.FtpUserIDTextBox.Text;
            string password = this.FtpUserPasswordBox.Password;

            // Check if fields are empty
            if (string.IsNullOrEmpty(ftpUrl) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill all fields.");
                return;
            }

            // SQL query to update the ftp table
            string query = "UPDATE ftp SET url = @ftpUrl, username = @username, password = @password WHERE id = 1";  // Adjust WHERE clause as per your table

            // Execute the query
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ftpUrl", ftpUrl);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("FTP details updated successfully.");
                        ((App)Application.Current).Globalftpurl=ftpUrl;
                        ((App)Application.Current).Globalftpuid=username;
                        ((App)Application.Current).Globalftppwd=password;
                    }
                    else
                    {
                        MessageBox.Show("Update failed. Please try again.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void LoadFtpDetails()
        {
            string query = "SELECT url, username, password FROM ftp WHERE id = 1";  // Adjust WHERE clause as per your table

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Populate the TextBoxes and PasswordBox with the data from the database
                            this.FtpIPTextBox.Text = reader["url"].ToString();
                            this.FtpUserIDTextBox.Text = reader["username"].ToString();
                            this.FtpUserPasswordBox.Password = reader["password"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("No FTP details found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading FTP details: " + ex.Message);
            }
        }

        private void btnDBUpdate_Click(object sender, RoutedEventArgs e)
        {
            String filePath = "dbconfig.txt";
            string server = this.DBHostIPTextBox.Text;            
            string username = this.DBUserIDTextBox.Text;
            string password = this.DBUserPasswordBox.Password; // Consider encrypting the password

            string[] lines = {
            $"{server}",            
            $"{username}",
            $"{password}"
            };
            try
            {
                File.WriteAllLines(filePath, lines);
                MessageBox.Show("DB Config Updated","DB Config",MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { 
                MessageBox.Show(ex.Message, "Error",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        private void CurrentPath_Click(object sender, RoutedEventArgs e)
        {
            if (this.CurrentPath.IsChecked == true) {
                this.LocationTextBox.Text = Directory.GetCurrentDirectory();
            }
        }
    }

    public class User
    {
        public int id { get; set; }

        public string UserID { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        
    }
}
