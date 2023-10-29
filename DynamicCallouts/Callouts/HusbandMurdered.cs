using DynamicCallouts.Utilities;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using RAGENativeUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CAPI = CalloutInterfaceAPI;

namespace DynamicCallouts.Callouts
{
    [CAPI.CalloutInterface("[DYNC] Husband Murdered", CalloutProbability.Low, "A woman has dialled 911, telling her husband has been murdered in their own home!", "Code 3")]
    //[CalloutInfo("[DYNC] Husband Murdered", CalloutProbability.Low)]
    public class HusbandMurdered : Callout
    {
        private Ped player => Game.LocalPlayer.Character;

        private Ped husband;
        private Ped wife;

        private Vector3 calloutLocation;
        private Vector3 doorLocation = new Vector3(266.1389f, -1007.465f, -102.0089f);

        private Blip calloutBlip;
        private Blip wifeBlip;
        private Blip husbandBlip;

        private LHandle pursuit;

        private Marker houseMarker;

        private Rage.Object kitchenKnife;
        private Rage.Object bodyBag;
        private Rage.Object bodyBagForWife;

        private bool CalloutRunning = false;
        private bool HasBegunAttacking = false;
        private bool HasPursuitBegun = false;
        private bool houseMarkerRemoved = false;

        private List<Vector3> calloutPoints = new List<Vector3>
        {
            //new Vector3(-231.4447f, 486.9982f, 126.7569f), // x -1
            //new Vector3(36.56081f, -71.33963f, 61.40691f),
            //new Vector3(951.6448f, -252.4166f, 65.97254f),
            //new Vector3(969.9536f, -700.9822f, 58.48196f)
            new Vector3(38.36615f, -71.51573f, 62.58065f),
            new Vector3(-1590.751f, -412.5249f, 42.06505f),
            new Vector3(352.5502f, -142.7054f, 65.68829f),
            new Vector3(952.3107f, -252.1355f, 66.76872f),
            new Vector3(906.4997f, -489.9599f, 58.43627f)
    };

        private List<string> wifeDialogue1_1 = new List<string>
        {
            "~g~Wife:~s~ Oh Officer, Officer! Please help me, my husband...",
            "~g~You:~s~ I see mam. It looks... Very.. Inhuman. Must have been a cold blooded murderer!",
            "~g~You:~s~ Now mam, we have to secure the corpse and find out who the murderer is.",
            "~g~Wife:~s~ Oh yes, officer. I can't bear to see it..."
        };

        private List<string> wifeDialogue1_2 = new List<string>
        {
            "~g~Wife:~s~ Oh my husband, my beloved. He's hurt and he's not breathing. *sobs*",
            "~g~You:~s~ Calm down mam. Everything is going to be just fine!",
            "~g~Wife:~s~ No it's not! He's not breathing... My beloved husband...",
            "~g~You:~s~ We're going to take care of it mam."
        };

        private List<string> wifeDialogue2_1 = new List<string>
        {
            "~g~You:~s~ What's your confession mam?",
            "~g~Wife:~s~ Do you see the bloody machete behind me?",
            "~g~You:~s~ It was, it was you! You're the murderer?",
            "~g~Wife:~s~ I know what I've done is not right in any way officer...",
            "~g~You:~s~ Mam you're getting arrested right away."
        };
        
        private List<string> wifeDialogue2_2 = new List<string>
        {
            "~g~You:~s~ Why's there a bloody machete over at the sink?",
            "~g~Wife:~s~ I used that to kill him... Please arrest me officer.",
            "~g~You:~s~ How could you... You're under arrest mam."
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            calloutLocation = CallHandler.chooseNearestLocation(calloutPoints);

            if (!CallHandler.MakeSurePlayerIsInRangeForCallout(600f, 30f, calloutLocation))
            {
                Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "DynamicCallouts", "Dispatch", "Another Unit is closer to call. Return to Patrol.");
                CallHandler.SafelyDelete(wife);
                CallHandler.SafelyDelete(husband);
                CallHandler.SafelyDelete(calloutBlip);
                CallHandler.SafelyDelete(wifeBlip);
                CallHandler.SafelyDelete(husbandBlip);
                CallHandler.SafelyDelete(kitchenKnife);
                CallHandler.SafelyDelete(bodyBag);
                Game.LogTrivial("Callout was succesfully declined.");
                return false;
            }

