using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Leauge_Auto_Accept
{
    internal class LCU
    {
        private static string[] leagueAuth;
        private static int lcuPid = 0;
        public static bool isLeagueOpen = false;

        public static void CheckIfLeagueClientIsOpenTask()
        {
            while (true)
            {
                Process client = Process.GetProcessesByName("LeagueClientUx").FirstOrDefault();
                if (client == null)
                {
                    isLeagueOpen = false;
                    MainLogic.gameState.Flags.IsAutoAcceptOn = false;
                    Data.champsSorterd.Clear();
                    Data.spellsSorted.Clear();
                    Data.currentSummonerId = "";
                    if (UI.currentWindow != "leagueClientIsClosedMessage" && UI.currentWindow != "exitMenu") UI.LeagueClientIsClosedMessage();
                    Thread.Sleep(2000);
                    return;
                }
                leagueAuth = getLeagueAuth(client);
                isLeagueOpen = true;
                if (lcuPid != client.Id)
                {
                    lcuPid = client.Id;
                    if (Settings.preloadData) // Check if preload data was enabled last time
                    {
                        Data.LoadChampionsList();
                        Data.LoadSpellsList();
                    }
                    if (Settings.shouldAutoAcceptbeOn) MainLogic.gameState.Flags.IsAutoAcceptOn = true;
                    if (UI.currentWindow != "exitMenu") UI.MainScreen();
                }
                Thread.Sleep(2000);
            }
        }

        public static bool CheckIfLeagueClientIsOpen()
        {
            Process client = Process.GetProcessesByName("LeagueClientUx").FirstOrDefault();
            if (client != null) return true; 
            else return false; 
        }

        private static string[] getLeagueAuth(Process client)
        {
            string command = "wmic process where 'Processid=" + client.Id + "' get Commandline";
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + command);
            psi.RedirectStandardOutput = true;
            Process cmd = new Process();
            cmd.StartInfo = psi;
            cmd.Start();
            string output = cmd.StandardOutput.ReadToEnd();
            cmd.WaitForExit();
            string port = Regex.Match(output, @"--app-port=""?(\d+)""?").Groups[1].Value; // Parse the port and auth token into variables
            string authToken = Regex.Match(output, @"--remoting-auth-token=([a-zA-Z0-9_-]+)").Groups[1].Value;
            string auth = "riot:" + authToken; // Compute the encoded key
            string authBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(auth));
            return new string[] { authBase64, port }; // Return content
        }

        public static string[] clientRequest(string method, string url, string body = null)
        {
            var handler = new HttpClientHandler() // Ignore invalid https
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            try
            {
                using (HttpClient client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri("https://127.0.0.1:" + leagueAuth[1] + "/"); // Set URL
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", leagueAuth[0]);
                    HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), url); // Set headers
                    if (!string.IsNullOrEmpty(body)) request.Content = new StringContent(body, Encoding.UTF8, "application/json"); // Send POST data when doing a post request                    
                    HttpResponseMessage response = client.SendAsync(request).Result; // Get the response
                    // If the response is null (League client closed?)
                    if (response == null) return new string[] { "999", "" };
                    int statusCode = (int)response.StatusCode;  // Get the HTTP status code
                    string statusString = statusCode.ToString();
                    string responseFromServer = response.Content.ReadAsStringAsync().Result;  // Get the body
                    response.Dispose(); // Clean up the response
                    return new string[] { statusString, responseFromServer }; // Return content
                }
            }
            catch // If the URL is invalid (League client closed?)
            { return new string[] { "999", "" }; }
        }

        public static string[] clientRequestUntilSuccess(string method, string url, string body = null)
        {
            string[] request = { "000", "" };
            while (request[0].Substring(0, 1) != "2")
            {
                request = clientRequest(method, url, body);
                if (request[0].Substring(0, 1) != "2" && CheckIfLeagueClientIsOpen()) Thread.Sleep(1000);
                return request;
            }
            return request;
        }
    }
}
