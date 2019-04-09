using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GUI
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        LoadBalancer.Configuration settings;

        public int BufferSize {
            get { return settings.BufferSize; }
            set { settings.BufferSize = value; NotifyPropertyChanged("BufferSize"); }
        }

        public int Port
        {
            get { return settings.Port; }
            set { settings.Port = value; NotifyPropertyChanged("Port"); }
        }

        public int MaxTimeout
        {
            get { return settings.MaxTimeout; }
            set { settings.MaxTimeout = value; NotifyPropertyChanged("MaxTimeout"); }
        }

        private string newServerEndpoint = "localhost";
        public string NewServerEndpoint
        {
            get { return newServerEndpoint; }
            set { newServerEndpoint = value; NotifyPropertyChanged("NewServerEndpoint"); }
        }

        private System.Type selectedAlgo;
        public System.Type SelectedAlgo
        {
            get { return selectedAlgo; }
            set { selectedAlgo = value; NotifyPropertyChanged("SelectedAlgo"); }
        }

        public int HealthCheckInterval
        {
            get { return settings.HealthCheckInterval; }
            set { settings.HealthCheckInterval = value; NotifyPropertyChanged("HealthCheckInterval"); }
        }

        private int newServerPort = 8081;
        public int NewServerPort
        {
            get { return newServerPort; }
            set { newServerPort = value; NotifyPropertyChanged("NewServerPort"); }
        }

        public ObservableCollection<LoadBalancer.Server> servers = new ObservableCollection<LoadBalancer.Server>() { };
        public ObservableCollection<LoadBalancer.Server> Servers
        {
            get { return servers; }
            set
            {
                this.servers = value; NotifyPropertyChanged("Servers");
            }
        }

        public ObservableCollection<System.Type> algos = new ObservableCollection<System.Type>() { };
        public ObservableCollection<System.Type> Algos
        {
            get { return algos; }
            set
            {
                this.algos = value; NotifyPropertyChanged("Algos");
            }
        }

        private Boolean loadBalancerStopped = true;
        public Boolean LoadBalancerStopped
        {
            get { return loadBalancerStopped; }
            set { this.loadBalancerStopped = value; NotifyPropertyChanged("LoadBalancerStopped"); NotifyPropertyChanged("LoadBalancerStarted"); }
        }

        public Boolean LoadBalancerStarted {
            get { return !loadBalancerStopped;  }
        }

        public ObservableCollection<string> messageLog = new ObservableCollection<string>() { };
        public ObservableCollection<string> MessageLog { 
            get { return messageLog; }
            set {
                this.messageLog = value; NotifyPropertyChanged("MessageLog");
            }
        }

        public ICommand StartLoadBalancer { get; set; }

        public ICommand AddServer { get; set; }

        public ICommand StopLoadBalancer { get; set; }

        public ICommand ClearLog { get; set; }

        public ICommand ClearServers { get; set; }


        private LoadBalancer.LoadBalancer loadBalancer;

        public MainWindowViewModel()
        {
            settings = new LoadBalancer.Configuration();

            InitAlgos();

            StartLoadBalancer = new RelayCommand(async (obj) =>
            {
                if (settings.Port < 1 || settings.Port > 65535) {
                    Log("Invalid port number");
                    return;
                }

                if (Servers.Count() == 0) {
                    Log("Please add some servers");
                    return;
                }

                settings.Servers = Servers.ToList();
                settings.LoadBalancerAlgo = (LoadBalancer.ILoadBalancerAlgo)Activator.CreateInstance(SelectedAlgo);

                loadBalancer = new LoadBalancer.LoadBalancer(settings);

                loadBalancer.OnLog += (sender, evt) =>
                {
                    this.Log(evt.Message);
                };

                try
                {
                    await loadBalancer.Start();
                    Log("Loadbalancer started");
                    this.LoadBalancerStopped = false;
                }
                catch (Exception e) {
                    Log("Loadbalancer cannot be started is there already a process running on this port?");
                    this.LoadBalancerStopped = true;
                    return;
                }
            });

            ClearServers = new RelayCommand((obj) =>
            {
                this.servers.Clear();
            });

            StopLoadBalancer = new RelayCommand((obj) =>
            {
                this.LoadBalancerStopped = true;
                loadBalancer.Stop();
                Log("LoadBalancer Stopped");
            });

            AddServer = new RelayCommand((obj) =>
            {
                if (newServerPort < 1 || newServerPort > 65535)
                {
                    Log("Invalid port number");
                    return;
                }

                if (newServerEndpoint.Trim().Length == 0) {
                    Log("No endpoint provided");
                    return;
                }

                this.Servers.Add(new LoadBalancer.Server() { Port = newServerPort, Endpoint = newServerEndpoint });
            });

            ClearLog = new RelayCommand((obj) => {
                MessageLog.Clear();
            });
        }

        public void Log(string log) {
            App.Current.Dispatcher.Invoke(() => {
                MessageLog.Add($"{DateTime.Now}: {log}");
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void InitAlgos() {
            var type = typeof(LoadBalancer.ILoadBalancerAlgo);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.FullName != "LoadBalancer.ILoadBalancerAlgo");

            SelectedAlgo = types.FirstOrDefault(t => t.FullName.Contains("RoundRobin"));

            foreach(var t in types){
                Algos.Add(t);
            }
        }

        protected void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

    }
}
