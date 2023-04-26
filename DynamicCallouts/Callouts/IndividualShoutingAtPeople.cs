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
using DynamicCallouts.VersionChecker;
using System.IO;

namespace DynamicCallouts.Callouts
{
    [CalloutInfo("[DYNC] Individual Shouting at People", CalloutProbability.High)]

    public class IndividualShoutingAtPeople : Callout
    {
        // Spawn points (Only spawns in city)
        ArrayList list = new ArrayList()
        {
        new Vector3(-493.170f, 130.356f, 38.933f),
        new Vector3(-356.814f, -212.564f, 37.278f),
        new Vector3(-260.493f, -138.198f, 43.785f),
        new Vector3(-225.193f, -41.198f, 49.598f),
        new Vector3(-60.921f, -81.378f, 57.893f),
        new Vector3(210.035f, -243.059f, 53.652f),
        new Vector3(154.011f, -477.123f, 43.036f),
        new Vector3(84.967f, -803.226f, 31.503f),
        new Vector3(284.516f, -913.002f, 29.038f),
        new Vector3(493.308f, -916.691f, 26.247f),
        new Vector3(516.083f, -1471.168f, 29.276f),
        new Vector3(511.286f, -1883.120f, 25.657f),
        new Vector3(179.376f, -1888.378f, 23.957f),
        new Vector3(58.887f, -1647.748f, 29.301f),
        new Vector3(-294.516f, -1410.883f, 31.301f),
        new Vector3(-555.914f, -1145.163f, 19.926f),
        new Vector3(-673.670f, -1290.076f, 10.624f),
        new Vector3(-835.219f, -1137.953f, 7.587f),
        new Vector3(-1094.357f, -1383.851f, 5.179f),
        new Vector3(-1245.519f, -1306.211f, 3.930f),
        new Vector3(-1235.889f, -1063.394f, 8.352f),
        new Vector3(-1307.430f, -849.375f, 16.193f),
        new Vector3(-1356.362f, -679.268f, 25.462f),
        new Vector3(-1414.431f, -393.054f, 36.247f),
        new Vector3(-1678.912f, -580.852f, 33.832f),
        new Vector3(-1690.949f, -394.028f, 47.365f),
        new Vector3(-1712.105f, -9.471f, 64.629f),
        new Vector3(-1296.094f, 390.206f, 71.771f),
        new Vector3(-902.434f, 257.104f, 70.598f),
        new Vector3(-528.826f, 270.529f, 82.969f)
        };

        private Vector3 VictimSpawnPoint;
        private String Zone;
        private String[] Suspects;

        private Blip VictimBlip;
        private Blip SuspectBlip;
        private Blip SuspectArea;

        private Ped Victim;
        private Ped Suspect;
        private Ped player => Game.LocalPlayer.Character;

        private int MainScenario;
        private int SuspectAction;

        private bool CalloutRunning = false;

        private LHandle MainPursuit;

        private readonly List<string> CallerDialogue1 = new List<string>()
        {
            "~b~Victim:~w~ Here! Hello officer.",
            "~g~You:~w~ Hello, can you tell me what happened?",
            "~b~Victim:~w~ Alright, here's what happened.",
            "~b~Victim:~w~ Some random man just got in my face and started shouting and swearing at me.",
            "~g~You:~w~ Did he hurt you in any kind of way other than verbally?",
            "~b~Victim:~w~ No, he did not.",
            "~g~You:~w~ Okay good, did you see where he went? And what does he look like?",
            "~b~Victim:~w~ I can't really remember, but I think I'd recognize him if I saw him again. Maybe I could help search?",
        };

        private readonly List<string> CallerDialogue2 = new List<string>()
        {
            "~b~Victim:~w~ Hi officer.",
            "~g~You:~w~ What happened?",
            "~b~Victim:~w~ I was just walking down the street when this man got in my face.",
            "~b~Victim:~w~ He started Swearing at me for no reason, and he actually punched me.",
            "~g~You:~w~ Alright, do you need any medical assistance?",
            "~b~Victim:~w~ Nah, I'm fine. But I really want to catch the guy!",
            "~g~You:~w~ Do you remember what he looked like, and where did he go?",
            "~b~Victim:~w~ I don't really remember, but I'd recognize him if I saw Him again.",
        };

