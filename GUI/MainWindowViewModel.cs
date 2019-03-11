using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private Boolean proxyStopped = true;
        public Boolean ProxyStopped
        {
            get { return proxyStopped; }
            set { this.proxyStopped = value; NotifyPropertyChanged("ProxyStopped"); }
        }

        public ObservableCollection<string> messageLog = new ObservableCollection<string>() { "hehe", "haha" };
        public ObservableCollection<string> MessageLog { 
            get { return messageLog; }
            set {
                this.messageLog = value; NotifyPropertyChanged("MessageLog");
            }
        }

        public ICommand StartProxy { get; set; }

        private Proxy.Proxy proxy;

        public MainWindowViewModel()
        {
            settings = new Proxy.Settings();

            MessageLog.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args) => {
                this.NotifyPropertyChanged("MessageLog");
            };

            StartProxy = new RelayCommand((obj) =>
            {
                proxy = new Proxy.Proxy(settings);

                proxy.RequestReceivedFromClient += (sender, args) => Log(args.Request.ToString());
                proxy.RequestSendToExternalServer += (sender, args) => Log(args.Request.ToString());
                proxy.ResponseFromExternalServer += (sender, args) => Log(Encoding.ASCII.GetString(args.Response.GetBytes()));
                proxy.ResponseSendToClient += (sender, args) => Log(Encoding.ASCII.GetString(args.Response.GetBytes()));

                proxy.Start();
                MessageLog.Add("Proxy started");
                this.ProxyStopped = false;
            });
        }

        public void Log(string log) {
            App.Current.Dispatcher.Invoke(() => {
                MessageLog.Add(log);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
    
        protected void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

    }
}
