using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Drawing;
using System.Xml;
using LSPD_First_Response.Mod.API;
using Rage;
using DynamicCallouts.Callouts;
using DynamicCallouts.VersionChecker;
using DynamicCallouts;

[assembly: Rage.Attributes.Plugin("DynamicCallouts", Description = "LSPDFR Callout Pack", Author = "XibblE, TheBroHypers")]
namespace DynamicCallouts
{
    public class Main : Plugin
    {
        public XmlDocument rankSys = new XmlDocument();
        public int XPValue;
        public string CallSign;

        public static bool CalloutInterface;
        public static bool STP;

        public override void Finally() { }

        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;

            GameFiber.StartNew(delegate
            {
                Menu.Main();
            });


            Settings.LoadSettings();

            //Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "DynamicCallouts", "~g~v" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " ~w~by ~b~Officer Jarad", "~g~successfully loaded!");
        }

        private void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
                RegisterCallouts();
                Game.Console.Print();
                Game.Console.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~================================================== DynamicCallouts ===================================================~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Game.Console.Print();
                Game.Console.Print("[LOG]: Callouts and settings were loaded correctly.");
                Game.Console.Print("[LOG]: The config file was loaded successfully.");
                Game.Console.Print("[VERSION]: Found Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
                Game.Console.Print("[LOG]: Checking for a new DynamicCallouts version...");
                Game.Console.Print();
                Game.Console.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~================================================== DynamicCallouts ===================================================~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Game.Console.Print();

                Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "DynamicCallouts", "~g~v" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " ~w~by ~b~Officer Jarad", "~g~successfully loaded!");

                PluginCheck.isUpdateAvailable();
                /*rankSys.Load("Plugins/LSPDFR/DynamicCallouts/Rank.xml");
                XPValue = Convert.ToInt32(rankSys.SelectSingleNode("Rank/XP").InnerText);
                Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "DynamicCallouts", "Stats", "Realism Counter: ~g~" + XPValue + "~w~.<br>CallSign: ~b~" + Settings.CallSign + "~w~.");*/

            }
        }

        private static void RegisterCallouts()
        {
            Game.Console.Print();
            Game.Console.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~================================================== DynamicCallouts ===================================================~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Game.Console.Print();
            if (Functions.GetAllUserPlugins().ToList().Any(a => a != null && a.FullName.Contains("CalloutInterface")) == true)
            {
                Game.LogTrivial("User has CalloutInterface INSTALLED starting integration.");
                CalloutInterface = true;
            }
            else
            {
                Game.LogTrivial("User do NOT have CalloutInterface installed.");
                CalloutInterface = false;
            }
            if (Functions.GetAllUserPlugins().ToList().Any(a => a != null && a.FullName.Contains("StopThePed")) == true)
            {
                Game.LogTrivial("User has StopThePed INSTALLED starting integration.");
                STP = true;
            }
            else
            {
                Game.LogTrivial("User do NOT have StopThePed installed.");
                STP = false;
            }
            Functions.RegisterCallout(typeof(IndividualShoutingAtPeople));
            Functions.RegisterCallout(typeof(ATMRobbery));
            Functions.RegisterCallout(typeof(HouseRaid));
            Functions.RegisterCallout(typeof(StolenVehicle));
            Game.Console.Print("[LOG]: All callouts were loaded!");
            Game.Console.Print();
            Game.Console.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~================================================== DynamicCallouts ===================================================~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Game.Console.Print();
        }
    }
}