        private readonly List<string> CallerDialogue3 = new List<string>()
        {
            "~b~Victim:~w~ Here, hi officer!",
            "~g~You:~w~ Describe the problem.",
            "~b~Victim:~w~ I was minding my own business but then a man started yelling at me.",
            "~b~Victim:~w~ He swore and shouted at me, I tried to leave, but he just started punching me.",
            "~g~You:~w~ Need any medical attention?",
            "~b~Victim:~w~ I'm fine, but I'd like to catch the guy. He probably thought he could get away with it.",
            "~g~You:~w~ What did he look like?",
            "~b~Victim:~w~ I can't remember, but maybe I'd recognize Him if I saw Him. Can I help find him?",
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            if (!Settings.IndividualShoutingAtPeople)
            {
                Game.LogTrivial("[LOG]: User has disabled IndividualShoutingAtPeople, returning false.");
                Game.LogTrivial("[LOG]: To enable the callout please check it in the menu or change false to true in the .ini file.");
                return false;
            }

            Zone = Functions.GetZoneAtPosition(Game.LocalPlayer.Character.Position).GameName;
            CallHandler.locationChooser(list);
            if (CallHandler.locationReturned) { VictimSpawnPoint = CallHandler.SpawnPoint; } else { return false; }
            ShowCalloutAreaBlipBeforeAccepting(VictimSpawnPoint, 75f);
            AddMinimumDistanceCheck(30f, VictimSpawnPoint);
            AddMaximumDistanceCheck(600f, VictimSpawnPoint);
            Functions.PlayScannerAudio("CITIZENS_REPORT_01 CRIME_DISTURBING_THE_PEACE_01");
            CalloutMessage = "[DYNC] Individual Shouting At People";
            CalloutPosition = VictimSpawnPoint;
            CalloutAdvisory = "Suspect is Reported to have ~y~Aggresively Shouted~w~ at Multiple Citizens.";

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutDisplayed()
        {
            if (Main.CalloutInterface)
            {
                CalloutInterfaceFunctions.SendCalloutDetails(this, "CODE 1", "");
            }
            else
            {
                Game.DisplayNotification("Respond with ~g~Code 1~w~.");
            }

            base.OnCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Victim = new Ped(VictimSpawnPoint);
            Victim.IsPersistent = true;
            Victim.BlockPermanentEvents = true;
            Victim.IsInvincible = true;
            VictimBlip = Victim.AttachBlip();
            VictimBlip.IsFriendly = true;
            VictimBlip.IsRouteEnabled = true;
            VictimBlip.Scale = 0.65f;
            VictimBlip.Name = "Victim";

            System.Random rYUY = new System.Random();
            SuspectAction = rYUY.Next(0, 3);

            Settings.RespondedCallouts++;
            Settings.Stats.SelectSingleNode("Stats/RespondedCallouts").InnerText = Settings.RespondedCallouts.ToString();
            Settings.Stats.Save(Settings.xmlpath);
            Game.LogTrivial("RespondedCallouts changed new int: " + Settings.RespondedCallouts);
            StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights;


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
                        while (player.DistanceTo(Victim) >= 15f && !Game.IsKeyDown(Settings.EndCall)) GameFiber.Wait(0);
                        if (Main.CalloutInterface) CalloutInterfaceFunctions.SendMessage(this, Settings.CallSign + " Arrived on Scene. Talking to Victim");
                        if (MainScenario == 0) AssaultOpening();
                        break;
                    }
                    End();
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

        private void AssaultOpening()
        {
            if (CalloutRunning)
            {
                Game.DisplayHelp("Speak to the ~b~Victim");

                NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Victim, player, -1);
                while (player.DistanceTo(Victim) >= 5f) GameFiber.Wait(0);
                Game.DisplayHelp("Press ~y~" + Settings.Dialog + " ~w~to Speak with the ~b~Victim");
                while (!Game.IsKeyDown(Settings.Dialog)) GameFiber.Wait(0);

                System.Random r = new System.Random();
                int OpeningDialogue = r.Next(0, 3);
                if (OpeningDialogue == 0)
                {
                    CallHandler.Dialogue(CallerDialogue1, Victim);
                }
                else if (OpeningDialogue == 1)
                {
                    CallHandler.Dialogue(CallerDialogue2, Victim);
                }
                else
                {
                    CallHandler.Dialogue(CallerDialogue3, Victim);
                }
                Victim.Tasks.ClearImmediately();

                NativeFunction.Natives.xA0F8A7517A273C05<Vector3>(World.GetNextPositionOnStreet(player.Position.Around(69)), 360, out Vector3 SuspectSpawn);
                Suspects = new string[8] { "A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01" };
                System.Random r2 = new System.Random();
                int SuspectModel = r2.Next(0, Suspects.Length);
                Suspect = new Ped(Suspects[SuspectModel], SuspectSpawn, 69);
                try
                {
                    Suspect.IsPersistent = true;
                    Suspect.BlockPermanentEvents = true;
                    Suspect.IsVisible = false;
                }
                catch (Rage.Exceptions.InvalidHandleableException)
                {
                    End();
                }

                Game.DisplayHelp("~y~" + Settings.InteractionKey1 + ":~b~ Let the Victim Help You Search for the Suspect. ~y~" + Settings.InteractionKey2 + ":~b~ Search for the Suspect Yourself.", true);
                CallHandler.IdleAction(Victim, false);
                while (!Game.IsKeyDown(Settings.InteractionKey1) && !Game.IsKeyDown(Settings.InteractionKey2)) GameFiber.Wait(0);
                Victim.Tasks.ClearImmediately();
                if (Game.IsKeyDown(Settings.InteractionKey1)) { Game.HideHelp(); Follow(); }
                else if (Game.IsKeyDown(Settings.InteractionKey2)) { Game.HideHelp(); Search(); }
            }
        }

