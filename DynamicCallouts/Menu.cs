﻿namespace DynamicCallouts
{
    using System.Linq;
    using System.Windows.Forms;
    using Rage;
    using Rage.Attributes;
    using Rage.Native;
    using RAGENativeUI;
    using RAGENativeUI.Elements;
    using RAGENativeUI.PauseMenu;
    using System.Diagnostics;
    using LSPD_First_Response.Mod.Callouts;
    using LSPD_First_Response.Mod.API;
    using DynamicCallouts.Utilities;
    using System;

    internal static class Menu
    {
        private static MenuPool pool;
        private static UIMenu mainMenu;

        public static Keys MenuKeyBinding;
        public static Keys DialogKeyBinding;
        public static Keys EndCallKeyBinding;
        public static Keys Interaction1KeyBinding;
        public static Keys Interaction2KeyBinding;
        public static InitializationFile ini;
        public static string NewCallsign;

        public static void Main()
        {
            var path = "Plugins/LSPDFR/DynamicCallouts/DynamicCallouts.ini";
            ini = new InitializationFile(path);
            ini.Create();
            MenuKeyBinding = ini.ReadEnum("Keys", "Menu", Keys.F9);
            DialogKeyBinding = Settings.Dialog;
            EndCallKeyBinding = ini.ReadEnum("Keys", "EndCall", Keys.End);
            Interaction1KeyBinding = ini.ReadEnum("Keys", "InteractionKey1", Keys.K);
            Interaction2KeyBinding = ini.ReadEnum("Keys", "InteractionKey2", Keys.L);

            pool = new MenuPool();

            // create the main menu
            mainMenu = new UIMenu("DynamicCallouts", "By ~g~Officer Jared, TheBroHypers");
            {
                var discordButton = new UIMenuItem("~b~Discord Server");
                discordButton.Activated += (s, e) => discordButtonClicked();

                var VehicleList = new UIMenuListScrollerItem<string>("Spawn Vehicle", "Spawn the selected vehicle.", new[] { "Police", "Police2", "Police3", "Police4", "Policeb", "Policet" });
                VehicleList.Activated += (s, e) => new Vehicle(VehicleList.SelectedItem, Game.LocalPlayer.Character.GetOffsetPositionFront(10f), Game.LocalPlayer.Character.Heading);

                var Stats = new UIMenuItem("Stats");
                Stats.Activated += (s, e) =>
                {
                    mainMenu.Visible = false;
                    StatsView.tabView.Visible = true;
                };

                mainMenu.AddItems(discordButton, VehicleList, Stats);
            }

            UIMenu settings = new UIMenu("Settings", "Change your settings inside the game!");
            {
                UIMenuItem bindItem = new UIMenuItem("Settings");

                mainMenu.AddItem(bindItem);
                mainMenu.BindMenuToItem(settings, bindItem);
            }

            UIMenu Keybinds = new UIMenu("Keybinds", "Change your keybinds");
            {
                UIMenuItem keybindsBindItem = new UIMenuItem("Keybinds");

                var EndCall = new UIMenuListScrollerItem<string>("EndCall", "Change the keybind of EndCall", new[] { Convert.ToString(Settings.EndCall), "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Escape", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "Insert", "Home", "Delete", "End", "PageUp", "PageDown", "NumLock", "NumPad0", "NumPad1", "NumPad2", "NumPad3", "NumPad4", "NumPad5", "NumPad6", "NumPad7", "NumPad8", "NumPad9"});
                EndCall.Activated += (s, e) =>
                {
                    Settings.ini.Write("Keys", "EndCall", " " + EndCall.SelectedItem);
                    EndCallKeyBinding = ini.ReadEnum("Keys", "EndCall", Keys.End);
                    Settings.EndCall = EndCallKeyBinding;
                };

                var Dialog = new UIMenuListScrollerItem<string>("Dialog", "Change the keybind of Dialog", new[] { Convert.ToString(Settings.Dialog), "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Escape", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "Insert", "Home", "Delete", "End", "PageUp", "PageDown", "NumLock", "NumPad0", "NumPad1", "NumPad2", "NumPad3", "NumPad4", "NumPad5", "NumPad6", "NumPad7", "NumPad8", "NumPad9" });
                Dialog.Activated += (s, e) =>
                {
                    Settings.ini.Write("Keys", "Dialog", " " + Dialog.SelectedItem);
                    DialogKeyBinding = ini.ReadEnum("Keys", "Dialog", Keys.Y);
                    Settings.Dialog = DialogKeyBinding;
                };

                var InteractionKey1 = new UIMenuListScrollerItem<string>("InteractionKey1", "Change the keybind of InteractionKey1", new[] { Convert.ToString(Settings.InteractionKey1), "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Escape", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "Insert", "Home", "Delete", "End", "PageUp", "PageDown", "NumLock", "NumPad0", "NumPad1", "NumPad2", "NumPad3", "NumPad4", "NumPad5", "NumPad6", "NumPad7", "NumPad8", "NumPad9" });
                InteractionKey1.Activated += (s, e) =>
                {
                    Settings.ini.Write("Keys", "InteractionKey1", " " + InteractionKey1.SelectedItem);
                    Interaction1KeyBinding = ini.ReadEnum("Keys", "InteractionKey1", Keys.K);
                    Settings.InteractionKey1 = Interaction1KeyBinding;
                };

                var InteractionKey2 = new UIMenuListScrollerItem<string>("InteractionKey2", "Change the keybind of InteractionKey2", new[] { Convert.ToString(Settings.InteractionKey2), "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Escape", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "Insert", "Home", "Delete", "End", "PageUp", "PageDown", "NumLock", "NumPad0", "NumPad1", "NumPad2", "NumPad3", "NumPad4", "NumPad5", "NumPad6", "NumPad7", "NumPad8", "NumPad9" });
                InteractionKey2.Activated += (s, e) =>
                {
                    Settings.ini.Write("Keys", "InteractionKey2", " " + InteractionKey2.SelectedItem);
                    Interaction2KeyBinding = ini.ReadEnum("Keys", "InteractionKey2", Keys.L);
                    Settings.InteractionKey2 = Interaction2KeyBinding;
                };

                var MenuKey = new UIMenuListScrollerItem<string>("Menu", "Change the keybind of Menu", new[] { Convert.ToString(Settings.Menu), "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Escape", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "Insert", "Home", "Delete", "End", "PageUp", "PageDown", "NumLock", "NumPad0", "NumPad1", "NumPad2", "NumPad3", "NumPad4", "NumPad5", "NumPad6", "NumPad7", "NumPad8", "NumPad9" });
                MenuKey.Activated += (s, e) =>
                {
                    Settings.ini.Write("Keys", "Menu", " " + MenuKey.SelectedItem);
                    MenuKeyBinding = ini.ReadEnum("Keys", "Menu", Keys.F9);
                    Settings.Menu = MenuKeyBinding;
                };

                Keybinds.AddItems(EndCall, Dialog, InteractionKey1, InteractionKey2, MenuKey);
                settings.AddItem(keybindsBindItem);
                settings.BindMenuToItem(Keybinds, keybindsBindItem);
            };

            UIMenu officerSettings = new UIMenu("Officer Settings", "Change your officer settings");
            {
                UIMenuItem officerSettingsBindItem = new UIMenuItem("Officer Settings");

                var OfficerCallsign = new UIMenuItem("Callsign", "Change your callsign");
                OfficerCallsign.Activated += (s, e) =>
                {
                    Localization.SetText("Callsign", "Enter Callsign (MAX 40 CHARACTERS):");
                    var result = CallHandler.OpenTextInput("Callsign", Settings.CallSign, 40);
                    Settings.ini.Write("Officer Settings", "CallSign", " " + result);
                    Settings.CallSign = result;
                    StatsView.tabView.Name = result;
                    Localization.ClearTextOverride("Callsign");
                };

                var OfficerName = new UIMenuItem("Officer Name", "Change your officers name");
                OfficerName.Activated += (s, e) =>
                {
                    Localization.SetText("OfficerName", "Enter Officer Name (MAX 60 CHARACTERS):");
                    var result = CallHandler.OpenTextInput("OfficerName", Settings.OfficerName, 60);
                    Settings.ini.Write("Officer Settings", "OfficerName", " " + result);
                    Settings.OfficerName = result;
                    StatsView.tabView.MoneySubtitle = result;
                    Localization.ClearTextOverride("OfficerName");
                };

                officerSettings.AddItems(OfficerCallsign, OfficerName);
                settings.AddItem(officerSettingsBindItem);
                settings.BindMenuToItem(officerSettings, officerSettingsBindItem);
            };

            UIMenu Callouts = new UIMenu("Callouts", "Enable and disable callouts!");
            {
                UIMenuItem calloutsBindItem = new UIMenuItem("Callouts");

                var IndividualShoutingAtPeople = new UIMenuCheckboxItem("Individual Shouting At People", Settings.IndividualShoutingAtPeople);
                IndividualShoutingAtPeople.CheckboxEvent += (s, c) =>
                {
                    if (IndividualShoutingAtPeople.Checked)
                        Settings.ini.Write("Callouts", "IndividualShoutingAtPeople", " " + true);
                    if (!IndividualShoutingAtPeople.Checked)
                        Settings.ini.Write("Callouts", "IndividualShoutingAtPeople", " " + false);
                    Settings.IndividualShoutingAtPeople = Settings.ini.ReadBoolean("Callouts", "IndividualShoutingAtPeople", true);
                };

                var ATMRobbery = new UIMenuCheckboxItem("ATMRobbery", Settings.ATMRobbery);
                ATMRobbery.CheckboxEvent += (s, c) =>
                {
                    if (ATMRobbery.Checked)
                        Settings.ini.Write("Callouts", "ATMRobbery", " " + true);
                    if (!ATMRobbery.Checked)
                        Settings.ini.Write("Callouts", "ATMRobbery", " " + false);
                    Settings.ATMRobbery = Settings.ini.ReadBoolean("Callouts", "ATMRobbery", true);
                };

                var HouseRaid = new UIMenuCheckboxItem("HouseRaid", Settings.HouseRaid);
                HouseRaid.CheckboxEvent += (s, c) =>
                {
                    if (HouseRaid.Checked)
                        Settings.ini.Write("Callouts", "HouseRaid", " " + true);
                    if (!HouseRaid.Checked)
                        Settings.ini.Write("Callouts", "HouseRaid", " " + false);
                    Settings.HouseRaid = Settings.ini.ReadBoolean("Callouts", "HouseRaid", true);
                };

                Callouts.AddItems(IndividualShoutingAtPeople, ATMRobbery, HouseRaid);
                settings.AddItems(calloutsBindItem);
                settings.BindMenuToItem(Callouts, calloutsBindItem);
            }

            pool.Add(mainMenu, settings, Callouts, Keybinds, officerSettings);

            GameFiber.StartNew(ProcessMenus);
        }

        private static void VehicleList_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            throw new System.NotImplementedException();
        }

        private static void discordButtonClicked()
        {
            System.Diagnostics.Process.Start("https://discord.gg/DN6hu49pSW");
        }

        private static void ProcessMenus()
        {

            while (true)
            {
                GameFiber.Yield();

                pool.ProcessMenus();

                if (Game.IsKeyDown(MenuKeyBinding))
                {
                    mainMenu.Visible = true;
                }
            }
        }
    }
}