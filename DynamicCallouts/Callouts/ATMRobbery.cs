using System;
using System.Collections.Generic;
using LSPD_First_Response.Engine.Scripting;
using System.Windows.Forms;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage.Native;
using System.Drawing;
using System.Collections;
using DynamicCallouts.Utilities;

namespace DynamicCallouts.Callouts
{
    [CalloutInfo("[DYNC] ATM Robbery", CalloutProbability.VeryHigh)]
    public class ATMRobbery : Callout
    {
        private string[] WeaponList = new string[] { "WEAPON_CROWBAR", "WEAPON_KNIFE", "WEAPON_PISTOL" };
        private string[] Peds = new string[] { "A_F_Y_Hippie_01", "A_M_Y_Skater_01", "A_M_M_FatLatin_01", "A_M_M_EastSA_01", "A_M_Y_Latino_01", "G_M_Y_FamDNF_01", "G_M_Y_FamCA_01", "G_M_Y_BallaSout_01", "G_M_Y_BallaOrig_01",
                                                  "G_M_Y_BallaEast_01", "G_M_Y_StrPunk_02", "S_M_Y_Dealer_01", "A_M_M_RurMeth_01", "A_M_Y_MethHead_01", "A_M_M_Skidrow_01", "S_M_Y_Dealer_01", "a_m_y_mexthug_01", "G_M_Y_MexGoon_03", "G_M_Y_MexGoon_02", "G_M_Y_MexGoon_01", "G_M_Y_SalvaGoon_01",
                                                  "G_M_Y_SalvaGoon_02", "G_M_Y_SalvaGoon_03", "G_M_Y_Korean_01", "G_M_Y_Korean_02", "G_M_Y_StrPunk_01" };

        public Vector3 SpawnPoint;
        public Vector3 SearchArea;

        public Ped Suspect;
        private Ped player = Game.LocalPlayer.Character;

        public Blip SuspectBlip;
        public Blip SearchAreaBlip;

        private bool HasBegunAttacking = false;
        private bool HasPursuitBegun = false;
        private bool HasPursuitBeenCreated = false;
        private bool CalloutRunning = false;
        private bool SuspectBlipCreated = false;
        private bool HasSuspectSurrended = false;

        private LHandle Pursuit;

        private int MainScenario;

        public override bool OnBeforeCalloutDisplayed()
        {
            Random rindum = new Random();
            List<Vector3> list = new List<Vector3>();
            Tuple<Vector3, float>[] SpawnLocationList =
            {
                Tuple.Create(new Vector3(-612.025f, -704.746f, 31.236f),174.819f),
                Tuple.Create(new Vector3(-614.513f, -704.370f, 31.236f),175.981f),
                Tuple.Create(new Vector3(-537.741f, -854.067f, 29.294f),175.677f),
                Tuple.Create(new Vector3(-712.964f, -819.124f, 23.723f),349.471f),
                Tuple.Create(new Vector3(-709.561f, -819.533f, 23.725f),0.439f),
                Tuple.Create(new Vector3(-203.365f, -861.716f, 30.268f),38.061f),
                Tuple.Create(new Vector3(1172.585f, 2702.419f, 38.175f),10.247f),
                Tuple.Create(new Vector3(1171.567f, 2702.450f, 38.175f),5.465f),
                Tuple.Create(new Vector3(-387.468f, 6046.016f, 31.500f),297.910f),
                Tuple.Create(new Vector3(-283.516f, 6225.709f, 31.494f),311.636f),
                Tuple.Create(new Vector3(-133.143f, 6366.625f, 31.477f),288.750f),
                Tuple.Create(new Vector3(-95.423f, 6457.142f, 31.460f),54.536f),
                Tuple.Create(new Vector3(-97.103f, 6455.540f, 31.466f),87.942f),
                Tuple.Create(new Vector3(1701.252f, 6426.565f, 32.764f),71.029f),
                Tuple.Create(new Vector3(1686.835f, 4815.847f, 42.008f),282.367f),
            };
            for (int i = 0; i < SpawnLocationList.Length; i++)
            {
                list.Add(SpawnLocationList[i].Item1);
            }
            int num = CallHandler.nearestLocationIndex(list);

            SpawnPoint = SpawnLocationList[num].Item1;

            Suspect = new Ped(Peds[new Random().Next((int)Peds.Length)], SpawnPoint, SpawnLocationList[num].Item2);

            MainScenario = new Random().Next(0, 3);

            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 20f);
            AddMinimumDistanceCheck(80f, SpawnPoint);
            AddMaximumDistanceCheck(600f, SpawnPoint);

