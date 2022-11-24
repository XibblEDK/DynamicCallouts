using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.API;

namespace DynamicCallouts.Callouts
{
    [CalloutInfo("[DYNC] Gunshots Reported", CalloutProbability.Medium)]
    public class GunshotsReported : Callout
    {
        private string[] WeaponList = new string[] { "WEAPON_PISTOL", "WEAPON_COMBATPISTOL", "WEAPON_SNSPISTOL", "WEAPON_HEAVYPISTOL", "WEAPON_VINTAGEPISTOL", "WEAPON_CERAMICPISTOL" };
        private string[] Peds = new string[] { "A_F_Y_Hippie_01", "A_M_Y_Skater_01", "A_M_M_FatLatin_01", "A_M_M_EastSA_01", "A_M_Y_Latino_01", "G_M_Y_FamDNF_01", "G_M_Y_FamCA_01", "G_M_Y_BallaSout_01", "G_M_Y_BallaOrig_01",
                                                  "G_M_Y_BallaEast_01", "G_M_Y_StrPunk_02", "S_M_Y_Dealer_01", "A_M_M_RurMeth_01", "A_M_Y_MethHead_01", "A_M_M_Skidrow_01", "S_M_Y_Dealer_01", "a_m_y_mexthug_01", "G_M_Y_MexGoon_03", "G_M_Y_MexGoon_02", "G_M_Y_MexGoon_01", "G_M_Y_SalvaGoon_01",
                                                  "G_M_Y_SalvaGoon_02", "G_M_Y_SalvaGoon_03", "G_M_Y_Korean_01", "G_M_Y_Korean_02", "G_M_Y_StrPunk_01" };

        private Ped Suspect;
        private Ped DeadPed;
        private Ped player => Game.LocalPlayer.Character;

        private Blip SuspectBlip;

        private Vector3 Spawnpoint;

        private bool CalloutRunning = false;
        private bool IsGoingToSurrender;
        private bool HasSurrended = false;
        private bool HasPursuitBegun = false;
        private bool HasBegunAttacking = false;

        private int SuspectAction;

        private LHandle Pursuit;

