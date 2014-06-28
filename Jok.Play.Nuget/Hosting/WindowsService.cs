using System;

namespace Jok.Play.Hosting
{
    public partial class WindowsService : System.ServiceProcess.ServiceBase
    {
        public Action OnStartAction { get; set; }
        public Action OnStopAction { get; set; }

        public WindowsService()
        {
            InitializeComponent();
        }


        protected override void OnStart(string[] args)
        {
            if (OnStartAction != null)
                OnStartAction();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            EventLog.WriteEntry(e.ToString(), System.Diagnostics.EventLogEntryType.Error);
            Console.WriteLine(e);
        }

        protected override void OnStop()
        {
            if (OnStopAction != null)
                OnStopAction();
        }

        public static void Run(string Name, Action OnStart)
        {
            System.ServiceProcess.ServiceBase.Run(new WindowsService { ServiceName = Name, OnStartAction = OnStart });
        }

    }
}
