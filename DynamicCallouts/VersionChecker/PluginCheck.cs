using System;
using Rage;
using System.Net;
using System.Linq;

namespace DynamicCallouts.VersionChecker
{
    public class PluginCheck
    {
        public static bool isUpdateAvailable()
        {
            string curVersion = Settings.PluginVersion;
            Uri latestVersionUri = new Uri("https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=40494&textOnly=1");
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
    }
}