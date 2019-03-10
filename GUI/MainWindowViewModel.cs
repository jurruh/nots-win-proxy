using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GUI
{
    class MainWindowViewModel : INotifyPropertyChanged
    {

        Proxy.Settings settings;

        public bool CachingEnabled {
            get { return settings.CachingEnabled; }
            set { settings.CachingEnabled = value; NotifyPropertyChanged("CachingEnabled"); }
        }

        public bool AuthenticationEnabled
        {
            get { return settings.AuthenticationEnabled; }
            set { settings.AuthenticationEnabled = value; NotifyPropertyChanged("AuthenticationEnabled"); }
        }

        public bool PrivacyModusEnabled
        {
            get { return settings.PrivacyModusEnabled; }
            set { settings.PrivacyModusEnabled = value; NotifyPropertyChanged("PrivacyModusEnabled"); }
        }

        public bool AdBlockerEnabled
        {
            get { return settings.AdBlockerEnabled; }
            set { settings.AdBlockerEnabled = value; NotifyPropertyChanged("AdBlockerEnabled"); }
        }

        public int BufferSize {
            get { return settings.BufferSize; }
            set { settings.BufferSize = value; NotifyPropertyChanged("BufferSize"); }
        }

        public int Port
        {
            get { return settings.Port; }
            set { settings.Port = value; NotifyPropertyChanged("Port"); }
        }

        public ICommand StartProxy { get; set; }

        public MainWindowViewModel()
        {
            settings = new Proxy.Settings();

            StartProxy = new RelayCommand((obj) => {
                MessageBox.Show("Hoi");
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
    
        protected void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

    }
}
