using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace WpfCameraAForge
{
    public partial class Login : Window
    {
        private string connectionString;
        public Login()
        {
            InitializeComponent();
            LoadDbConfig();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string server = URLTextBox.Text;
            string username = UserTextBox.Text;
            string password = PasswordTextBox.Password;
            string shift = ((ComboBoxItem)ShiftComboBox.SelectedItem)?.Content.ToString();

            if (ValidateLogin(username, password))
            {
                LogUserLogin(username, shift);
                LoadSettingsAfterLogin();  // Load settings immediately after successful login
                MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Open the main window or continue with the next operation after login
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid login details!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Function to validate login credentials
        private bool ValidateLogin(string username, string password)
        {
            bool isValid = false;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT COUNT(*) FROM users WHERE UserID=@username AND Password=@password";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        int result = Convert.ToInt32(command.ExecuteScalar());
                        isValid = result > 0;  // If the query returns 1 or more, credentials are valid
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return isValid;
        }

        // Function to log the user's login details into the UserLog table
        private void LogUserLogin(string username, string shift)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "INSERT INTO UserLog (Username, Shift, LoginTime, LoginDate) VALUES (@username, @shift, @loginTime, @loginDate)";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@shift", shift);
                        command.Parameters.AddWithValue("@loginTime", DateTime.Now.ToString("HH:mm:ss"));
                        command.Parameters.AddWithValue("@loginDate", DateTime.Now.ToString("yyyy-MM-dd"));

                        command.ExecuteNonQuery();  // Execute the insert query
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to log user login: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Function to load camera and recording settings after login
        public void LoadSettingsAfterLogin()
        {
            string username = UserTextBox.Text;
            string password = PasswordTextBox.Password;
            string shift = ((ComboBoxItem)ShiftComboBox.SelectedItem)?.Content.ToString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Load recording settings
                    string recordingQuery = "SELECT * FROM recordingsettings WHERE id = 1";
                    MySqlCommand recordingCmd = new MySqlCommand(recordingQuery, connection);
                    MySqlDataReader recordingReader = recordingCmd.ExecuteReader();

                    if (recordingReader.Read())
                    {
                        int.TryParse(recordingReader["RecordingInt"].ToString(), out int value1);
                        GlobalSettings.RecordingInterval = value1;
                        int.TryParse(recordingReader["ImagePS"].ToString(), out int value2);
                        GlobalSettings.ImagesPerSet = value2;
                        int.TryParse(recordingReader["MaxRecordT"].ToString(), out int value3);
                        GlobalSettings.MaxRecordTime = value3 ;
                        GlobalSettings.Location = recordingReader["Location"].ToString();
                    }
                    recordingReader.Close();

                    // Load camera settings
                    string cameraQuery = "SELECT * FROM camerasettings";
                    MySqlCommand cameraCmd = new MySqlCommand(cameraQuery, connection);
                    MySqlDataReader cameraReader = cameraCmd.ExecuteReader();

                    GlobalSettings.Cameras.Clear();

                    while (cameraReader.Read())
                    {
                        GlobalSettings.Cameras.Add(new CameraSetting
                        {
                            IPAddress = cameraReader["IPAddress"].ToString(),
                            Protocol = cameraReader["Protocol"].ToString(),
                            Port = cameraReader["Port"].ToString(),        // Newly added Port
                            UserID = cameraReader["UserID"].ToString(),
                            Password = cameraReader["Password"].ToString(),
                            URL = cameraReader["URL"].ToString(),          // Newly added URL
                            id=cameraReader["id"].ToString()
                        });
                    }
                    cameraReader.Close();

                    // Load camera settings
                    string userQuery = "SELECT * FROM users WHERE UserID=@username AND Password=@password";
                    MySqlCommand userCmd = new MySqlCommand(userQuery, connection);                    
                    userCmd.Parameters.AddWithValue("@username", username);
                    userCmd.Parameters.AddWithValue("@password", password);
                    
                    MySqlDataReader userReader = userCmd.ExecuteReader();

                    while (userReader.Read())
                    {
                        //Users.UserID = userReader["UserID"].ToString();
                        Users.UserID = userReader["UserName"].ToString();
                        Users.Shift = shift;
                        ((App)Application.Current).GlobalUser = userReader["UserName"].ToString();
                        ((App)Application.Current).GlobalShift = shift;
                        ((App)Application.Current).GlobalUserType = userReader["UserType"].ToString();                        
                    }
                    userReader.Close();
                    string ftpQuery = "select * from ftp";
                    MySqlCommand ftpCmd = new MySqlCommand(ftpQuery, connection);
                    MySqlDataReader ftpReader = ftpCmd.ExecuteReader();
                    if (ftpReader.Read()) { 
                        ((App)Application.Current).Globalftpurl=ftpReader["url"].ToString();
                        ((App)Application.Current).Globalftpuid = ftpReader["username"].ToString();
                        ((App)Application.Current).Globalftppwd = ftpReader["password"].ToString();
                    }
                    ftpReader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void URLTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void LoadDbConfig()
        {
            String filePath = "dbconfig.txt";
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                int linecount = 1;
                foreach (string line in lines)
                {
                    if (linecount == 1)
                    {
                        GlobalSettings.DbHost = line;
                        this.URLTextBox.Text = line;
                    }
                    if (linecount == 2)
                    {
                        GlobalSettings.Dbusername = line;
                    }
                    if (linecount == 3)
                    {
                        GlobalSettings.Dbpassword = line;
                    }
                    linecount++;
                }
                connectionString = $"server={GlobalSettings.DbHost};database=cameradb;user={GlobalSettings.Dbusername};password={GlobalSettings.Dbpassword};";
                GlobalSettings.connectinstring = connectionString;
            }
            else
            {
                MessageBox.Show("DB Setting Not Found","DB Error",MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }

    public static class GlobalSettings
    {
        public static int RecordingInterval { get; set; }
        public static int ImagesPerSet { get; set; }
        public static int MaxRecordTime { get; set; }
        public static string Location { get; set; }
        public static string DbHost { get; set; }
        public static string Dbusername { get; set; }
        public static string Dbpassword { get; set; }
        public static string connectinstring { get; set; }

        public static List<CameraSetting> Cameras { get; set; } = new List<CameraSetting>();
    }

    public class CameraSetting
    {
        public string IPAddress { get; set; }
        public string Protocol { get; set; }
        public string Port { get; set; }      // Added Port
        public string UserID { get; set; }
        public string Password { get; set; }
        public string URL { get; set; }       // Added URL
        public string id { get; set; }
    }
    public static class Users
    {
        public static string UserID { get; set; }
        public static string Shift { get; set; }
    }
    
}
