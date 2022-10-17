namespace DynamicCallouts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    using Rage;
    using Rage.Attributes;
    using Rage.Native;
    using RAGENativeUI;
    using RAGENativeUI.Elements;
    using RAGENativeUI.PauseMenu;

    internal static class StatsView
    {
        public static TabView tabView;
        public static TabTextItem textTab;

        private static uint handle;
        public static string textureStr;

        public static void Main()
        {
            tabView = new TabView("Dynamic Callouts Player Stats Overview");
            tabView.MoneySubtitle = Settings.OfficerName;
            tabView.Name = Settings.CallSign;
            //Photo logic
            GameFiber.Wait(7500);
            handle = NativeFunction.Natives.REGISTER_PEDHEADSHOT<uint>(Game.LocalPlayer.Character);
            GameFiber.Wait(5500);
            if (NativeFunction.Natives.IS_PEDHEADSHOT_READY<bool>(handle))
            {
                textureStr = NativeFunction.Natives.GET_PEDHEADSHOT_TXD_STRING<string>(handle);
            }
            tabView.Photo = new Sprite(textureStr, textureStr, System.Drawing.Point.Empty, System.Drawing.Size.Empty);

            tabView.AddTab(textTab = new TabTextItem("Stats", "Career Stats", "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights + "~n~ ~n~Deaths: " + Settings.Deaths));

            

            tabView.RefreshIndex();

            // start the fiber which will handle drawing and processing the pause menu
            GameFiber.StartNew(ProcessMenus);
        }

        private static void ProcessMenus()
        {
            Game.RawFrameRender += (s, e) => tabView.DrawTextures(e.Graphics);

            while (true)
            {
                GameFiber.Yield();

                tabView.Update();
            }
        }
    }
}