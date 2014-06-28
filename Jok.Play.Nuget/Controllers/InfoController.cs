using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Jok.Play.Controllers
{
    public class InfoController : ApiController
    {
        [HttpGet]
        public dynamic Stats()
        {
            decimal memoryInMB = 0;

            try
            {
                var proc = Process.GetCurrentProcess();
                memoryInMB = proc.PrivateMemorySize64 / (decimal)(1024 * 1024);
            }
            catch { }

            return Json(new
            {
                Name = Startup.ApplicationName,
                StartDate = Startup.StartDate,
                UpTimeDays = (DateTime.Now - Startup.StartDate).TotalDays,
                MemoryUsage = memoryInMB,
                Connections = Startup.GetConnectionsCount(),
                Tables = Startup.GetTablesCount()
            });
        }
    }
}
