using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jok.Play
{
    public class Jok<T>
        where T : IHub
    {
        static IHubContext Hub = GlobalHost.ConnectionManager.GetHubContext<T>();

        public static void Send(ICallback to, Action<dynamic> command, params ICallback[] exclude)
        {
            var users = GetUsers(to, exclude);
            if (users == null)
            {
                return;
            }

            command(Hub.Clients.Clients(users));
        }

        static IList<string> GetUsers(ICallback to, params ICallback[] exclude)
        {
            if (to == null) return null;

            var result = new List<string>();
            var ignoreList = new List<string>();

            exclude.ToList().ForEach(i1 => i1.ConnectionIDs.ForEach(ignoreList.Add));

            foreach (var item in to.ConnectionIDs)
            {
                if (!ignoreList.Contains(item))
                    result.Add(item);
            }

            if (result.Count == 0)
                return null;

            return result;
        }
    }
}