        public override bool OnBeforeCalloutDisplayed()
        {
            if (!Settings.GunshotsReported)
            {
                Game.LogTrivial("[LOG]: User has disabled GunshotsReported, returning false.");
                Game.LogTrivial("[LOG]: To enable the callout please check it in the menu or change false to true in the .ini file.");
                return false;
            }

            Spawnpoint = World.GetNextPositionOnStreet(player.Position.Around(1000f));

            ShowCalloutAreaBlipBeforeAccepting(Spawnpoint, 30f);
            AddMinimumDistanceCheck(30f, Spawnpoint);

            var random = new Random();
            IsGoingToSurrender = random.Next(4) == 1;

            var r = new Random();
            SuspectAction = r.Next(2);

            CalloutMessage = "[DYNC] Gunshots Reported";
            CalloutPosition = Spawnpoint;
            CalloutAdvisory = "Gunshots have been reported, respond as fast as possible.";
            Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS_01 WE_HAVE CRIME_GUNFIRE_02 IN_OR_ON_POSITION UNITS_RESPOND_CODE_03_01", Spawnpoint);

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
            Suspect = new Ped(Peds[new Random().Next((int)Peds.Length)], Spawnpoint, 0);
            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;
            Suspect.Inventory.GiveNewWeapon(WeaponList[new Random().Next((int)WeaponList.Length)], 75, true);
            Suspect.Tasks.Wander();

            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.Color = Color.Red;
            SuspectBlip.IsRouteEnabled = true;
            SuspectBlip.RouteColor = Color.Yellow;

            Settings.RespondedCallouts++;
            Settings.Stats.SelectSingleNode("Stats/RespondedCallouts").InnerText = Settings.RespondedCallouts.ToString();
            Settings.Stats.Save(Settings.xmlpath);
            Game.LogTrivial("RespondedCallouts changed new int: " + Settings.RespondedCallouts);
            StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights;

            new RelationshipGroup("Citizens");
            new RelationshipGroup("Suspect");
            Suspect.RelationshipGroup = "Suspect";
            World.GetAllPeds().ToList().ForEach(p =>
            {
                if (p.DistanceTo(Suspect) > 80f) p.Tasks.Flee(Suspect, 10f, -1);
                p.RelationshipGroup = "Citizens";
            });
            player.Tasks.ClearImmediately();
            Game.SetRelationshipBetweenRelationshipGroups("Suspect", "Citizens", Relationship.Hate);
            Suspect.Tasks.FightAgainstClosestHatedTarget(120f);

            DeadPed = new Ped(Suspect.Position.Around(8f));
            DeadPed.BlockPermanentEvents = true;
            DeadPed.IsPersistent = true;
            DeadPed.Tasks.StandStill(-1);
            DeadPed.Kill();

            if (!CalloutRunning) Callout(); CalloutRunning = true;
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (Suspect) Suspect.Delete();
            if (SuspectBlip) SuspectBlip.Delete();
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
                        if (Suspect && Suspect.DistanceTo(player.GetOffsetPosition(Vector3.RelativeFront)) <= 20f)
                        {
                            if (IsGoingToSurrender && !HasSurrended)
                            {
                                Suspect.Tasks.ClearImmediately();
                                Suspect.Inventory.EquippedWeapon.DropToGround();
                                Suspect.Tasks.PutHandsUp(-1, player);
                                while (Suspect.Exists() && !Functions.IsPedArrested(Suspect) && Suspect.IsAlive)
                                {
                                    GameFiber.Yield();
                                }
                                if (Suspect.Exists())
                                {
                                    if (Functions.IsPedArrested(Suspect) && Suspect.IsAlive) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~."); Settings.Arrests++; Settings.Stats.SelectSingleNode("Stats/Arrests").InnerText = Settings.Arrests.ToString(); Settings.Stats.Save(Settings.xmlpath); StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights; }
                                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was unfortunately ~r~Killed~w~."); }
                                }
                                GameFiber.Wait(2000);
                                Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                                GameFiber.Wait(2000);

                                HasSurrended = true;
                            }
                            else if (!IsGoingToSurrender && SuspectAction == 0 && !HasBegunAttacking)
                            {
                                Suspect.Tasks.ClearImmediately();
                                Suspect.Tasks.FightAgainst(player);
                                Settings.InvolvedInFights++;
                                Settings.Stats.SelectSingleNode("Stats/InvolvedInFights").InnerText = Settings.InvolvedInFights.ToString();
                                Settings.Stats.Save(Settings.xmlpath);
                                StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights;
                                while (Suspect.Exists() && !Functions.IsPedArrested(Suspect) && Suspect.IsAlive)
                                {
                                    GameFiber.Yield();
                                }
                                if (Suspect.Exists())
                                {
                                    if (Functions.IsPedArrested(Suspect) && Suspect.IsAlive) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Attempting to ~r~Assault an Officer."); Settings.Arrests++; Settings.Stats.SelectSingleNode("Stats/Arrests").InnerText = Settings.Arrests.ToString(); Settings.Stats.Save(Settings.xmlpath); StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights; }
                                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was unfortunately ~r~Killed~w~ for ~r~Assaulting an Officer."); }
                                }
                                GameFiber.Wait(2000);
                                Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                                GameFiber.Wait(2000);

                                HasBegunAttacking = true;
                            }
                            else if (!IsGoingToSurrender && SuspectAction == 1 && !HasPursuitBegun)
                            {
                                Settings.Pursuits++;
                                Settings.Stats.SelectSingleNode("Stats/Pursuits").InnerText = Settings.Pursuits.ToString();
                                Settings.Stats.Save(Settings.xmlpath);
                                StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights;
                                Pursuit = Functions.CreatePursuit();
                                if (Settings.AutomaticBackup)
                                {
                                    Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                                }
                                Functions.AddPedToPursuit(Pursuit, Suspect);
                                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                                HasPursuitBegun = true;
                                while (Functions.IsPursuitStillRunning(Pursuit)) { GameFiber.Wait(0); }
                                if (Suspect.Exists())
                                {
                                    if (Functions.IsPedArrested(Suspect)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Following the Pursuit."); Settings.Arrests++; Settings.Stats.SelectSingleNode("Stats/Arrests").InnerText = Settings.Arrests.ToString(); Settings.Stats.Save(Settings.xmlpath); StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights; }
                                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Pursuit."); }
                                }
                                GameFiber.Wait(2000);
                                Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                                GameFiber.Wait(2000);
                            }
                        }
                        if (Suspect && Suspect.IsDead) End();
                        if (Functions.IsPedArrested(Suspect)) End();
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

            CalloutRunning = false;
            if (SuspectBlip) SuspectBlip.Delete();
            if (Suspect) Suspect.Dismiss();
        }
    }
}
