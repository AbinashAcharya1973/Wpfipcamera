using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
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
using AForge.Video;
using AForge.Video.DirectShow;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using MySql.Data.MySqlClient;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace WpfCameraAForge
{
    /// <summary>
    /// Interaction logic for Insta360.xaml
    /// </summary>
    public partial class Insta360 : UserControl
    {
        private string connectionString = GlobalSettings.connectinstring;
        private FilterInfoCollection videoDevices; // To list available video devices (webcams)
        private LibVLC _libVLC;
        private LibVLCSharp.Shared.MediaPlayer _mediaPlayer;
        string selectedCameraName;
        int imagecount = 0;
        public Insta360()
        {
            InitializeComponent();
            LoadAvailableCameras();
        }
        private void LoadAvailableCameras()
        {
            // Get available video input devices (webcams)
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            // Add devices to the ComboBox for selection
            foreach (FilterInfo device in videoDevices)
            {
                cameraList.Items.Add(device.Name);
                
            }

            if (cameraList.Items.Count > 0)
            {
                cameraList.SelectedIndex = 0;
            }
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            string location = GlobalSettings.Location; // Make sure this exists in your settings
            string imageFolder;
            try
            {
                if (String.IsNullOrWhiteSpace(this.txtBarcode.Text))
                {
                    MessageBox.Show("Please Enter Barcode Before Starting the Recording", "Barcode Missing", MessageBoxButton.OK, MessageBoxImage.Error);

                }
                else
                {
                    System.Windows.Forms.OpenFileDialog newfileopen = new System.Windows.Forms.OpenFileDialog();
                    System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
                    {
                        Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                        Multiselect = true // Allow multiple files to be selected
                    };
                    var result = openFileDialog.ShowDialog();
                    foreach (var file in openFileDialog.FileNames)
                    {
                        imageFolder = System.IO.Path.Combine(location, "image/" + this.txtBarcode.Text + "_Insta360");

                        // Create directories if they don't exist


                        if (!Directory.Exists(imageFolder))
                        {
                            Directory.CreateDirectory(imageFolder);
                        }
                        string desitinationfilepath = imageFolder + "/" + System.IO.Path.GetFileName(file);
                        File.Copy(file, desitinationfilepath, true);
                        BitmapImage bitmap = new BitmapImage(new Uri(desitinationfilepath, UriKind.Absolute));
                        System.Windows.Controls.Image snapshotImage = new System.Windows.Controls.Image
                        {
                            Source = bitmap,
                            Width = 100,
                            Height = 100,
                            Margin = new Thickness(5)
                        };
                        InsertRecordingInfo(desitinationfilepath, "image", this.txtBarcode.Text, "Insta360");
                        this.SnapshotWrapPanel.Children.Add(snapshotImage);
                        //MessageBox.Show(desitinationfilepath, "File", MessageBoxButton.OK);
                    }
                    MessageBox.Show("Finished","Image Upload",MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message,"Error File Copy",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }
        private void InsertRecordingInfo(string filePath, string fileType, string barcode, string camno)
        {
            try
            {
                // Assuming you have a MySQL connection and relevant settings loaded
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO recordings (RDateTime, RPath, FileType, UserID, Shift,barcode,camera) VALUES (@dateTime, @filePath, @fileType, @userID, @shift,@barcode,@camno)";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@dateTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("@filePath", filePath);
                    cmd.Parameters.AddWithValue("@fileType", fileType);
                    cmd.Parameters.AddWithValue("@userID", Users.UserID);  // Assuming you have a current user ID stored
                    cmd.Parameters.AddWithValue("@shift", Users.Shift);    // Assuming you have a shift info stored
                    cmd.Parameters.AddWithValue("@barcode", barcode);
                    cmd.Parameters.AddWithValue("@camno", camno);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inserting recording info: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cameraList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            

            // Select the chosen camera
            selectedCameraName = cameraList.SelectedItem as string;
            
        }
       
        // Convert a Bitmap to ImageSource for WPF
    
        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedCameraName))
            {
                MessageBox.Show("Please select a webcam!");
                return;
            }

            // Initialize LibVLC
            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            this.WebcamImage1.MediaPlayer= _mediaPlayer;
            // Start capturing the selected webcam
            StartWebcamCapture(selectedCameraName);

        }
        private void StartWebcamCapture(string webcamName)
        {
            // Assuming the selected camera has a DirectShow-based URL
            string webcamUrl = $"dshow:// :dshow-vdev=\"{webcamName}\" :live-caching=100";

            /*using (var media = new Media(_libVLC, webcamUrl, FromType.FromLocation))
            {
                // Set media options for DirectShow devices
                media.AddOption(":input-slave=none");
                media.AddOption(":sout=#transcode{vcodec=h264,vb=800,scale=1}:file{dst=webcam.mp4}");

                _mediaPlayer.Play(media);
            }*/
            var media = new Media(_libVLC, webcamUrl, FromType.FromLocation);
            media.AddOption(":input-slave=none");
            //media.AddOption(":sout=#transcode{vcodec=h264,vb=800,scale=1}:file{dst=webcam.mp4}");
            _mediaPlayer.Play(media);

        }
        private void TakeSnapshot_Click(object sender, RoutedEventArgs e)
        {
            string location = GlobalSettings.Location; // Make sure this exists in your settings
            string imageFolder;
            try
            {

                if (String.IsNullOrWhiteSpace(this.txtBarcode.Text))
                {
                    MessageBox.Show("Please Enter Barcode Before Starting the Recording", "Barcode Missing", MessageBoxButton.OK, MessageBoxImage.Error);

                }
                else
                {
                    imageFolder = System.IO.Path.Combine(location, "image/" + this.txtBarcode.Text + "_Insta360");
                    if (!Directory.Exists(imageFolder))
                    {
                        Directory.CreateDirectory(imageFolder);
                    }
                    uint width = 1920;  // You can adjust this as needed
                    uint height = 1080; // You can adjust this as needed
                    imagecount++;
                    string imagePath1 = System.IO.Path.Combine(imageFolder, imagecount + $".png");
                    bool result = _mediaPlayer.TakeSnapshot(0, imagePath1, width, height);
                    if (!result)
                    {
                        MessageBox.Show("Failed to take snapshot." + imagecount + "Camera1", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        InsertRecordingInfo(imagePath1, "image", this.txtBarcode.Text, "Insta360");
                        BitmapImage bitmap = new BitmapImage(new Uri(imagePath1, UriKind.Absolute));
                        System.Windows.Controls.Image snapshotImage = new System.Windows.Controls.Image
                        {
                            Source = bitmap,
                            Width = 100,
                            Height = 100,
                            Margin = new Thickness(5)
                        };
                        SnapshotWrapPanel.Children.Add(snapshotImage);

                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"Error inserting recording info: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _mediaPlayer?.Stop();
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
        }
    }
}
