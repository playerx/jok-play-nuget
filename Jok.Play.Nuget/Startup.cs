using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

[assembly: OwinStartup(typeof(Jok.Play.Startup))]

namespace Jok.Play
{
    public class Startup
    {
        public static void Configure(string applicationName, string serviceDisplayName, string serviceDescription, Func<int> getConnectionsCount, Func<List<IGameTable>> getTables)
        {
            ApplicationName = applicationName;
            ServiceDisplayName = serviceDisplayName;
            ServiceDescription = serviceDescription;

            GetConnectionsCount = getConnectionsCount;
            GetTables = getTables;
        }


        public static string ApplicationName { get; internal set; }
        public static string ServiceDisplayName { get; internal set; }
        public static string ServiceDescription { get; internal set; }

        internal static Func<int> GetConnectionsCount { get; private set; }
        internal static Func<List<IGameTable>> GetTables { get; private set; }

        internal static DateTime StartDate = DateTime.Now;
        public static Action<IAppBuilder> ConfigureApp;


        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();


            // Configure Web API for self-host. 
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "StatsApi",
                routeTemplate: "{action}",
                defaults: new { controller = "Info", action = "Index", id = RouteParameter.Optional }
            );

            app.UseWebApi(config);

            if (ConfigureApp != null)
                ConfigureApp(app);
        }



        public static void Start(bool isConsole, string url)
        {
            ServicePointManager.DefaultConnectionLimit = 1000;

            // Start
            WebApp.Start(url);
            Console.WriteLine("Server running on {0}", url);


            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (!isConsole) return;


            // Disable exit
            DisableCloseButton();


            // Commandline
            var command = String.Empty;
            do
            {
                command = Console.ReadLine();

                if (command == "stats")
                    Console.WriteLine("Connections: {0}", GetConnectionsCount());

            }
            while (command != "exit");
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
            EventLog.WriteEntry(ApplicationName, e.ExceptionObject.ToString(), System.Diagnostics.EventLogEntryType.Error);
        }


        #region Helper
        private const int MF_BYCOMMAND = 0x00000000;
        private const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        internal static void DisableCloseButton()
        {
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);
        }
        #endregion
    }
}
