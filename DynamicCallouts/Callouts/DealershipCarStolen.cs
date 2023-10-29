using DynamicCallouts.Utilities;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicCallouts.Callouts
{
    //[CalloutInfo("[DYNC] Dealership Car Stolen", CalloutProbability.High)]
    public class DealershipCarStolen : Callout
    {
        private Ped player => Game.LocalPlayer.Character;

        private Ped CarDealerMan;

        private Vector3 calloutLocation = new Vector3(-58.161064f, -1099.174f, 25.10041f);

        private Blip CarDealerManBlip;

        private LHandle pursuit;

        private bool CalloutRunning = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            ShowCalloutAreaBlipBeforeAccepting(calloutLocation, 75f);
            CalloutMessage = "[DYNC] Dealership Car Stolen";
            CalloutAdvisory = "Coming soon";
            CalloutPosition = calloutLocation;
            //Functions.PlayScannerAudioUsingPosition("", calloutLocation);

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutDisplayed()
        {
            if (Main.CalloutInterface)
            {
                //CalloutInterfaceFunctions.SendCalloutDetails(this, "CODE 3");
            }
            else
            {
                Game.DisplayNotification("Respond with ~r~Code 3~w~.");
            }

            base.OnCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Settings.RespondedCallouts++;
            Settings.Stats.SelectSingleNode("Stats/RespondedCallouts").InnerText = Settings.RespondedCallouts.ToString();
            Settings.Stats.Save(Settings.xmlpath);
            Game.LogTrivial("RespondedCallouts changed new int: " + Settings.RespondedCallouts);
            StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights;

            NativeFunction.Natives.REQUEST_IPL("carshowroom_boarded");

            CarDealerManBlip = new Blip(calloutLocation, 20f);
            CarDealerManBlip.Color = Color.Yellow;
            CarDealerManBlip.EnableRoute(Color.Yellow);
            CarDealerManBlip.Alpha = 0.65f;
            CarDealerManBlip.Name = "Car Dealer";

            if (!CalloutRunning) Callout(); CalloutRunning = true;
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
        }

        public void Callout()
        {
            CalloutRunning = true;
            GameFiber.StartNew(delegate
            {
                try
                {
                    while (CalloutRunning)
                    {
                        GameFiber.Yield();
                        if (Game.IsKeyDown(Keys.E))
                        {
                            player.Position = calloutLocation;
                            GameFiber.Sleep(-1);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (CalloutRunning)
                    {
                        Game.LogTrivial("IN: " + this);
                        string error = e.ToString();
                        Game.LogTrivial("ERROR: " + error);
                    }
                    else
                    {
                        string error = e.ToString();
                        Game.LogTrivial("ERROR: " + error);
                    }
                    End();
                }
            });
        }

        public override void End()
        {
            CallHandler.SafelyDelete(CarDealerMan);
            CallHandler.SafelyDelete(CarDealerManBlip);

            NativeFunction.Natives.REMOVE_IPL("carshowroom_boarded");

            CalloutRunning = false;
            base.End();
        }
    }
}