            CalloutMessage = "[DYNC] ATM Robbery";
            CalloutPosition = SpawnPoint;
            CalloutAdvisory = "Suspect is Reported to have a ~r~weapon on him~w~.";

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
            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;
            Suspect.Armor = 175;
            Suspect.Inventory.GiveNewWeapon(new WeaponAsset(WeaponList[new Random().Next((int)WeaponList.Length)]), 500, true);

            SearchArea = SpawnPoint.Around2D(1f, 2f);
            SearchAreaBlip = new Blip(SearchArea, 20f);
            SearchAreaBlip.Color = Color.Yellow;
            SearchAreaBlip.EnableRoute(Color.Yellow);
            SearchAreaBlip.Alpha = 0.65f;
            SearchAreaBlip.Name = "Search Area";

            if (!CalloutRunning) Callout(); CalloutRunning = true;
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (Suspect) Suspect.Delete();
            if (SearchAreaBlip) SearchAreaBlip.Delete();
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
                        if (Suspect && Suspect.DistanceTo(player.GetOffsetPosition(Vector3.RelativeFront)) < 20f)
                        {
                            if (!SuspectBlipCreated)
                            {
                                SearchAreaBlip.Alpha = 0;
                                SearchAreaBlip.IsRouteEnabled = false;
                                SuspectBlip = new Blip(Suspect);
                                SuspectBlip.Name = "Suspect";
                                SuspectBlip.IsFriendly = false;
                                Game.DisplaySubtitle("~g~You:~w~ Hey you! Stop right there!");
                                SuspectBlipCreated = true;
                            }

                            if (MainScenario == 0 && !HasBegunAttacking)
                            {
                                new RelationshipGroup("SUSPECT");
                                new RelationshipGroup("PLAYER");

                                Suspect.RelationshipGroup = "SUSPECT";
                                player.RelationshipGroup = "PLAYER";

                                Game.SetRelationshipBetweenRelationshipGroups("SUSPECT", "PLAYER", Relationship.Hate);

                                Suspect.Tasks.FightAgainst(player, -1);

                                while (Suspect.Exists() && !Functions.IsPedArrested(Suspect) && Suspect.IsAlive)
                                {
                                    GameFiber.Yield();
                                }
                                if (Suspect.Exists())
                                {
                                    if (Functions.IsPedArrested(Suspect) || Suspect.IsAlive) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Attempting to ~r~Assault an Officer."); }
                                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was unfortunately ~r~Killed~w~ for ~r~Assaulting an Officer."); }
                                }
                                GameFiber.Wait(2000);
                                Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                                GameFiber.Wait(2000);

                                HasBegunAttacking = true;
                            }

                            if (MainScenario == 1 && !HasPursuitBeenCreated)
                            {
                                if (!HasPursuitBegun)
                                {
                                    Pursuit = Functions.CreatePursuit();
                                    if (Settings.AutomaticBackup)
                                    {
                                        Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                                    }
                                    Functions.AddPedToPursuit(Pursuit, Suspect);
                                    Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                                    HasPursuitBeenCreated = true;
                                    HasPursuitBegun = true;
                                    while (Functions.IsPursuitStillRunning(Pursuit)) { GameFiber.Wait(0); }
                                    if (Suspect.Exists())
                                    {
                                        if (Functions.IsPedArrested(Suspect)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Following the Pursuit."); }
                                        else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Pursuit."); }
                                    }
                                    GameFiber.Wait(2000);
                                    Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                                    GameFiber.Wait(2000);
                                }
                            }

                            else if (!HasSuspectSurrended)
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
                                    if (Functions.IsPedArrested(Suspect) || Suspect.IsAlive) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Attempting to ~r~Rob an ATM~w~."); }
                                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was unfortunately ~r~Killed~w~."); }
                                }
                                GameFiber.Wait(2000);
                                Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                                GameFiber.Wait(2000);

                                HasSuspectSurrended = true;
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
            if (SearchAreaBlip) SearchAreaBlip.Delete();
            if (SuspectBlip) SuspectBlip.Delete();
            if (Suspect) Suspect.Dismiss();
        }
    }
}