        private void Follow()
        {
            if (CalloutRunning)
            {
                Game.DisplaySubtitle("~g~You:~w~ Sure, It Would Help a lot if You Were There to Help Me. Hop In!", 3000);
                GameFiber.Wait(3500);
                Game.DisplayHelp("Get in your ~g~Vehicle.");
                if (Main.CalloutInterface) CalloutInterfaceFunctions.SendMessage(this, "Victim is helping to finding the Suspect.");

                while (!Game.LocalPlayer.Character.IsInAnyPoliceVehicle) { GameFiber.Wait(0); }
                Game.DisplayHelp("~y~" + Settings.InteractionKey1 + ": ~b~ Tell the Victim to Enter the Passenger Seat. ~y~" + Settings.InteractionKey2 + ":~b~ Tell the Victim to Enter the Rear Seat.", true);
                while (!Game.IsKeyDown(Settings.InteractionKey1) && !Game.IsKeyDown(Settings.InteractionKey2)) GameFiber.Wait(0);
                int SeatIndex;
                if (Game.IsKeyDown(Settings.InteractionKey1))
                {
                    Game.HideHelp();
                    SeatIndex = (int)Game.LocalPlayer.Character.CurrentVehicle.GetFreePassengerSeatIndex();
                    Victim.Tasks.EnterVehicle(Game.LocalPlayer.Character.CurrentVehicle, SeatIndex, EnterVehicleFlags.None).WaitForCompletion();
                }
                else
                {
                    Game.HideHelp();
                    SeatIndex = (int)Game.LocalPlayer.Character.CurrentVehicle.GetFreeSeatIndex(1, 2);
                    Victim.Tasks.EnterVehicle(Game.LocalPlayer.Character.CurrentVehicle, SeatIndex, EnterVehicleFlags.None).WaitForCompletion();
                }
                if (VictimBlip.Exists()) { VictimBlip.Delete(); }
                Suspect.IsVisible = false;

                Game.DisplayHelp("Start ~o~Searching~w~ for the ~r~Suspect.");
                SuspectArea = new Blip(Suspect.Position.Around(15), 250);
                SuspectArea.Color = Color.Orange;
                SuspectArea.Alpha = 0.5f;
                GameFiber.Wait(1500);

                System.Random coco = new System.Random();
                int WaitTime = coco.Next(20000, 40000);
                GameFiber.Wait(WaitTime);

                Suspect.Position = World.GetNextPositionOnStreet(Victim.Position.Around(45));
                Suspect.IsVisible = true;
                Suspect.Tasks.Wander();
                Game.DisplaySubtitle("~b~Victim:~w~ Officer I See the ~r~Suspect~w~! He's Right Over There!", 2500);
                GameFiber.Wait(2000);
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.IsFriendly = false;
                SuspectBlip.Scale = 0.75f;
                SuspectArea.Delete();
                Game.DisplayHelp("Arrest or Fine the ~r~Suspect.");
                while (player.DistanceTo(Suspect) >= 5) GameFiber.Wait(0);
                Game.DisplaySubtitle("~g~You:~w~ Hey You, I Need to Speak With You!", 2500);
                GameFiber.Wait(1000);
                SuspectEnding();
            }
        }

