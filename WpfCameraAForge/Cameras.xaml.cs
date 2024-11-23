using LibVLCSharp.Shared;
using MySql.Data.MySqlClient;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using System.Windows.Media.Imaging;
using LibVLCSharp.WPF;
using System.Windows.Media;

namespace WpfCameraAForge
{
    public partial class Cameras : UserControl
    {
        private LibVLC _libVLC;
        private LibVLCSharp.Shared.MediaPlayer _mediaPlayer2;
        private LibVLCSharp.Shared.MediaPlayer _mediaPlayer3;
        private LibVLCSharp.Shared.MediaPlayer _mediaPlayer4;
        private Media _media2;
        private Media _media3;
        private Media _media4;
        string videoFilePath1;
        string videoFilePath2;
        string videoFilePath3;
        private System.Timers.Timer _recordingtimer;
        private System.Timers.Timer _snaptimer;
        private int imagecount;
        private string connectionString = GlobalSettings.connectinstring;
        String barcode;
        string imageFolder;
        private LibVLCSharp.Shared.MediaPlayer Instaplyer3;
        private LibVLCSharp.Shared.MediaPlayer thumbnailplyer3;


        public Cameras()
        {
            InitializeComponent();
            Core.Initialize();  // Initialize LibVLC

            // Create the LibVLC instance
            _libVLC = new LibVLC();
            Instaplyer3 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            // Initialize media players for the video controls
            _mediaPlayer2 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            VlcControl2.MediaPlayer = _mediaPlayer2;

            _mediaPlayer3 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            VlcControl3.MediaPlayer = _mediaPlayer3;

            _mediaPlayer4 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            VlcControl4.MediaPlayer = _mediaPlayer4;
        }

        public void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Retrieve the camera settings from GlobalSettings
                var camera1 = GlobalSettings.Cameras.FirstOrDefault();
                var camera2 = GlobalSettings.Cameras.Skip(1).FirstOrDefault();
                var camera3 = GlobalSettings.Cameras.Skip(2).FirstOrDefault();

                string url1 = $"{camera1.URL}";
                string url2 = $"{camera2.URL}";
                string url3 = $"{camera3.URL}";

