using System;
using Rage;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Net.Http;

namespace DynamicCallouts.VersionChecker
{
    public class PluginCheck
    {
        public static bool isUpdateAvailable()
        {
            string curVersion = Settings.PluginVersion;
            Uri latestVersionUri = new Uri("https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=40649&textOnly=1");
            WebClient webClient = new WebClient();
            string receivedData = string.Empty;
            try
            {
                receivedData = webClient.DownloadString(latestVersionUri).Trim();
            }
            catch (WebException)
            {
                GameFiber.Wait(35000);
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~w~DynamicCallouts Warning", "~r~Failed to check for an update", "Please make sure you are ~y~connected~w~ to the internet or try to ~y~reload~w~ the plugin.");
                Game.Console.Print();
                Game.Console.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~================================================== DynamicCallouts ===================================================~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Game.Console.Print();
                Game.Console.Print("[WARNING]: Failed to check for an update.");
                Game.Console.Print("[LOG]: Please make sure you are connected to the internet or try to reload the plugin.");
                Game.Console.Print();
                Game.Console.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~================================================== DynamicCallouts ===================================================~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Game.Console.Print();
                return false;
            }
            if (receivedData != Settings.PluginVersion)
            {
                GameFiber.Wait(35000);
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~w~DynamicCallouts Warning", "~y~A new Update is available!", "Current Version: ~r~" + curVersion + "~w~<br>New Version: ~g~" + receivedData + "<br>~r~Please update to the latest version!");
                Game.Console.Print();
                Game.Console.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~================================================== DynamicCallouts ===================================================~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Game.Console.Print();
                Game.Console.Print("[WARNING]: A newer version of DynamicCallouts is available! Update to the latest version or play on your own risk.");
                Game.Console.Print("[LOG]: Current Version:  " + curVersion);
                Game.Console.Print("[LOG]: New Version:  " + receivedData);
                Game.Console.Print();
                Game.Console.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~================================================== DynamicCallouts ===================================================~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Game.Console.Print();
                return true;
            }
            else
            {
                Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "~w~DynamicCallouts", "", "Detected the ~g~latest~w~ version of ~y~DynamicCallouts~w~!");
                return false;
            }
        }

        /*public static async System.Threading.Tasks.Task<bool> isRNUIUpdateAvailable()
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("RAGENativeUI"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("alexguirre", "RAGENativeUI");

            var versionInfo = FileVersionInfo.GetVersionInfo("RAGENativeUI");

            Version latestGitHubVersion = new Version(releases[0].TagName);
            Version localVersion = new Version(versionInfo.ToString());

            int versionComparison = localVersion.CompareTo(latestGitHubVersion);
            if (versionComparison < 0)
            {
                GameFiber.Wait(35000);
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~w~DynamicCallouts Warning", "~y~A new RAGENativeUI Update is available!", "Current Version: ~r~" + localVersion + "~w~<br>New Version: ~g~" + latestGitHubVersion + "<br>~r~Please update to the latest version!");
                Game.Console.Print();
                Game.Console.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~================================================== DynamicCallouts ===================================================~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Game.Console.Print();
                Game.Console.Print("[WARNING]: A newer version of RAGENativeUI is available! Update to the latest version or play on your own risk.");
                Game.Console.Print("[LOG]: Current Version:  " + localVersion);
                Game.Console.Print("[LOG]: New Version:  " + latestGitHubVersion);
                Game.Console.Print();
                Game.Console.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~================================================== DynamicCallouts ===================================================~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Game.Console.Print();
                return true;
            }
            else if (versionComparison > 0)
            {
                GameFiber.Wait(35000);
                Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~w~DynamicCallouts Warning", "~y~RAGENativeUI", "Your local version is greater than the one on GitHub. Current Version: ~r~" + localVersion + "~w~<br><Version on GitHub: ~g~" + latestGitHubVersion + "<br>~r~Please get this fixed!");
                Game.Console.Print();
                Game.Console.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~================================================== DynamicCallouts ===================================================~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Game.Console.Print();
                Game.Console.Print("[WARNING]: Local version is greater than the released version. Please get this fixed.");
                Game.Console.Print("[LOG]: Current Version:  " + localVersion);
                Game.Console.Print("[LOG]: GitHub Version:  " + latestGitHubVersion);
                Game.Console.Print();
                Game.Console.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~================================================== DynamicCallouts ===================================================~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Game.Console.Print();
                return false;
            }
            else 
            {
                Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "~w~DynamicCallouts", "", "Detected the ~g~latest~w~ version of ~y~RAGENativeUI~w~!");
                return false;
            }
        }*/
    }
}