        private void Search()
        {
            if (CalloutRunning)
            {
                Game.DisplaySubtitle("~g~You:~w~ No Sorry, I Can't Let You Come With Me. I'll Search for the Suspect Based on Your Information.", 3500);
                GameFiber.Wait(3500);
                Victim.Dismiss();
                if (VictimBlip.Exists()) VictimBlip.Delete();

                Game.DisplayHelp("Start ~o~Searching~w~ for the ~r~Suspect.");
                if (Main.CalloutInterface) { CalloutInterfaceFunctions.SendMessage(this, "Searching for the Suspect."); }
                SuspectArea = new Blip(Suspect.Position.Around(15), 250);
                SuspectArea.Color = Color.Orange;
                SuspectArea.Alpha = 0.5f;

                System.Random coco = new System.Random();
                int WaitTime = coco.Next(35000, 69000);
                GameFiber.Wait(WaitTime);

                Suspect.Position = World.GetNextPositionOnStreet(player.Position.Around(100));
                Suspect.IsVisible = true;
                Suspect.Tasks.Wander();
                Game.DisplaySubtitle("~g~You:~w~ Hm, That Looks Like the Suspect Right There!", 2500);
                GameFiber.Wait(2000);
                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.IsFriendly = false;
                SuspectBlip.Scale = 0.75f;
                SuspectArea.Delete();
                Game.DisplayHelp("Arrest or Fine the ~r~Suspect.");
                while (player.DistanceTo(Suspect) >= 5) GameFiber.Wait(0);
                Game.DisplaySubtitle("~g~You:~w~ Hey Sir, I need to Speak with You!", 2500);
                GameFiber.Wait(1000);
                SuspectEnding();
            }
        }

        private void SuspectEnding()
        {
            if (CalloutRunning)
            {
                if (SuspectAction == 0)
                {
                    Game.DisplayHelp("Arrest or Fine the ~r~Suspect.");
                    Suspect.Tasks.Clear();
                    while (Suspect.Exists())
                    {
                        GameFiber.Yield();
                        if (Suspect.IsDead || Functions.IsPedStoppedByPlayer(Suspect) || !Suspect.Exists()) break;
                    }
                    if (Suspect.Exists())
                    {
                        if (Suspect.IsDead) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Suspect was unfortunately ~r~Killed~w~."); }
                    }
                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, Suspect was unfortunately ~r~Killed~w~."); }

                    GameFiber.Wait(2000);
                    Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                    GameFiber.Wait(2000);
                }
                else if (SuspectAction == 1) Fight();
                else Flee();
            }
        }

        private void Fight()
        {
            if (CalloutRunning)
            {
                Settings.InvolvedInFights++;
                Settings.Stats.SelectSingleNode("Stats/InvolvedInFights").InnerText = Settings.InvolvedInFights.ToString();
                Settings.Stats.Save(Settings.xmlpath);
                StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights;

                Suspect.Tasks.ClearImmediately();
                Suspect.Inventory.Weapons.Clear();
                GameFiber.Wait(500);
                Suspect.Tasks.FightAgainst(Game.LocalPlayer.Character, -1);
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
            }
        }

        private void Flee()
        {
            if (CalloutRunning)
            {
                GameFiber.Wait(500);
                Suspect.Tasks.ClearImmediately();
                Functions.PlayScannerAudio("CRIME_SUSPECT_ON_THE_RUN_01");
                Settings.Pursuits++;
                Settings.Stats.SelectSingleNode("Stats/Pursuits").InnerText = Settings.Pursuits.ToString();
                Settings.Stats.Save(Settings.xmlpath);
                StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights;
                MainPursuit = Functions.CreatePursuit();
                if (Settings.AutomaticBackup)
                {
                    Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                }
                Functions.AddPedToPursuit(MainPursuit, Suspect);
                Functions.SetPursuitIsActiveForPlayer(MainPursuit, true);
                while (Functions.IsPursuitStillRunning(MainPursuit)) { GameFiber.Wait(0); }
                if (Suspect.Exists())
                {
                    if (Functions.IsPedArrested(Suspect)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Following the Pursuit."); Settings.Arrests++; Settings.Stats.SelectSingleNode("Stats/Arrests").InnerText = Settings.Arrests.ToString(); Settings.Stats.Save(Settings.xmlpath); StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights; }
                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Pursuit."); }
                }
                else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Pursuit."); }
                GameFiber.Wait(2000);
                Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                GameFiber.Wait(2000);
            }
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
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            if (VictimBlip.Exists()) { VictimBlip.Delete(); }
            if (Suspect.Exists()) { Suspect.Dismiss(); }
            if (Victim.Exists()) { Victim.Tasks.ClearImmediately(); }
            if (Victim.Exists()) { Victim.Dismiss(); }
            if (SuspectArea.Exists()) SuspectArea.Delete();
        }
    }
}