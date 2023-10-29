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
using DynamicCallouts.Utilities;
using RAGENativeUI.Elements;
using Rage.ConsoleCommands;
using System.Runtime.InteropServices;
using Rage.Native;
using Rage.Attributes;

[assembly: Rage.Attributes.Plugin("DynamicCallouts", Description = "LSPDFR Callout Pack", Author = "XibblE, TheBroHypers")]
namespace DynamicCallouts
{
    public class Main : Plugin
    {
        public static bool CalloutInterface;
        public static bool STP;
        public static Sprite headshotSprite;

        public override void Finally() { }

        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;

            //Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "DynamicCallouts", "~g~v" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " ~w~by ~b~Officer Jarad", "~g~successfully loaded!");
        }

        private void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
                GameFiber.StartNew(delegate
                {
                    Game.AddConsoleCommands();
                    Settings.LoadSettings();
                    Menu.Main();
                    StatsView.Main();
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

                    Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "DynamicCallouts", "~g~v" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " ~w~by ~b~Officer Jared", "~g~successfully loaded! Have a great shift! :)");
                    GameFiber.Wait(5000);
                    Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Player Info", "Dynamic Callouts", "Callsign: " + Settings.CallSign + "<br>Officer Name: " + Settings.OfficerName + "<br>Responded Callouts: " + Settings.RespondedCallouts + "<br>Arrests: " + Settings.Arrests + "<br>Pursuits: " + Settings.Pursuits + "<br>Involved in fights: " + Settings.InvolvedInFights);

                    PluginCheck.isUpdateAvailable();
                });

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
            Functions.RegisterCallout(typeof(GunshotsReported));
            Functions.RegisterCallout(typeof(GarbageOnFire));
            Functions.RegisterCallout(typeof(LorryPursuit));
            Functions.RegisterCallout(typeof(HusbandMurdered));
            //Functions.RegisterCallout(typeof(DealershipCarStolen));
            Game.AddConsoleCommands();
            Game.Console.Print("[LOG]: All callouts were loaded!");
            Game.Console.Print();
            Game.Console.Print("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~================================================== DynamicCallouts ===================================================~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Game.Console.Print();
        }

        [ConsoleCommand]
        private static void Command_GoToHouseInterior()
        {
            Game.LocalPlayer.Character.Position = new Vector3(266.139f, -1007.465f, -101.009f);
        }

        [ConsoleCommand]
        private static void Command_GetVector3FeetLocation()
        {
            Game.LogTrivial(Game.LocalPlayer.Character.GetOffsetPosition(Vector3.RelativeBottom).ToString());
        }
    }
}