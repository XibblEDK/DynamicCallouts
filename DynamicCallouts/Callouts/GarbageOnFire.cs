using DynamicCallouts.Utilities;
using LSPD_First_Response.Engine;
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

namespace DynamicCallouts.Callouts
{
    [CalloutInfo("[DYNC] Garbage on Fire", CalloutProbability.High)]
    public class GarbageOnFire : Callout
    {
        private Random random = new Random();

        private Ped Suspect;
        private Ped player => Game.LocalPlayer.Character;

        private List<Vector3> locations = new List<Vector3>() { new Vector3(-857.6f, -240.9f, 39.5f),
                                                                new Vector3(-1165.1f, -1396.4f, 4.9f),
                                                                new Vector3(1057.8f, -787.16f, 58.26f),
                                                                new Vector3(129.1f, -1486.8f, 29.14f),
                                                                new Vector3(2543.4f, 341.0f, 108.5f),
                                                                new Vector3(-2954.0f, 445.75f, 15.28f),
                                                                new Vector3(1534.0f, 3610.7f, 35.35f),
                                                                new Vector3(1639.2f, 4820.9f, 41.9f),
                                                                new Vector3(-256.417f, 6246.083f, 32.57662f)};
        private List<uint> FireList = new List<uint>();

        private string[] weaponList = new string[] { "weapon_molotov", "weapon_petrolcan" };

        private Vector3 Spawnpoint;
        private Vector3 Area;

        private Blip LocationBlip;
        private Blip SuspectBlip;

        private LHandle pursuit;

        private bool CalloutRunning = false;
        private bool PursuitCreated = false;
        public bool HasBegunAttacking = false;

        private uint Fire;
        private int SuspectAction;

