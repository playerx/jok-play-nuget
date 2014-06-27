using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Jok.GameEngine
{
    public abstract class GameControllerBase : Controller
    {
        protected abstract string AuthorizationUrl { get; }
        protected abstract string ExitUrl { get; }
        protected abstract int ConnectionsCount { get; }
        protected abstract int TablesCount { get; }

        public static DateTime CreateDate { get; set; }

        static GameControllerBase()
        {
            CreateDate = DateTime.UtcNow;

            try
            {
                JokAPI.GameID = Convert.ToInt32(ConfigurationManager.AppSettings["Jok:GameID"]);
                JokAPI.GameSecret = ConfigurationManager.AppSettings["Jok:GameSecret"];
            }
            catch { Debug.WriteLine("Jok:GameID or Jok:GameSecret not found in web.config"); }
        }


        public virtual ActionResult Index()
        {
            return RedirectToAction("Play");
        }

        public virtual ActionResult Play(string id, string sid, string source)
        {
            if (!String.IsNullOrEmpty(sid))
            {
                var cookie = Request.Cookies["sid"];
                if (cookie == null)
                    cookie = new HttpCookie("sid", sid);

                cookie.Value = sid;
                cookie.Expires = DateTime.UtcNow.AddYears(30);

                if (Request.Url.Host.Contains('.'))
                    cookie.Domain = Request.Url.Host.Substring(Request.Url.Host.IndexOf('.'));

                Response.Cookies.Remove("sid");
                Response.Cookies.Add(cookie);
            }
            else
            {
                sid = Request.Cookies["sid"] == null ? "" : Request.Cookies["sid"].Value;
            }



            if (String.IsNullOrEmpty(sid))
                return Redirect(AuthorizationUrl);


            var userInfo = JokAPI.GetUser(sid, Request.UserHostAddress);
            if (userInfo.IsSuccess != true)
                return Redirect(AuthorizationUrl + "&getUserInfo=failed");

            if (!String.IsNullOrEmpty(userInfo.CultureName))
            {
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(userInfo.CultureName);
                ViewBag.Language = userInfo.CultureName.Replace('-', '_');
            }


            ViewBag.ID = id;
            ViewBag.SID = sid;
            ViewBag.Source = source;
            ViewBag.IsMobileApp = (source == "mobileapp");
            ViewBag.GameID = JokAPI.GameID;
            ViewBag.UserID = userInfo.UserID;
            ViewBag.IsVIPMember = userInfo.IsVIP;
            ViewBag.Channel = (!String.IsNullOrWhiteSpace(id) && id.ToLower() == "private") ? ShortGuid.NewGuid().ToString() : id;
            ViewBag.AuthorizationUrl = AuthorizationUrl;
            ViewBag.ExitUrl = ExitUrl;

            return View();
        }

        public virtual ActionResult Stats(string id)
        {
            return Json(new
            {
                ConnectionsCount = ConnectionsCount,
                TablesCount = TablesCount,
                StartTime = CreateDate.ToString(),
                Uptime = (DateTime.Now - CreateDate)
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
