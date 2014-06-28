using Jok.Play;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;

namespace Jok.Play.Hosting
{
    [RunInstaller(true)]
    public partial class WindowsServiceInstallerBase : Installer
    {
        public WindowsServiceInstallerBase()
        {
            var windowsServiceInstaller = new ServiceInstaller
            {
                Description = Startup.ServiceDescription,
                DisplayName = Startup.ServiceDisplayName,
                ServiceName = Startup.ApplicationName,
                StartType = ServiceStartMode.Automatic
            };

            var windowsServiceProcessInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };

            this.Installers.AddRange(new Installer[] { windowsServiceProcessInstaller, windowsServiceInstaller });
        }
    }
}