                // Check if the camera settings are available
                if (camera1 != null)
                {
                    _mediaPlayer2.Play(new Media(_libVLC, url1, FromType.FromLocation));
                }
                else {
                    MessageBox.Show("camera 1 is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (camera2 != null)
                {
                    _mediaPlayer3.Play(new Media(_libVLC, url2, FromType.FromLocation));
                }
                else
                {
                    MessageBox.Show("camera 2 is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (camera3 != null)
                {
                    _mediaPlayer4.Play(new Media(_libVLC, url3, FromType.FromLocation));
                }
                else
                {
                    MessageBox.Show("camera 3 is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                // Construct URLs using the camera settings
                /*string url1 = $"{camera1.Protocol}://{camera1.UserID}:{camera1.Password}@{camera1.IPAddress}:{camera1.Port}";
                string url2 = $"{camera2.Protocol}://{camera2.UserID}:{camera2.Password}@{camera2.IPAddress}:{camera2.Port}";
                string url3 = $"{camera3.Protocol}://{camera3.UserID}:{camera3.Password}@{camera3.IPAddress}:{camera3.Port}";*/


                // Play streams in respective VideoViews


                

                //MessageBox.Show("Video streams started successfully!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                this.recordStatus.Content = "Cameras Connected";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting video streams: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopVideoStream();
        }

        private void StopVideoStream()
        {
            try
            {
                // Stop all video streams
                if (_mediaPlayer2.IsPlaying) _mediaPlayer2.Stop();
                if (_mediaPlayer3.IsPlaying) _mediaPlayer3.Stop();
                if (_mediaPlayer4.IsPlaying) _mediaPlayer4.Stop();

                MessageBox.Show("Video streams stopped successfully!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                this.recordStatus.Content = "Not Connected";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping video streams: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Clean up the media player and libVLC instances when the control is unloaded
            _mediaPlayer2?.Dispose();
            _mediaPlayer3?.Dispose();
            _mediaPlayer4?.Dispose();
            _libVLC?.Dispose();
        }
        private void VideoRecording()
        {
            
            int videoduration = GlobalSettings.MaxRecordTime;
            int durationInmilisecond = videoduration * 1000;
            _recordingtimer = new System.Timers.Timer();
            _recordingtimer.Interval = durationInmilisecond;
            _recordingtimer.AutoReset = false;
            _recordingtimer.Elapsed += _recordingtimer_Elapsed;
            string location = GlobalSettings.Location; // Make sure this exists in your settings
            string videoFolder = Path.Combine(location, "video/"+barcode);

            try
            {
                if (!Directory.Exists(videoFolder))
                {
                    Directory.CreateDirectory(videoFolder);
                }
                // Construct output file paths for videos
                videoFilePath1 = Path.Combine(videoFolder, $"Camera1_{barcode}.mp4");
                videoFilePath2 = Path.Combine(videoFolder, $"Camera2_{barcode}.mp4");
                videoFilePath3 = Path.Combine(videoFolder, $"Camera3_{barcode}.mp4");

                // Retrieve camera settings from GlobalSettings
                var camera1 = GlobalSettings.Cameras[0];
                var camera2 = GlobalSettings.Cameras[1];
                var camera3 = GlobalSettings.Cameras[2];

                if (camera1 == null || camera2 == null || camera3 == null)
                {
                    MessageBox.Show("Camera settings are missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Construct RTSP URLs for recording
                string rtspUrl1 = camera1.URL;
                string rtspUrl2 = camera2.URL;
                string rtspUrl3 = camera3.URL;

                // Create media for each camera
                _media2 = new Media(_libVLC, rtspUrl1, FromType.FromLocation);
                _media3 = new Media(_libVLC, rtspUrl2, FromType.FromLocation);
                _media4 = new Media(_libVLC, rtspUrl3, FromType.FromLocation);


                // Add media options for recording camera 1
                _media2.AddOption($":sout=#file{{dst='{videoFilePath1}'}}");
                _media2.AddOption(":sout-keep");

                // Add media options for recording camera 2
                _media3.AddOption($":sout=#file{{dst='{videoFilePath2}'}}");
                _media3.AddOption(":sout-keep");

                // Add media options for recording camera 3
                _media4.AddOption($":sout=#file{{dst='{videoFilePath3}'}}");
                _media4.AddOption(":sout-keep");

                // Start recording each camera's stream
                _mediaPlayer2.Play(_media2);
                _mediaPlayer3.Play(_media3);
                _mediaPlayer4.Play(_media4);

                // Insert record information into the database
                InsertRecordingInfo(videoFilePath1, "video", barcode, "Camera1");
                InsertRecordingInfo(videoFilePath2, "video", barcode, "Camera2");
                InsertRecordingInfo(videoFilePath3, "video", barcode, "Camera3");

                _recordingtimer.Start();

                //MessageBox.Show("Recording started successfully!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                this.recordStatus.Content = "Video Recording is in Progress";
            }
            catch (Exception ex) {

                MessageBox.Show($"Error starting recording: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            SnapshotWrapPanel.Children.Clear();

            if (String.IsNullOrWhiteSpace(this.txtBarcode.Text))
            {
                MessageBox.Show("Please Enter Barcode Before Starting the Recording","Barcode Missing",MessageBoxButton.OK,MessageBoxImage.Error);
                
            }
            else
            {
                
                int snapinterval = GlobalSettings.RecordingInterval;
                
                int snapintervalInmilisecond=snapinterval * 1000;
                
                _snaptimer = new System.Timers.Timer();
                
                _snaptimer.Interval = snapintervalInmilisecond;
                
                _snaptimer.AutoReset= true;
                
                _snaptimer.Elapsed += _snaptimer_Elapsed;
                
                barcode = this.txtBarcode.Text;
                
                try
                {
                    MessageBoxResult ans=MessageBox.Show($"Dp you Want to Start the Recording?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (ans == MessageBoxResult.Yes)
                    {
                        // Retrieve the recording location from GlobalSettings (assumed to be loaded already)
                        string location = GlobalSettings.Location; // Make sure this exists in your settings

                        imageFolder = Path.Combine(location, "image/"+barcode);

                        // Create directories if they don't exist


                        if (!Directory.Exists(imageFolder))
                        {
                            Directory.CreateDirectory(imageFolder);
                        }
                        this.recordStatus.Content = "Still Image Capturing is in Progress";
                        imagecount = 1;
                        _snaptimer.Start();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error starting recording: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
        }

        private void _snaptimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => TakeSnapshot("lkl"));
        }

        private void _recordingtimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => StopRecording());
        }
        private void StopRecording()
        {
            var Videothumnail1 = new VideoView();
            var Videothumnail2 = new VideoView();
            var Videothumnail3 = new VideoView();
            try {
                if (_mediaPlayer2.IsPlaying)
                {
                    _mediaPlayer2.Stop();
                    MessageBox.Show("Recording Finished.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    /*Videothumnail1.Height = 200;Videothumnail1.Width = 300;
                    SnapshotWrapPanel.Children.Add(Videothumnail1);*/
                    var thumbnailplyer1 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
                    WebcamImage1.MediaPlayer = thumbnailplyer1;
                    
                    thumbnailplyer1.Play(new Media(_libVLC, videoFilePath1, FromType.FromPath));
                    this.recordStatus.Content = "A Recording Session has been Finished for Barcode"+this.txtBarcode.Text;
                }
                if (_mediaPlayer3.IsPlaying)
                {
                    _mediaPlayer3.Stop();
                    /*Videothumnail2.Height = 200; Videothumnail2.Width = 300;
                    SnapshotWrapPanel.Children.Add(Videothumnail2);*/
                    var thumbnailplyer2 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
                    WebcamImage2.MediaPlayer = thumbnailplyer2;
                    
                    thumbnailplyer2.Play(new Media(_libVLC, videoFilePath2, FromType.FromPath));
                    //MessageBox.Show("Recording stopped.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                if (_mediaPlayer4.IsPlaying)
                {
                    _mediaPlayer4.Stop();
                    /*Videothumnail3.Height = 200; Videothumnail3.Width = 300;
                    SnapshotWrapPanel.Children.Add(Videothumnail3);*/
                    thumbnailplyer3 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
                    WebcamImage3.MediaPlayer = thumbnailplyer3;
                    
                    thumbnailplyer3.Play(new Media(_libVLC, videoFilePath3, FromType.FromPath));
                    //MessageBox.Show("Recording stopped.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Error stopping recording: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TakeSnapshot(string imagePath)
        {
            try
            {
                // TakeSnapshot requires a media player instance number, file path, width, and height.
                // Assuming _mediaPlayer2 is the MediaPlayer instance for the stream, and we use full video resolution
                
                if (imagecount <= GlobalSettings.ImagesPerSet)
                {
                    imagecount++;
                    uint width = 1920;  // You can adjust this as needed
                    uint height = 1080; // You can adjust this as needed
                    /*string imagePath1 = Path.Combine(imageFolder, $"Camera1_{barcode}_{imagecount}.png");
                    string imagePath2 = Path.Combine(imageFolder, $"Camera2_{barcode}_{imagecount}.png");
                    string imagePath3 = Path.Combine(imageFolder, $"Camera3_{barcode}_{imagecount}.png");*/
                    string imagePath1 = Path.Combine(imageFolder, $"Camera1_{barcode}_{imagecount}.jpg");
                    string imagePath2 = Path.Combine(imageFolder, $"Camera2_{barcode}_{imagecount}.jpg");
                    string imagePath3 = Path.Combine(imageFolder, $"Camera3_{barcode}_{imagecount}.jpg");
                    bool result = _mediaPlayer2.TakeSnapshot(0, imagePath1, width, height);
                    bool result1 = _mediaPlayer3.TakeSnapshot(0, imagePath2, width, height);
                    bool result2 = _mediaPlayer4.TakeSnapshot(0, imagePath3, width, height);
                 
                    this.recordStatus.Content = "Still Image Capturing is in Progress";
                    if (!result)
                    {
                        MessageBox.Show("Failed to take snapshot."+imagecount+"Camera1", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        InsertRecordingInfo(imagePath1, "image", barcode, "Camera1");
                        BitmapImage bitmap = new BitmapImage(new Uri(imagePath1, UriKind.Absolute));
                        Image snapshotImage = new Image
                        {
                            Source = bitmap,
                            Width = 100,
                            Height = 100,
                            Margin = new Thickness(5)
                        };
                        SnapshotWrapPanel.Children.Add(snapshotImage);

                    }
                    if (!result1)
                    {
                        MessageBox.Show("Failed to take snapshot." + imagecount + "Camera2", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        InsertRecordingInfo(imagePath2, "image", barcode, "Camera2");
                        BitmapImage bitmap = new BitmapImage(new Uri(imagePath2, UriKind.Absolute));
                        Image snapshotImage = new Image
                        {
                            Source = bitmap,
                            Width = 100,
                            Height = 100,
                            Margin = new Thickness(5)
                        };
                        SnapshotWrapPanel.Children.Add(snapshotImage);
                    }
                    if (!result2)
                    {
                        MessageBox.Show("Failed to take snapshot." + imagecount + "Camera3", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        InsertRecordingInfo(imagePath3, "image", barcode, "Camera3");
                        BitmapImage bitmap = new BitmapImage(new Uri(imagePath3, UriKind.Absolute));
                        Image snapshotImage = new Image
                        {
                            Source = bitmap,
                            Width = 100,
                            Height = 100,
                            Margin = new Thickness(5)
                        };
                        SnapshotWrapPanel.Children.Add(snapshotImage);
                    }

                }
                else
                {
                    _snaptimer.Stop();
                    MessageBox.Show("Snapshot saved successfully! And Video Recording Will Start", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    VideoRecording();
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error capturing image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InsertRecordingInfo(string filePath, string fileType,string barcode,string camno)
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

        private void StopRecordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_mediaPlayer2.IsPlaying)
                {
                    _mediaPlayer2.Stop();
                    MessageBox.Show("Recording Finished.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.recordStatus.Content = "A Recording Session has been Finished";
                }
                if (_mediaPlayer3.IsPlaying)
                {
                    _mediaPlayer3.Stop();
                    //MessageBox.Show("Recording stopped.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                if (_mediaPlayer4.IsPlaying)
                {
                    _mediaPlayer4.Stop();
                    //MessageBox.Show("Recording stopped.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping recording: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btninstaupload_Click(object sender, RoutedEventArgs e)
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
                        Filter = "Image files (*.mp4)|*.mp4;",
                        Multiselect = true // Allow multiple files to be selected
                    };
                    var result = openFileDialog.ShowDialog();
                    foreach (var file in openFileDialog.FileNames)
                    {
                        imageFolder = System.IO.Path.Combine(location, "video/" + this.txtBarcode.Text + "_Insta360");

                        // Create directories if they don't exist


                        if (!Directory.Exists(imageFolder))
                        {
                            Directory.CreateDirectory(imageFolder);
                        }
                        string desitinationfilepath = imageFolder + "/" + System.IO.Path.GetFileName(file);
                        File.Copy(file, desitinationfilepath, true);
                        var fixedImagePath = "pack://application:,,,/Images/video.png";
                        BitmapImage bitmap = new BitmapImage(new Uri(fixedImagePath, UriKind.Absolute));
                        System.Windows.Controls.Image snapshotImage = new System.Windows.Controls.Image
                        {
                            Source = bitmap,
                            Width = 100,
                            Height = 100,
                            Margin = new Thickness(5),
                            Cursor = System.Windows.Input.Cursors.Hand
                        };
                        snapshotImage.MouseLeftButtonUp += (s, ev) =>
                        {
                            PlayVideo(desitinationfilepath);  // Play video when thumbnail is clicked
                        };
                        InsertRecordingInfo(desitinationfilepath, "video", this.txtBarcode.Text, "Insta360");
                        this.SnapshotWrapPanel.Children.Add(snapshotImage);
                        //MessageBox.Show(desitinationfilepath, "File", MessageBoxButton.OK);
                    }
                    MessageBox.Show("Finished", "Video Upload", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error File Copy", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void PlayVideo(string videoPath)
        {
            // Play the selected video in the VideoView
            if(thumbnailplyer3.IsPlaying)
                { thumbnailplyer3.Stop(); }
            if(Instaplyer3.IsPlaying)
                Instaplyer3.Stop();
            WebcamImage3.MediaPlayer = Instaplyer3;
            Instaplyer3.Play(new Media(_libVLC, new Uri(videoPath)));
        }
    }
}