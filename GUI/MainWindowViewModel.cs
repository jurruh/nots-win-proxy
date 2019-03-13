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
            set { this.proxyStopped = value; NotifyPropertyChanged("ProxyStopped"); NotifyPropertyChanged("ProxyStarted"); }
        }

        public Boolean ProxyStarted {
            get { return !proxyStopped;  }
        }

        private Boolean logIncomingRequests = true;
        public Boolean LogIncomingRequests
        {
            get { return logIncomingRequests; }
            set { this.logIncomingRequests = value; NotifyPropertyChanged("LogIncomingRequests"); }
        }

        private Boolean logOutgoingRequests = true;
        public Boolean LogOutgoingRequests
        {
            get { return logOutgoingRequests; }
            set { this.logOutgoingRequests = value; NotifyPropertyChanged("LogOutgoingRequests"); }
        }

        private Boolean logIncomingResponses = true;
        public Boolean LogIncomingResponses
        {
            get { return logIncomingResponses; }
            set { this.logIncomingResponses = value; NotifyPropertyChanged("LogIncomingResponses"); }
        }

        private Boolean logOutgoingResponses = true;
        public Boolean LogOutgoingResponses
        {
            get { return logOutgoingResponses; }
            set { this.logOutgoingResponses = value; NotifyPropertyChanged("LogOutgoingResponses"); }
        }


        public ObservableCollection<string> messageLog = new ObservableCollection<string>() { };
        public ObservableCollection<string> MessageLog { 
            get { return messageLog; }
            set {
                this.messageLog = value; NotifyPropertyChanged("MessageLog");
            }
        }

        public ICommand StartProxy { get; set; }

        public ICommand StopProxy { get; set; }

        private Proxy.Proxy proxy;

        public MainWindowViewModel()
        {
            settings = new Proxy.Settings();

            MessageLog.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args) => {
                this.NotifyPropertyChanged("MessageLog");
            };

            StartProxy = new RelayCommand(async (obj) =>
            {
                proxy = new Proxy.Proxy(settings);

                proxy.RequestReceivedFromClient += (sender, args) => { if (LogIncomingRequests) { Log(args.Request.ToString()); } };
                proxy.RequestSendToExternalServer += (sender, args) => { if (LogOutgoingRequests) { Log(args.Request.ToString()); } };
                proxy.ResponseFromExternalServer += (sender, args) => { if (LogIncomingResponses) { Log(Encoding.ASCII.GetString(args.Response.GetHeaderBytes())); } };
                proxy.ResponseSendToClient += (sender, args) => { if (LogOutgoingResponses) { Log(Encoding.ASCII.GetString(args.Response.GetHeaderBytes())); } };

                try
                {
                    proxy.Start();
                    Log("Proxy started");
                    this.ProxyStopped = false;
                }
                catch (Exception e) {
                    Log("Proxy cannot be started is there already a process running on this port?");
                    this.ProxyStopped = true;
                    return;
                }


            });

            StopProxy = new RelayCommand((obj) =>
            {
                this.ProxyStopped = true;
                proxy.Stop();
                Log("Proxy Stopped");
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
