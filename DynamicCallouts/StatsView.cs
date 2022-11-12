namespace DynamicCallouts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using DynamicCallouts.Utilities;
    using Rage;
    using Rage.Attributes;
    using Rage.Native;
    using RAGENativeUI;
    using RAGENativeUI.Elements;
    using RAGENativeUI.PauseMenu;
    using DynamicCallouts;

    internal static class StatsView
    {
        public static TabView tabView;
        public static TabTextItem textTab;

        public static void Main()
        {
            tabView = new TabView("Dynamic Callouts Player Stats Overview");
            tabView.MoneySubtitle = Settings.OfficerName;
            tabView.Name = Settings.CallSign;
            //Photo logic
            tabView.Photo = new Sprite("3dtextures", "mpgroundlogo_cops", System.Drawing.Point.Empty, System.Drawing.Size.Empty);

            tabView.AddTab(textTab = new TabTextItem("Stats", "Career Stats", "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights));

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