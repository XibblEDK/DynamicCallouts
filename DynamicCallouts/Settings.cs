using System.Windows.Forms;
using System.Collections.Generic;
using Rage;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System;

namespace DynamicCallouts
{
    internal static class Settings
    {
        internal static bool AutomaticBackup = true;
        internal static bool LeaveCalloutsRunning = false;
        internal static bool HelpMessages = true;
        internal static Keys EndCall = Keys.End;
        internal static Keys Dialog = Keys.Y;
        internal static Keys Menu = Keys.F9;
        internal static Keys InteractionKey1 = Keys.K;
        internal static Keys InteractionKey2 = Keys.L;
        internal static string CallSign = "1-Lincoln-18";

        internal static void LoadSettings()
        {
            Game.LogTrivial("[LOG]: Loading config file from DynamicCallouts.");
            var path = "Plugins/LSPDFR/DynamicCallouts/DynamicCallouts.ini";
            var ini = new InitializationFile(path);
            ini.Create();
            EndCall = ini.ReadEnum("Keys", "EndCall", Keys.End);
            Dialog = ini.ReadEnum("Keys", "Dialog", Keys.Y);
            Menu = ini.ReadEnum("Keys", "Menu", Keys.F9);
            InteractionKey1 = ini.ReadEnum("Keys", "InteractionKey1", Keys.K);
            InteractionKey2 = ini.ReadEnum("Keys", "InteractionKey2", Keys.L);
            HelpMessages = ini.ReadBoolean("Miscellaneous", "HelpMessages", true);
            LeaveCalloutsRunning = ini.ReadBoolean("Miscellaneous", "LeaveCalloutsRunning", false);
            AutomaticBackup = ini.ReadBoolean("Miscellaneous", "AutomaticBackup", true);
            CallSign = ini.ReadString("Officer Settings", "CallSign", "1-Lincoln-18");
        }
        public static readonly string PluginVersion = "1.0.5.1";
    }
}