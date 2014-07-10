using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace Jok.Play.Controllers
{
    public class InfoController : ApiController
    {
        [HttpGet]
        public dynamic Index()
        {
            return Json(new
            {
                Status = "Online"
            });
        }

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

            var tables = Startup.GetTables();

            return Json(new
            {
                Name = Startup.ApplicationName,
                StartDate = Startup.StartDate,
                UpTimeDays = (DateTime.Now - Startup.StartDate).TotalDays,
                MemoryUsage = memoryInMB,
                Connections = Startup.GetConnectionsCount(),
                Tables = tables.Count,
                TablesStarted = tables.Count(t => t.IsStarted),
                TablesFinished = tables.Count(t => t.IsFinished),
                GroupConnectionsCount = Startup.GetGroupConnectionsCount(),
                ErrorsCount = Startup.ErrorsCount
            });
        }

        [HttpGet]
        public dynamic Tables(bool? isStarted = null, bool? isFinished = null)
        {
            var tables = Startup.GetTables();

            if (isStarted.HasValue)
                tables = tables.Where(t => t.IsStarted).ToList();

            if (isFinished.HasValue)
                tables = tables.Where(t => t.IsFinished).ToList();

            return Json(tables);
        }
    }
}