        public override bool OnBeforeCalloutDisplayed()
        {
            if (!Settings.GarbageOnFire)
            {
                Game.LogTrivial("[LOG]: User has disabled GarbageOnFire, returning false.");
                Game.LogTrivial("[LOG]: To enable the callout please check it in the menu or change false to true in the .ini file.");
                return false;
            }

            int decision;
            float offsetx, offsety, offsetz;
            Vector3 PedSpawn;

            Spawnpoint = CallHandler.chooseNearestLocation(locations);

            ShowCalloutAreaBlipBeforeAccepting(Spawnpoint, 30f);
            AddMinimumDistanceCheck(100f, Spawnpoint);

            CalloutMessage = "[DYNC] Garbage on Fire";
            CalloutPosition = Spawnpoint;
            CalloutAdvisory = "Citizens have reported some garbage on fire.";

            for (int f = 1; f < 11; f++)
            {
                decision = random.Next(0, 6);
                offsetx = decision * f / 100;
                decision = random.Next(0, 6);
                offsety = decision * f / 100;
                decision = random.Next(0, 6);
                offsetz = decision * f / 100;

                decision = random.Next(0, 2);
                if (decision == 0)
                {
                    offsetx = -offsetx;
                }
                decision = random.Next(0, 2);
                if (decision == 0)
                {
                    offsetz = -offsetz;
                }

                Fire = NativeFunction.Natives.StartScriptFire<uint>(Spawnpoint.X + offsetx, Spawnpoint.Y + offsety, Spawnpoint.Z + offsetz, 25, true);

                FireList.Add(Fire);
            }

            PedSpawn = Spawnpoint;
            PedSpawn.Z += 1f;

            Suspect = new Ped(PedSpawn)
            {
                IsFireProof = true,
                IsPersistent = true,
                BlockPermanentEvents = true
            };
            Suspect.Tasks.Wander();

            decision = random.Next(0, 1);
            if (decision > 0)
            {
                decision = random.Next(0, weaponList.Length);
                if (decision == 2)
                {
                    Suspect.Inventory.GiveNewWeapon(new WeaponAsset(weaponList[decision]), 1, true);
                } else {
                    Suspect.Inventory.GiveNewWeapon(new WeaponAsset(weaponList[decision]), 16, true);
                }
            }

            Functions.PlayScannerAudioUsingPosition("ASSISTANCE_REQUIRED IN_OR_ON_POSITION", Spawnpoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutDisplayed()
        {
            if (Main.CalloutInterface)
            {
                CalloutInterfaceFunctions.SendCalloutDetails(this, "CODE 2", "LSPD");
            }
            else
            {
                Game.DisplayNotification("Respond with ~y~Code 2~w~.");
            }

            base.OnCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Area = Spawnpoint.Around2D(1f, 2f);
            LocationBlip = new Blip(Area, 40f)
            {
                Color = Color.Yellow
            };
            LocationBlip.EnableRoute(Color.Yellow);

            Settings.RespondedCallouts++;
            Settings.Stats.SelectSingleNode("Stats/RespondedCallouts").InnerText = Settings.RespondedCallouts.ToString();
            Settings.Stats.Save(Settings.xmlpath);
            Game.LogTrivial("RespondedCallouts changed new int: " + Settings.RespondedCallouts);
            StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights;

            SuspectAction = random.Next(0, 2);
            //Game.LogTrivial(SuspectAction.ToString());

            if (!CalloutRunning) Callout(); CalloutRunning = true;
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (Suspect) Suspect.Delete();
            if (LocationBlip) LocationBlip.Delete();

            foreach (uint f in FireList)
            {
                NativeFunction.Natives.RemoveScriptFire(f);
            }

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
                        if (Suspect.Exists() && Suspect.DistanceTo(player.GetOffsetPosition(Vector3.RelativeFront)) < 35f)
                        {
                            Suspect.KeepTasks = true;
                            if (LocationBlip.Exists()) LocationBlip.Delete();
                            GameFiber.Wait(2000);
                        }

                        if (SuspectAction == 1 && Suspect.Exists() && Suspect.DistanceTo(player.GetOffsetPosition(Vector3.RelativeFront)) < 35) 
                        {
                            SuspectBlip = new Blip(Suspect);
                            SuspectBlip.IsFriendly = false;
                            SuspectBlip.Name = "Suspect";
                        }


                        NativeFunction.CallByName<uint>("TASK_REACT_AND_FLEE_PED", Suspect);

                        if (!PursuitCreated && Suspect.DistanceTo(player.GetOffsetPosition(Vector3.RelativeFront)) < 30f && SuspectAction == 0)
                        {
                            Settings.Pursuits++;
                            Settings.Stats.SelectSingleNode("Stats/Pursuits").InnerText = Settings.Pursuits.ToString();
                            Settings.Stats.Save(Settings.xmlpath);
                            StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights;
                            pursuit = Functions.CreatePursuit();
                            Functions.AddPedToPursuit(pursuit, Suspect);
                            Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                            if (Settings.AutomaticBackup)
                            {
                                Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                            }
                            while (Functions.IsPursuitStillRunning(pursuit)) { GameFiber.Wait(0); }
                            if (Suspect.Exists())
                            {
                                if (Functions.IsPedArrested(Suspect) && Suspect.IsAlive) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Following the Pursuit."); Settings.Arrests++; Settings.Stats.SelectSingleNode("Stats/Arrests").InnerText = Settings.Arrests.ToString(); Settings.Stats.Save(Settings.xmlpath); StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights; }
                                else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Pursuit."); }
                            }
                            GameFiber.Wait(2000);
                            Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                            PursuitCreated = true;
                        }

                        if (!HasBegunAttacking && Suspect.DistanceTo(player.GetOffsetPosition(Vector3.RelativeFront)) < 30f && SuspectAction == 1)
                        {
                            Suspect.Tasks.ClearImmediately();
                            Settings.InvolvedInFights++;
                            Settings.Stats.SelectSingleNode("Stats/InvolvedInFights").InnerText = Settings.InvolvedInFights.ToString();
                            Settings.Stats.Save(Settings.xmlpath);
                            StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights;
                            Suspect.Tasks.FightAgainst(player);
                            Game.DisplaySubtitle("~r~Suspect:~w~ I hate this country, and I hate you!");
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
            if (LocationBlip) LocationBlip.Delete();
            if (SuspectBlip.Exists()) SuspectBlip.Delete();
            if (Suspect) Suspect.Dismiss();
        }
    }
}