            ShowCalloutAreaBlipBeforeAccepting(calloutLocation, 75f);
            AddMinimumDistanceCheck(30f, calloutLocation);
            AddMaximumDistanceCheck(600f, calloutLocation);
            CalloutMessage = "[DYNC] Husband Murdered";
            CalloutAdvisory = "A woman has dialled 911, telling her husband has been murdered in their own home!";
            CalloutPosition = calloutLocation;
            Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS_01 WE_HAVE A_01 CRIME_DEAD_BODY_01 CODE3", calloutLocation);

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

            kitchenKnife = new Rage.Object(new Model("ch_prop_ch_bloodymachete_01a"), new Vector3(0f, 0f, 0f)); 
            bodyBag = new Rage.Object(new Model("xm_prop_body_bag"), new Vector3(0f, 0f, 0f)); 
            bodyBagForWife = new Rage.Object(new Model("xm_prop_body_bag"), new Vector3(0f, 0f, 0f)); 

            houseMarker = new Marker(calloutLocation, Color.Blue);
            calloutBlip = new Blip(calloutLocation, 20f);
            calloutBlip.Color = Color.Yellow;
            calloutBlip.EnableRoute(Color.Yellow);
            calloutBlip.Alpha = 0.65f;
            calloutBlip.Name = "Search Area";

