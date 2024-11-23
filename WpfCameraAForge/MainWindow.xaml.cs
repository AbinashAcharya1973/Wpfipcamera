using System;
using System.Collections.Generic;
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

namespace WpfCameraAForge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isSidebarVisible = true;
        public MainWindow()
        {
            InitializeComponent();
            this.txtShift.Text = "Shift         : " + ((App)Application.Current).GlobalShift;
            this.txtUserID.Text = "User         : " + ((App)Application.Current).GlobalUser;
            this.txtUserType.Text = "User Type : " + ((App)Application.Current).GlobalUserType;
        }

        private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
        {
            if (isSidebarVisible)
            {
                SidebarColumn.Width = new GridLength(37);
            }
            else
            {
                SidebarColumn.Width = new GridLength(200);
            }
            isSidebarVisible = !isSidebarVisible;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Children.Clear();
            Cameras CameraControl=new Cameras();
            MainContent.Children.Add(CameraControl);
            CameraControl.StartButton_Click(this, new RoutedEventArgs());
            //CameraRecording();
        }

        public void CameraRecording()
        {
            // Assuming you have a Cameras control defined in your XAML with x:Name="camerasControl"
            camerasControl.StartButton_Click(this, new RoutedEventArgs());
        }

        private void CSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            String username = ((App)Application.Current).GlobalUser;
            
            if (username == "admin")
            {
                MainContent.Children.Clear();
                CameraSettings cameraSettings = new CameraSettings();
                MainContent.Children.Add(cameraSettings);
            }
            else
            {
                MessageBox.Show("Not Authorised","Confirmation",MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RSearchButton_Click(object sender, RoutedEventArgs e)
        {
            String username = ((App)Application.Current).GlobalUser;
            if (username == "admin")
            {
                MainContent.Children.Clear();
                VideoSearch videoSearch = new VideoSearch();
                MainContent.Children.Add(videoSearch);
            }
            else
            {
                MessageBox.Show("Not Authorised", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            CameraStop();
        }
        public void CameraStop()
        {
            //camerasControl.StopButton_Click(this, new RoutedEventArgs());
        }

        
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Insta360Button_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Children.Clear();
            Insta360 insta360 = new Insta360();
            MainContent.Children.Add(insta360);
        }

        private void PublishButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Children.Clear();
            DBManage DBManage = new DBManage();
            MainContent.Children.Add(DBManage);
        }
    }
}