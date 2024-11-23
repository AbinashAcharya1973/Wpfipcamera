using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfCameraAForge
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public String GlobalUser {  get; set; }
        public String GlobalUserType { get; set; }
        public String GlobalShift { get; set; }
        public String Globalftpurl { get; set; }
        public String Globalftpuid { get; set; }
        public String Globalftppwd { get; set; }

    }
}
