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

    internal static class Menu
    {
        private static MenuPool pool;
        private static UIMenu mainMenu;

        private static Vehicle currentVehicle;

        private static readonly Keys KeyBinding = Settings.Menu;

        public static void Main()
        {
            // create the pool that handles drawing and processing the menus
            pool = new MenuPool();

            // create the main menu
            mainMenu = new UIMenu("DynamicCallouts", "By ~g~Officer Jarad, TheBroHypers");

            // create the menu items
            {
                var discordButton = new UIMenuItem("~b~Discord Server");
                discordButton.Activated += (s, e) => discordButtonClicked();

                var VehicleList = new UIMenuListScrollerItem<string>("Spawn Vehicle", "Spawn the selected vehicle.", new[] { "Police", "Police2", "Police3", "Police4", "Policeb", "Policet" });
                VehicleList.Activated += (s, e) => new Vehicle(VehicleList.SelectedItem, Game.LocalPlayer.Character.GetOffsetPositionFront(10f), Game.LocalPlayer.Character.Heading);

                mainMenu.AddItems(discordButton, VehicleList);
            }

            // create a child menu
            UIMenu childMenu = new UIMenu("Callouts", "Press ENTER to force the specified callout.");

            // create a new item in the main menu and bind the child menu to it
            {
                UIMenuItem bindItem = new UIMenuItem("Callouts");

                mainMenu.AddItem(bindItem);
                mainMenu.BindMenuToItem(childMenu, bindItem);


                // bindItem.RightBadge = UIMenuItem.BadgeStyle.Star;
                // mainMenu.OnIndexChange += (menu, index) => mainMenu.MenuItems[index].RightBadge = UIMenuItem.BadgeStyle.None;
            }

            // create the child menu items
            childMenu.AddItems(Enumerable.Range(1, 50).Select(i => new UIMenuItem($"Item #{i}")));

            // add all the menus to the pool
            pool.Add(mainMenu, childMenu);

            // start the fiber which will handle drawing and processing the menus
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
            // draw the menu banners (only needed if UIMenu.SetBannerType(Rage.Texture) is used)
            // Game.RawFrameRender += (s, e) => pool.DrawBanners(e.Graphics);

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