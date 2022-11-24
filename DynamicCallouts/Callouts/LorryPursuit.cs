using LSPD_First_Response.Engine;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DynamicCallouts.Callouts
{
    [CalloutInfo("[DYNC] Lorry Pursuit", CalloutProbability.Medium)]
    public class LorryPursuit : Callout
    {
        private string[] TruckerList = new string[] { "TRAILERS", "TANKER" };

        private Vehicle Lorry;
        private Vehicle Tanker;

        private Ped Aggressor;
        private Ped player => Game.LocalPlayer.Character;

        private Vector3 Spawnpoint;
        private Vector3 tankerSpawnpoint;

        private Blip ABlip;

        private LHandle pursuit;

        private bool CalloutRunning = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            if (!Settings.LorryPursuit)
            {
                Game.LogTrivial("[LOG]: User has disabled LorryPursuit, returning false.");
                Game.LogTrivial("[LOG]: To enable the callout please check it in the menu or change false to true in the .ini file.");
                return false;
            }

            Spawnpoint = World.GetNextPositionOnStreet(player.Position.Around(350f));
            tankerSpawnpoint = World.GetNextPositionOnStreet(player.Position.Around(350f));

            ShowCalloutAreaBlipBeforeAccepting(Spawnpoint, 15f);
            AddMinimumDistanceCheck(100f, Spawnpoint);
            Functions.PlayScannerAudioUsingPosition("CRIME_OFFICER_IN_NEED_OF_ASSISTANCE_01 CRIME_RESIST_ARREST_02", Spawnpoint);

            CalloutMessage = "[DYNC] Lorry Pursuit";
            CalloutPosition = Spawnpoint;
            CalloutAdvisory = "A truck driver has gone crazy and is now in a pursuit.";

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutDisplayed()
        {
            if (Main.CalloutInterface)
            {
                CalloutInterfaceFunctions.SendCalloutDetails(this, "CODE 3", "LSPD");
            }
            else
            {
                Game.DisplayNotification("Respond with ~r~Code 3~w~.");
            }

            base.OnCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Aggressor = new Ped("s_m_m_trucker_01", Spawnpoint, 0f);
            Lorry = new Vehicle("PHANTOM", Spawnpoint);
            Tanker = new Vehicle(TruckerList[new Random().Next((int)TruckerList.Length)], tankerSpawnpoint);
            Aggressor.WarpIntoVehicle(Lorry, -1);
            Lorry.Trailer = Tanker;

            Settings.RespondedCallouts++;
            Settings.Stats.SelectSingleNode("Stats/RespondedCallouts").InnerText = Settings.RespondedCallouts.ToString();
            Settings.Stats.Save(Settings.xmlpath);
            Game.LogTrivial("RespondedCallouts changed new int: " + Settings.RespondedCallouts);
            StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights;

            ABlip = Aggressor.AttachBlip();
            pursuit = Functions.CreatePursuit();
            Functions.AddPedToPursuit(pursuit, Aggressor);
            Functions.SetPursuitIsActiveForPlayer(pursuit, true);
            if (Settings.AutomaticBackup)
            {
                Functions.RequestBackup(Spawnpoint, LSPD_First_Response.EBackupResponseType.Pursuit, LSPD_First_Response.EBackupUnitType.LocalUnit);
            }

            Settings.Pursuits++;
            Settings.Stats.SelectSingleNode("Stats/Pursuits").InnerText = Settings.Pursuits.ToString();
            Settings.Stats.Save(Settings.xmlpath);
            StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights;

            ABlip.EnableRoute(Color.Yellow);

            if (!CalloutRunning) { Callout(); } CalloutRunning = true;
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();

            if (Aggressor.Exists()) Aggressor.Delete();
            if (Lorry.Exists()) Lorry.Delete();
            if (ABlip.Exists()) ABlip.Delete();
            if (Tanker.Exists()) Tanker.Delete();
        }

        private void Callout()
        {
            CalloutRunning = true;
            GameFiber.StartNew(delegate
            {
                try
                {
                    while (CalloutRunning)
                    {
                        GameFiber.Yield();
                        if (!Functions.IsPursuitStillRunning(pursuit)) End();
                        if (player.IsDead) End();
                        if (Game.IsKeyDown(Settings.EndCall)) End();
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
            base.End();
            if (CalloutRunning)
            {
                Game.DisplayNotification("~g~Code 4~w~, return to patrol.");
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS WE_ARE_CODE_4");
            }
            if (Functions.IsPursuitStillRunning(pursuit)) Functions.ForceEndPursuit(pursuit);

            CalloutRunning = false;
            if (ABlip.Exists()) ABlip.Delete();
            if (Lorry.Exists()) Lorry.Dismiss();
            if (Tanker.Exists()) Tanker.Dismiss();
            if (Aggressor.Exists()) Aggressor.Dismiss();
        }
    }
}
