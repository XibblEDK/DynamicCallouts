using System.Windows.Forms;
using System.Collections.Generic;
using Rage;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System;
using System.Xml;
using DynamicCallouts;
using DynamicCallouts.Callouts;

namespace DynamicCallouts
{
    internal static class Settings
    {
        internal static XmlDocument Stats = new XmlDocument();
        internal static string xmlpath = "Plugins/LSPDFR/DynamicCallouts/Stats.xml";
        internal static int RespondedCallouts = 0;
        internal static int Arrests = 0;
        internal static int Pursuits = 0;
        internal static int InvolvedInFights = 0;

        internal static bool AutomaticBackup = true;
        internal static bool HelpMessages = true;
        internal static bool IndividualShoutingAtPeople = true;
        internal static bool ATMRobbery = true;
        internal static bool GunshotsReported = true;
        internal static bool GarbageOnFire = true;
        internal static bool LorryPursuit = true;
        internal static Keys EndCall = Keys.End;
        internal static Keys Dialog = Keys.Y;
        internal static Keys Menu;
        internal static Keys InteractionKey1 = Keys.K;
        internal static Keys InteractionKey2 = Keys.L;
        internal static string CallSign;
        internal static string OfficerName = "Jared Anthony";
        internal static InitializationFile ini;
        internal static string inipath = "Plugins/LSPDFR/DynamicCallouts/DynamicCallouts.ini";

        internal static void LoadSettings()
        {
            Game.Console.Print("[LOG]: Loading config file from DynamicCallouts.");
            ini = new InitializationFile(inipath);
            ini.Create();
            EndCall = ini.ReadEnum("Keys", "EndCall", Keys.End);
            Dialog = ini.ReadEnum("Keys", "Dialog", Keys.Y);
            Menu = ini.ReadEnum("Keys", "Menu", Keys.End);
            InteractionKey1 = ini.ReadEnum("Keys", "InteractionKey1", Keys.K);
            InteractionKey2 = ini.ReadEnum("Keys", "InteractionKey2", Keys.L);
            HelpMessages = ini.ReadBoolean("Miscellaneous", "HelpMessages", true);
            AutomaticBackup = ini.ReadBoolean("Miscellaneous", "AutomaticBackup", true);
            CallSign = ini.ReadString("Officer Settings", "CallSign", "1-Lincoln-18");
            OfficerName = ini.ReadString("Officer Settings", "OfficerName", "Jared Anthony");
            IndividualShoutingAtPeople = ini.ReadBoolean("Callouts", "IndividualShoutingAtPeople", true);
            ATMRobbery = ini.ReadBoolean("Callouts", "ATMRobbery", true);
            GunshotsReported = ini.ReadBoolean("Callouts", "GunshotsReported", true);
            GarbageOnFire = ini.ReadBoolean("Callouts", "GarbageOnFire", true);
            LorryPursuit = ini.ReadBoolean("Callouts", "LorryPursuit", true);

            Game.Console.Print("[LOG]: Loading Stats file from DynamicCallouts");
            Stats.Load(xmlpath);
            RespondedCallouts = Convert.ToInt32(Stats.SelectSingleNode("Stats/RespondedCallouts").InnerText);
            Arrests = Convert.ToInt32(Stats.SelectSingleNode("Stats/Arrests").InnerText);
            Pursuits = Convert.ToInt32(Stats.SelectSingleNode("Stats/Pursuits").InnerText);
            InvolvedInFights = Convert.ToInt32(Stats.SelectSingleNode("Stats/InvolvedInFights").InnerText);
        }
        public static readonly string PluginVersion = "1.0.0.0";
    }
}