            husband = CallHandler.SpawnMalePed(new Vector3(258.5119f, -997.7697f, -99.00857f), 250.6773f);
            if (husband) { husband.Kill(); }
            husband.Health = 0;
            CallHandler.ApplyDamagePackToPed(husband, "TD_KNIFE_FRONT", 100.00f, 2.00f);
            CallHandler.ApplyDamagePackToPed(husband, "TD_KNIFE_FRONT_VA", 100.00f, 2.00f);
            CallHandler.ApplyDamagePackToPed(husband, "TD_KNIFE_FRONT_VB", 100.00f, 2.00f);
            CallHandler.ApplyDamagePackToPed(husband, "TD_KNIFE_REAR", 100.00f, 2.00f);
            CallHandler.ApplyDamagePackToPed(husband, "TD_KNIFE_REAR_VA", 100.00f, 2.00f);
            CallHandler.ApplyDamagePackToPed(husband, "TD_KNIFE_REAR_VB", 100.00f, 50.00f);

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
                    GameFiber.StartNew(EndCallChecker);
                    while (CalloutRunning)
                    {
                        GameFiber.Yield();
                        if (husband && calloutLocation.DistanceTo(player.GetOffsetPosition(Vector3.RelativeFront)) < 3f)
                        {
                            Game.DisplayHelp($"Press ~{Settings.InteractionKey1.GetInstructionalId()}~ ~s~to ~b~Enter the house");

                            if (Game.IsKeyDown(Settings.InteractionKey1))
                            {
                                wife = CallHandler.SpawnFemalePed(new Vector3(265.1792f, -996.8583f, -99.00861f), 141.8419f);
                                wife.IsPersistent = true;
                                wife.BlockPermanentEvents = true;
                                CallHandler.EnterHouse(new Vector3(266.139f, -1007.465f, -101.009f), 2.382068f);
                                kitchenKnife.Position = new Vector3(266.0350f, -994.9182f, -99.0256f);
                                kitchenKnife.Rotation = new Rotator(-90f, 117.7333f, 0f);
                                wifeBlip = new Blip(wife);
                                wifeBlip.Color = Color.Green;
                                wifeBlip.Name = "House Wife";
                                Game.DisplayHelp("Go to the ~g~Wife~s~ of the ~r~Murdered Husband~s~.");
                                TalkToHouseWife();
                            }
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

        private void TalkToHouseWife()
        {
            if (CalloutRunning)
            {
                /*if (player.DistanceTo(wife) < 1f && wife && husband)
                {
                    Game.DisplayHelp($"Press ~{Settings.Dialog.GetInstructionalId()}~ ~s~Start talking to the ~g~Wife~s~.");
                    if (Game.IsKeyDown(Settings.Dialog))
                    {
                        End();
                    }
                }*/

                while (player.DistanceTo(wife) > 1f && wife && husband) GameFiber.Wait(0);
                Game.DisplayHelp($"Press ~{Settings.Dialog.GetInstructionalId()}~ ~s~Start talking to the ~g~Wife~s~.");
                while (!Game.IsKeyDown(Settings.Dialog)) GameFiber.Wait(0);
                Random r = new Random();
                int openingDialogue = r.Next(0, 2);
                if (openingDialogue == 0)
                {
                    CallHandler.Dialogue(wifeDialogue1_1, wife);
                }
                else
                {
                    CallHandler.Dialogue(wifeDialogue1_2, wife);
                }
                wife.Tasks.ClearImmediately();
                GoToDeadHusband();
            }
        }

        private void GoToDeadHusband()
        {
            if (CalloutRunning)
            {
                husbandBlip = new Blip(husband);
                husbandBlip.Color = Color.Red;
                husbandBlip.Name = "Dead husband";
                Game.DisplayHelp("Go to the ~r~dead husband~s~, and put him in a body bag.");
                while (player.DistanceTo(husband) > 1f && wife && husband) GameFiber.Wait(0);
                Game.DisplayHelp($"Press ~{Settings.InteractionKey1.GetInstructionalId()}~ to put the ~r~dead husband~s~ in a body bag.");
                while (!Game.IsKeyDown(Settings.InteractionKey1)) GameFiber.Wait(0);
                Game.LocalPlayer.HasControl = false;
                player.Tasks.PlayAnimation("anim@amb@business@weed@weed_inspecting_lo_med_hi@", "weed_spraybottle_crouch_base_inspector", 8.0f, AnimationFlags.None);
                GameFiber.Wait(5000);
                bodyBag.Heading = husband.Heading;
                bodyBag.Position = husband.Position;
                husband.Delete();
                player.Tasks.ClearImmediately();
                Game.LocalPlayer.HasControl = true;
                ChooseEnding();
            }
        }

        private void ChooseEnding()
        {
            if (CalloutRunning)
            {
                Random r = new Random();
                int ending = r.Next(0, 3);
                Game.LogTrivial(ending.ToString());
                if (ending == 0 && wife && !HasBegunAttacking)
                {
                    // backstab with machete
                    Settings.InvolvedInFights++;
                    Settings.Stats.SelectSingleNode("Stats/InvolvedInFights").InnerText = Settings.InvolvedInFights.ToString();
                    Settings.Stats.Save(Settings.xmlpath);
                    StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights;

                    wife.Inventory.GiveNewWeapon("weapon_machete", -1, true);
                    wife.Tasks.FightAgainst(player);
                    Game.DisplaySubtitle("~g~Wife:~s~ I killed him, and now it's your turn! *evil laugh*");
                    while (wife.Exists() && !Functions.IsPedArrested(wife) && wife.IsAlive) GameFiber.Yield();
                    if (wife.Exists())
                    {
                        if (Functions.IsPedArrested(wife) && wife.IsAlive) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Attempting to ~r~Assault an Officer."); Settings.Arrests++; Settings.Stats.SelectSingleNode("Stats/Arrests").InnerText = Settings.Arrests.ToString(); Settings.Stats.Save(Settings.xmlpath); StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights; }
                        else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was unfortunately ~r~Killed~w~ for ~r~Assaulting an Officer."); }
                    }
                    GameFiber.Wait(2000);
                    Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                    GameFiber.Wait(2000);

                    HasBegunAttacking = true;
                }
                else if (ending == 1)
                {
                    // she's gone
                    wife.Position = calloutLocation;
                    wife.Tasks.Flee(calloutLocation, 10000f, -1);
                    Game.DisplaySubtitle("~g~You:~s~ Now mam... Mam? Mam!");
                    WifeGone();
                }
                else
                {
                    // mental breakdown to arrest
                    Game.DisplaySubtitle("~g~Wife:~s~ Officer, I have something to confess.");
                    WifeConfession();
                }
                if (wife && wife.IsDead && ending == 0 && HasBegunAttacking)
                {
                    CleanUpCrimeScene();
                }
                if (wife && wife.IsDead && ending != 0) End();
                if (wife && Functions.IsPedArrested(wife)) End();
                if (player.IsDead) End();
            }
        }

        private void CleanUpCrimeScene()
        {
            if (CalloutRunning)
            {
                Game.DisplayHelp("Put the ~g~Wife~s~ in a body bag.");
                while (player.DistanceTo(wife) > 1f) GameFiber.Wait(0);
                Game.DisplayHelp($"Press ~{Settings.InteractionKey1.GetInstructionalId()}~ to put the ~g~dead wife~s~ in a body bag.");
                while (!Game.IsKeyDown(Settings.InteractionKey1)) GameFiber.Wait(0);
                Game.LocalPlayer.HasControl = false;
                player.Tasks.PlayAnimation("anim@amb@business@weed@weed_inspecting_lo_med_hi@", "weed_spraybottle_crouch_base_inspector", 8.0f, AnimationFlags.None);
                GameFiber.Wait(5000);
                bodyBagForWife.Heading = wife.Heading;
                bodyBagForWife.Position = wife.Position;
                wife.Delete();
                player.Tasks.ClearImmediately();
                Game.LocalPlayer.HasControl = true;
                Game.DisplayHelp($"Press ~{Settings.InteractionKey1.GetInstructionalId()}~ ~s~To clear up the crime scene.");
                while (!Game.IsKeyDown(Settings.InteractionKey1)) GameFiber.Wait(0);
                GameFiber.Wait(1250);
                Game.FadeScreenOut(1500, true);
                Game.LocalPlayer.HasControl = false;
                GameFiber.Wait(2500);
                CallHandler.SafelyDelete(kitchenKnife);
                CallHandler.SafelyDelete(wifeBlip);
                CallHandler.SafelyDelete(bodyBag);
                CallHandler.SafelyDelete(bodyBagForWife);
                Game.FadeScreenIn(1500, true);
                Game.LocalPlayer.HasControl = true;
                Game.DisplayHelp("You're now clear to leave the crime scene.");
                Marker m = new Marker(doorLocation, Color.DarkBlue);
                while (player.DistanceTo(doorLocation) > 1f) GameFiber.Yield();
                Game.DisplayHelp($"Press ~{Settings.InteractionKey1.GetInstructionalId()}~ ~s~To leave the crime scene.");
                while (!Game.IsKeyDown(Settings.InteractionKey1)) GameFiber.Wait(0);
                CallHandler.EnterHouse(calloutLocation); // don't mind the function name, we're actually exiting the house
                m.Dispose();
                GameFiber.Wait(2500);
                End();
            }
        }

        private void WifeGone()
        {
            if (CalloutRunning)
            {
                Marker m = new Marker(doorLocation, Color.DarkBlue);
                Game.DisplayHelp("Exit the House and chase down the ~g~Wife~s~.");
                while (player.DistanceTo(doorLocation) > 1f) GameFiber.Yield();
                Game.DisplayHelp($"Press ~{Settings.InteractionKey1.GetInstructionalId()}~ ~s~To leave the House.");
                while (!Game.IsKeyDown(Settings.InteractionKey1)) GameFiber.Wait(0);
                CallHandler.EnterHouse(calloutLocation);
                m.Dispose();
                wife.Tasks.ClearImmediately();
                if (!HasPursuitBegun)
                {
                    Settings.Pursuits++;
                    Settings.Stats.SelectSingleNode("Stats/Pursuits").InnerText = Settings.Pursuits.ToString();
                    Settings.Stats.Save(Settings.xmlpath);
                    StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights;
                    pursuit = Functions.CreatePursuit();
                    if (Settings.AutomaticBackup)
                    {
                        Functions.RequestBackup(wife.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                    }
                    Functions.AddPedToPursuit(pursuit, wife);
                    Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                    HasPursuitBegun = true;
                    while (Functions.IsPursuitStillRunning(pursuit)) { GameFiber.Wait(0); }
                    if (wife.Exists())
                    {
                        if (Functions.IsPedArrested(wife)) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ Following the Pursuit."); Settings.Arrests++; Settings.Stats.SelectSingleNode("Stats/Arrests").InnerText = Settings.Arrests.ToString(); Settings.Stats.Save(Settings.xmlpath); StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights; }
                        else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was ~r~Killed~w~ Following the Pursuit."); }
                    }
                    GameFiber.Wait(2000);
                    Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                    GameFiber.Wait(2000);
                }
            }
        }

        private void WifeConfession()
        {
            if (CalloutRunning)
            {
                Game.DisplayHelp("Go talk to the ~g~Wife~s~.");
                while (player.DistanceTo(wife) > 1f && wife) GameFiber.Wait(0);
                Game.DisplayHelp($"Press ~{Settings.Dialog.GetInstructionalId()}~ ~s~Start talking to the ~g~Wife~s~.");
                while (!Game.IsKeyDown(Settings.Dialog)) GameFiber.Wait(0);
                Random r = new Random();
                int DialogueNumber = r.Next(0, 2);
                if (DialogueNumber == 0)
                {
                    CallHandler.Dialogue(wifeDialogue2_1, wife);
                }
                else
                {
                    CallHandler.Dialogue(wifeDialogue2_2, wife);
                }
                wife.Tasks.ClearImmediately();
                Game.DisplayHelp("~r~Arrest~s~ the ~g~Wife~s~.");
                while (wife.Exists() && !Functions.IsPedArrested(wife) && wife.IsAlive)
                {
                    GameFiber.Yield();
                }
                if (wife.Exists())
                {
                    if (Functions.IsPedArrested(wife) && wife.IsAlive) { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect is Under ~g~Arrest~w~ For ~r~Murdering her husband~s~."); Settings.Arrests++; Settings.Stats.SelectSingleNode("Stats/Arrests").InnerText = Settings.Arrests.ToString(); Settings.Stats.Save(Settings.xmlpath); StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights; }
                    else { GameFiber.Wait(1000); Game.DisplayNotification("Dispatch, a Suspect Was unfortunately ~r~Killed~w~."); }
                }
                GameFiber.Wait(2000);
                Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_02");
                GameFiber.Wait(2000);
                Game.DisplayHelp($"Press ~{Settings.InteractionKey1.GetInstructionalId()}~ ~s~To clear up the crime scene.");
                while (!Game.IsKeyDown(Settings.InteractionKey1)) GameFiber.Wait(0);
                GameFiber.Wait(1250);
                Game.FadeScreenOut(1500, true);
                Game.LocalPlayer.HasControl = false;
                GameFiber.Wait(2500);
                CallHandler.SafelyDelete(wife);
                CallHandler.SafelyDelete(kitchenKnife);
                CallHandler.SafelyDelete(wifeBlip);
                CallHandler.SafelyDelete(bodyBag);
                Game.FadeScreenIn(1500, true);
                Game.LocalPlayer.HasControl = true;
                Game.DisplayHelp("You're now clear to leave the crime scene.");
                Marker m = new Marker(doorLocation, Color.DarkBlue);
                while (player.DistanceTo(doorLocation) > 1f) GameFiber.Yield();
                Game.DisplayHelp($"Press ~{Settings.InteractionKey1.GetInstructionalId()}~ ~s~To leave the crime scene.");
                while (!Game.IsKeyDown(Settings.InteractionKey1)) GameFiber.Wait(0);
                CallHandler.EnterHouse(calloutLocation); // don't mind the function name, we're actually exiting the house
                m.Dispose();
                GameFiber.Wait(2500);
                End();
            }
        }

        public override void End()
        {
            if (wife) wife.Dismiss();
            CallHandler.SafelyDelete(husband);
            CallHandler.SafelyDelete(calloutBlip);
            CallHandler.SafelyDelete(wifeBlip);
            CallHandler.SafelyDelete(husbandBlip);
            CallHandler.SafelyDelete(kitchenKnife);
            CallHandler.SafelyDelete(bodyBag);

            if (CalloutRunning)
            {
                Game.DisplayNotification("~g~Code 4~w~, return to patrol.");
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS WE_ARE_CODE_4");
            }

            if (!houseMarkerRemoved) houseMarker.Dispose();

            CalloutRunning = false;
            base.End();
        }

        private void EndCallChecker()
        {
            while (CalloutRunning)
            {
                GameFiber.Yield();
                if (Game.IsKeyDown(Settings.EndCall))
                {
                    End();
                }
            }
        }
    }
}
