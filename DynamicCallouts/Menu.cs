namespace DynamicCallouts
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

    internal static class Menu
    {
        private static MenuPool pool;
        private static UIMenu mainMenu;

        private static Vehicle currentVehicle;

        private static readonly Keys KeyBinding = Settings.Menu;

        public static void Main()
        {
            pool = new MenuPool();

            // create the main menu
            mainMenu = new UIMenu("DynamicCallouts", "By ~g~Officer Jared, TheBroHypers");
            {
                var discordButton = new UIMenuItem("~b~Discord Server");
                discordButton.Activated += (s, e) => discordButtonClicked();

                var VehicleList = new UIMenuListScrollerItem<string>("Spawn Vehicle", "Spawn the selected vehicle.", new[] { "Police", "Police2", "Police3", "Police4", "Policeb", "Policet" });
                VehicleList.Activated += (s, e) => new Vehicle(VehicleList.SelectedItem, Game.LocalPlayer.Character.GetOffsetPositionFront(10f), Game.LocalPlayer.Character.Heading);

                mainMenu.AddItems(discordButton, VehicleList);
            }

            UIMenu childMenu = new UIMenu("Callouts", "Press ENTER to force the Callout.");
            {

                UIMenuItem bindItem = new UIMenuItem("Callouts");

                var ATMRobbery = new UIMenuItem("ATM Robbery");
                ATMRobbery.Activated += (s, e) => Functions.StartCallout("[DYNC] ATM Robbery");

                var IndividualShoutingAtPeople = new UIMenuItem("Individual Shouting at People");
                IndividualShoutingAtPeople.Activated += (s, e) => Functions.StartCallout("[DYNC] Individual Shouting at People");

                var HouseRaid = new UIMenuItem("House Raid");
                HouseRaid.Activated += (s, e) =>
                {
                    childMenu.Visible = false;
                    Functions.StartCallout("[DYNC] House Raid");
                };

                childMenu.AddItems(ATMRobbery, IndividualShoutingAtPeople, HouseRaid);

                mainMenu.AddItem(bindItem);
                mainMenu.BindMenuToItem(childMenu, bindItem);
            }

            pool.Add(mainMenu, childMenu);

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

                if (Game.IsKeyDown(KeyBinding))
                {
                    mainMenu.Visible = true;
                }
            }
        }


        // a command that simulates loading the plugin
        [ConsoleCommand]
        private static void RunMenuExample() => GameFiber.StartNew(Main);
    }
}