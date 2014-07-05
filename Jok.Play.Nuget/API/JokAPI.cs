using Jok.Play.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Jok.Play
{
    public static class JokAPI
    {
        /// <summary>
        /// Default: https://api.jok.ge/user/{0}/info?ip={1}&gameid={2}
        /// </summary>
        public static string GetUserUrl { get; set; }
        /// <summary>
        /// Default: https://api.jok.ge/game/{0}/finish
        /// </summary>
        public static string FinishGameUrl { get; set; }
        public static string PlayerLoginUrl { get; set; }
        public static string PlayerLogoutUrl { get; set; }
        public static int? GameID { get; set; }
        public static string GameSecret { get; set; }

        static JokAPI()
        {
            GameID = null;
            GameSecret = null;
            GetUserUrl = "http://api.jok.io/User/InfoBySID/?sid={0}&ipAddress={1}";
            FinishGameUrl = "http://api.jok.io/Game/Finish";
            PlayerLoginUrl = "http://api.jok.io/Game/PlayerLogin";
            PlayerLogoutUrl = "http://api.jok.io/Game/PlayerLogout";
        }


        public static JokUserInfo GetUser(string token, string ipaddress)
        {
            try
            {
                var url = String.Format(GetUserUrl, token, ipaddress);
                var t = new HttpClient().GetStringAsync(url);
                t.Wait();

                return JsonConvert.DeserializeObject<JokUserInfo>(t.Result);
            }
            catch
            {
                return new JokUserInfo();
            }
        }

        public static void PlayerLogin(int userID, string IPAddress)
        {
            try
            {
                var request = new HttpClient().PostAsJsonAsync(PlayerLoginUrl, new PlayerStatusChangeModel
                {
                    GameID = JokAPI.GameID,
                    GameSecret = JokAPI.GameSecret,
                    UserID = userID,
                    IPAddress = IPAddress
                });
            }
            catch
            {
            }
        }

        public static void PlayerLogout(int userID)
        {
            try
            {
                var request = new HttpClient().PostAsJsonAsync(PlayerLogoutUrl, new PlayerStatusChangeModel
                {
                    GameID = JokAPI.GameID,
                    GameSecret = JokAPI.GameSecret,
                    UserID = userID
                });
            }
            catch
            {
            }
        }

        public static GameFinishResult FinishGame(bool? isFullGame, bool? isDraw, string channel, string results, params GameFinishPlayerInfo[] playerInfos)
        {
            try
            {
                if (GameID == null || GameSecret == null)
                    new GameFinishResult { Error = new Exception("Please set GameID & GameSecret") };

                var gameInfo = new GameFinishInfo
                {
                    GameID = GameID.Value,
                    GameSecret = GameSecret,
                    IsFull = isFullGame,
                    IsDraw = isDraw,
                    Channel = channel,
                    Results = results,
                    Players = playerInfos.ToList()
                };


                var url = String.Format(FinishGameUrl, GameID);
                var postTask = new HttpClient().PostAsJsonAsync<GameFinishInfo>(url, gameInfo);
                postTask.Wait();

                if (postTask.IsCompleted && postTask.Result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var resultTask = postTask.Result.Content.ReadAsAsync<GameFinishResult>();
                    resultTask.Wait();

                    if (!resultTask.IsCompleted)
                    {
                        return new GameFinishResult { Error = resultTask.Exception };
                    }

                    return resultTask.Result;
                }

                return new GameFinishResult { Error = postTask.Exception };
            }
            catch (Exception ex)
            {
                return new GameFinishResult { Error = ex };
            }
        }
    }
}
