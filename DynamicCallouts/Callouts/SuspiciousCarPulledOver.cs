using DynamicCallouts.Utilities;
using LSPD_First_Response.Engine.Scripting;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace DynamicCallouts.Callouts
{
    //Declaring the callouts name and probability
    [CalloutInfo("[DYNC] Suspicous car pulled over", CalloutProbability.Medium)]
    public class SuspiciousCarPulledOver : Callout
    {
        //Possible vehicles in the callout
        private string[] CopCars = new string[] { "POLICE", "POLICE2", "POLICE3", "POLICE4", "FBI", "FBI2", "SHERIFF", "SHERIFF2" };
        private string[] DrugCars = new string[] { "BURRITO2", "BURRITO3", "BURRITO4" };

        //Possible peds in the callout
        private string[] Suspects = new string[] { "A_F_Y_Hippie_01", "A_M_Y_Skater_01", "A_M_M_FatLatin_01", "A_M_M_EastSA_01", "A_M_Y_Latino_01", "G_M_Y_FamDNF_01", "G_M_Y_FamCA_01", "G_M_Y_BallaSout_01", "G_M_Y_BallaOrig_01",
                                                  "G_M_Y_BallaEast_01", "G_M_Y_StrPunk_02", "S_M_Y_Dealer_01", "A_M_M_RurMeth_01", "A_M_Y_MethHead_01", "A_M_M_Skidrow_01", "S_M_Y_Dealer_01", "a_m_y_mexthug_01", "G_M_Y_MexGoon_03", "G_M_Y_MexGoon_02", "G_M_Y_MexGoon_01", "G_M_Y_SalvaGoon_01",
                                                  "G_M_Y_SalvaGoon_02", "G_M_Y_SalvaGoon_03", "G_M_Y_Korean_01", "G_M_Y_Korean_02", "G_M_Y_StrPunk_01" };
        private string[] CopList = new string[] { "S_M_Y_COP_01", "S_F_Y_COP_01", "S_M_Y_SHERIFF_01", "S_F_Y_SHERIFF_01" };

        //Just a drug list
        private string[] DrugList = new string[] { "Heroin", "Cocain" };
        private string drug;

        //Peds
        public Ped Suspect;
        private Ped Suspect1;
        private Ped Cop;
        private Ped Victim;
        private Ped Victim1;
        private Ped player => Game.LocalPlayer.Character;

        //Vehicles with the vehicle class
        private Vehicle SuspectCar;
        private Vehicle CopCar;

        //Positions/Vector3s
        private Vector3 Spawnpoint;
        private Vector3 TempLocation = new Vector3(0f, 0f, 0f);

        //Blips aka the dots on the map
        private Blip CopBlip;
        private Blip DriverBlip;

        //Ints/whole number
        private int SuspicousThing; //We'll use this to determine which sus thing the suspects have in their car in scene 3
        private int num;

        //Pursuits/LHandles
        private LHandle pursuit;

        //Bools/true or false
        private bool CalloutRunning = false;
        private bool PursuitCreated = false;
        private bool Scene1 = false;
        private bool Scene2 = false;
        private bool Scene3 = false;
        private bool Scene3Done = false;
        private bool TookTest = false;

        //Rage props for the drugs, weapons and etc.
        private Rage.Object Drug;
        private Rage.Object Drug1;
        private Rage.Object Drug2;
        private Rage.Object Drug3;

        //Dialogs so we can't talk in the callout
        private readonly List<string> TookTestDialogue = new List<string>()
        {
            "~b~Cop:~w~ Hello there! Thanks for coming that quickly!",
            "~g~You:~w~ Well no problem. So what's the problem?",
            "~b~Cop:~w~ Well I stopped this car because I found it suspicous.",
            "~b~Cop:~w~ I asked the driver to get out of the vehicle but first he denied, after some time I convinced him.",
            "~b~Cop:~w~ I then got a search warrant, and then I began searching and soon after I found the drugs you see over there.",
            "~b~Cop:~w~ I found my drug test kit and took a test, found out it was some cocain.",
            "~b~Cop:~w~ Without knowing the driver secretly entered the vehicle and locked himself in.",
            "~g~You:~w~ Hmm alright, what a story. So he's a drug dealer I suppose?",
            "~b~Cop:~w~ Yeah he might be. Also I think he might be armed as I didn't frisk him.",
            "~g~You:~w~ Okay, now I understand why you called for backup! I'm going to talk to him.",
        };
        private readonly List<string> TalkToDriverDialogue = new List<string>()
        {
            "~g~You:~w~ Sir if you're armed please hand it over to me.",
            "~y~Driver:~w~ Officer I ain't armed.",
            "~g~You:~w~ Then please step out of the vehicle.",
        };
        private readonly List<string> TalkToDriverDialogue1 = new List<string>()
        {
            "~g~You:~w~ So you're a drug dealer huh?",
            "~y~Driver:~w~ No I only deliver the drugs for the dealer.",
            "~g~You:~w~ So who's the dealer?",
            "~y~Driver:~w~ I ain't gon' snitch you scum!",
        };
        private readonly List<string> DidntTakeTestDialogue = new List<string>()
        {
            "~b~Cop:~w~ Hey! Thanks for coming!",
        };

        Tuple<Vector3, float>[] SpawningLocationList =
        {
            Tuple.Create(new Vector3(-452.2763f, 5930.209f, 32.00574f),141.1158f),
            Tuple.Create(new Vector3(2689.76f, 4379.656f, 46.21445f),123.7446f),
            Tuple.Create(new Vector3(-2848.013f, 2205.696f, 31.40776f),117.3819f),
            Tuple.Create(new Vector3(-1079.767f, -2050.001f, 12.78075f),223.3597f),
            Tuple.Create(new Vector3(1901.965f, -735.1039f, 84.55292f),125.9702f),
            Tuple.Create(new Vector3(2620.896f, 255.5361f, 97.55639f),349.3095f),
            Tuple.Create(new Vector3(1524.368f, 820.0878f, 77.10448f),332.4926f),
            Tuple.Create(new Vector3(2404.46f, 2872.158f, 39.88745f),307.5641f),
            Tuple.Create(new Vector3(2913.759f, 4148.546f, 50.26934f),16.63741f),
        };

        private VehicleDoor[] SuspectCarDoors;

        public override bool OnBeforeCalloutDisplayed()
        {
            Random random = new Random();
            List<Vector3> list = new List<Vector3>();
            for (int i = 0; i < SpawningLocationList.Length; i++)
            {
                list.Add(SpawningLocationList[i].Item1);
            }
            num = CallHandler.nearestLocationIndex(list);
            Game.LogTrivial(num.ToString());
            foreach(Vector3 pos in list)
            {
                Game.LogTrivial(pos.ToString());
            }

            Spawnpoint = SpawningLocationList[num].Item1;
            Game.LogTrivial("Spawnpoint is " + Spawnpoint.ToString());

            Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS OFFICER_REQUESTING_BACKUP", Spawnpoint);
            ShowCalloutAreaBlipBeforeAccepting(Spawnpoint, 100f);
            //AddMinimumDistanceCheck(150f, Spawnpoint);

            CalloutMessage = "[DYNC] Suspicous car pulled over";
            CalloutPosition = Spawnpoint;
            CalloutAdvisory = "Nothing yet.";

            //SuspectCar = new Vehicle(DrugCars[new Random().Next((int)DrugCars.Length)], CopCar.GetOffsetPosition(Vector3.RelativeFront * 9), CopCar.Heading);

            /*Random r = new Random();
            int Scene = r.Next(0, 3);
            if (Scene == 0)
            {
                Scene1 = true;
            }
            else if (Scene == 1)
            {
                Scene2 = true;
            }
            else
            {
                Scene3 = true;
            }*/
            Scene3 = true;

            if (Scene3)
            {
                Drug = new Rage.Object(new Model("ex_office_swag_drugbag2"), TempLocation);
                Drug1 = new Rage.Object(new Model("hei_prop_heist_drug_tub_01"), TempLocation);
                Drug2 = new Rage.Object(new Model("ex_office_swag_drugbags"), TempLocation);
                Drug3 = new Rage.Object(new Model("hei_prop_hei_drug_case"), TempLocation);
            }

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            try
            {
                CopCar = new Vehicle(CopCars[new Random().Next(CopCars.Length)], Spawnpoint, SpawningLocationList[num].Item2);
                CopCar.IsSirenOn = true;
                CopCar.IsSirenSilent = true;

                SuspectCar = new Vehicle(DrugCars[new Random().Next(DrugCars.Length)], CopCar.GetOffsetPosition(Vector3.RelativeFront * 9), CopCar.Heading);
                drug = DrugList[new Random().Next(DrugList.Length)];
                SuspectCar.Metadata.searchTrunk = "~r~Lots of " + drug + "~s~";
            }
            catch (Exception e)
            {
                Game.LogTrivial(e.ToString());
                return false;
            }

            if (Scene3)
            {
                int Bodyshellbone = NativeFunction.Natives.GET_ENTITY_BONE_INDEX_BY_NAME<int>(SuspectCar, "bodyshell");
                Game.LogTrivial(Bodyshellbone.ToString());
                Drug.AttachTo(SuspectCar, Bodyshellbone, new Vector3(-0.7f, -0.86f, -0.3f), new Rotator(0f, 0f, 88.3005f));
                Drug1.AttachTo(SuspectCar, Bodyshellbone, new Vector3(0.7f, -1.15f, -0.12f), new Rotator(0f, 0f, -90f));
                Drug2.AttachTo(SuspectCar, Bodyshellbone, new Vector3(0.67f, -1.13f, 0.06f), new Rotator(0f, 0f, -53.8203f));
                Drug3.AttachTo(SuspectCar, Bodyshellbone, new Vector3(-0.1f, -2.18f, 0.04f), new Rotator(0f, 0f, 0f));
                /*NativeFunction.Natives.SET_VEHICLE_DOOR_OPEN(SuspectCar, 2, false, true);
                NativeFunction.Natives.SET_VEHICLE_DOOR_OPEN(SuspectCar, 3, false, true);
                bool door2 = NativeFunction.Natives.IS_VEHICLE_DOOR_FULLY_OPEN<bool>(SuspectCar, 2);
                bool door3 = NativeFunction.Natives.IS_VEHICLE_DOOR_FULLY_OPEN<bool>(SuspectCar, 3);
                if (!door2 || !door3)
                {
                    NativeFunction.Natives.SET_VEHICLE_DOOR_OPEN(SuspectCar, 2, false, true);
                    NativeFunction.Natives.SET_VEHICLE_DOOR_OPEN(SuspectCar, 3, false, true);
                }*/
                SuspectCarDoors = SuspectCar.GetDoors();
                SuspectCarDoors[2].Open(true);
                SuspectCarDoors[3].Open(true);
            }

            Cop = new Ped(CopList[new Random().Next(CopList.Length)], Spawnpoint, 0f);
            Cop.IsPersistent = true;
            Cop.BlockPermanentEvents = true;
            Cop.Inventory.GiveNewWeapon("WEAPON_PISTOL", 500, true);
            Cop.WarpIntoVehicle(CopCar, -1);
            Cop.Tasks.CruiseWithVehicle(0, VehicleDrivingFlags.None);
            Functions.IsPedACop(Cop);

            Suspect = new Ped(Suspects[new Random().Next(Suspects.Length)], Spawnpoint, 0f);
            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;
            Suspect.WarpIntoVehicle(SuspectCar, -1);
            Suspect.Tasks.CruiseWithVehicle(0, VehicleDrivingFlags.None);

            CopBlip = new Blip(Cop);
            CopBlip.EnableRoute(Color.Yellow);
            CopBlip.Color = Color.LightBlue;
            CopBlip.Scale = 0.65f;
            CopBlip.Name = "Cop";
            SuspectCarDoors[2].Open(true);
            SuspectCarDoors[3].Open(true);

            if (!CalloutRunning) Callout(); CalloutRunning = true;
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (Cop.Exists()) Cop.Delete();
            if (Suspect.Exists()) Suspect.Delete();
            if (CopBlip.Exists()) CopBlip.Delete();
            if (SuspectCar.Exists()) SuspectCar.Delete();
            if (CopCar.Exists()) CopCar.Delete();
            if (Drug.Exists()) Drug.Delete();
            if (Drug1.Exists()) Drug1.Delete();
            if (Drug2.Exists()) Drug2.Delete();
            if (Drug3.Exists()) Drug3.Delete();

            base.OnCalloutNotAccepted();
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
                        bool hasCarDoorsBeenOpened = false;
                        if (!hasCarDoorsBeenOpened) { SuspectCarDoors[2].Open(true); SuspectCarDoors[3].Open(true); }
                        if (Spawnpoint.DistanceTo(player) < 25f)
                        {
                            if (Scene3 && Cop.DistanceTo(player) < 25f)
                            {
                                Cop.Tasks.LeaveVehicle(CopCar, LeaveVehicleFlags.LeaveDoorOpen);
                                Game.DisplayHelp("Speak to the other ~b~Cop");
                                NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Cop, player, -1);
                                while (player.DistanceTo(Cop) > 5f && !player.IsOnFoot) GameFiber.Wait(0);
                                Game.DisplayHelp("Press ~y~" + Settings.Dialog + " ~w~to Speak with the ~b~Cop");
                                while (!Game.IsKeyDown(Settings.Dialog)) GameFiber.Wait(0);
                                
                                Random r = new Random();
                                TookTest = r.Next(0, 2) == 0;
                                if (TookTest)
                                {
                                    CallHandler.Dialogue(TookTestDialogue, Cop);
                                    DriverBlip = new Blip(Suspect);
                                    DriverBlip.Color = Color.Yellow;
                                    DriverBlip.Scale = 0.65f;
                                    DriverBlip.Name = "Driver";
                                }
                                else
                                {
                                    CallHandler.Dialogue(DidntTakeTestDialogue, Cop);
                                    DriverBlip = new Blip(Suspect);
                                    DriverBlip.Color = Color.Yellow;
                                    DriverBlip.Scale = 0.65f;
                                    DriverBlip.Name = "Driver";
                                }
                                Cop.Tasks.ClearImmediately();
                                CallHandler.IdleAction(Cop, true);
                                if (TookTest) { Game.DisplayHelp("Talk to the ~y~Driver", false); TookTestVoid(); }
                                else { if (Settings.IsSTPKeyModifierSet) { Game.DisplayHelp("Get the ~y~Driver~w~ out of the vehicle to initiate the drug test. Tip: Double press ~y~" + Settings.StopThePedKey + " + " + Settings.StopThePedKey1 + "~w~ to stop the ped"); } else if (!Settings.IsSTPKeyModifierSet) { Game.DisplayHelp("Get the ~y~Driver~w~ out of the vehicle to initiate the drug test. Tip: Double press ~y~" + Settings.StopThePedKey + "~w~ to stop the ped"); } else { Game.DisplayHelp("Get the ~y~Driver~w~ out of the vehicle to initiate the drug test."); } while (Suspect.IsInAnyVehicle(false)) GameFiber.Wait(0); DidntTakeTestVoid();  }
                                break;
                            }
                        }

                        if (Suspect && Suspect.IsDead) End();
                        if (Suspect && Functions.IsPedArrested(Suspect)) End();
                        if (Game.IsKeyDown(Settings.EndCall)) End();
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

        public override void End()
        {
            if (CalloutRunning)
            {
                Game.DisplayNotification("~g~Code 4~w~, return to patrol.");
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS WE_ARE_CODE_4");
            }

            CalloutRunning = false;
            if (Cop.Exists()) Cop.Delete();
            if (Suspect.Exists()) Suspect.Dismiss();
            if (CopBlip.Exists()) CopBlip.Delete();
            if (SuspectCar.Exists()) SuspectCar.Delete();
            if (CopCar.Exists()) CopCar.Delete();
            if (Drug.Exists()) Drug.SafelyDelete();
            if (Drug1.Exists()) Drug1.Delete();
            if (Drug2.Exists()) Drug2.Delete();
            if (Drug3.Exists()) Drug3.Delete();
            if (PursuitCreated) Functions.ForceEndPursuit(pursuit);
            if (DriverBlip.Exists()) DriverBlip.Delete();

            base.End();
        }

        private void TookTestVoid()
        {
            if (CalloutRunning)
            {
                while (player.DistanceTo(Suspect) > 4f) GameFiber.Wait(0);
                Random r = new Random();
                int number = r.Next(3);
                Game.LogTrivial(number.ToString());
                if (number == 0) Fight();
                else if (number == 1) Flee();
                else SpeakToStoppedPed();
            }
        }

        private void Fight()
        {
            if (CalloutRunning)
            {
                DriverBlip = new Blip(Suspect);
                DriverBlip.Color = Color.Red;
                DriverBlip.Scale = 0.65f;
                DriverBlip.Name = "Driver";
                Cop.Tasks.ClearImmediately();
                Suspect.Tasks.ClearImmediately();
                Suspect.Inventory.Weapons.Clear();
                GameFiber.Wait(500);
                Suspect.Tasks.FightAgainst(Game.LocalPlayer.Character, -1);
                Cop.Tasks.FightAgainst(Suspect, -1);
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
                Functions.PlayScannerAudio("CRIME_SUSPECT_ON_THE_RUN_01");
                Settings.Pursuits++;
                Settings.Stats.SelectSingleNode("Stats/Pursuits").InnerText = Settings.Pursuits.ToString();
                Settings.Stats.Save(Settings.xmlpath);
                StatsView.textTab.Text = "Responded Callouts: " + Settings.RespondedCallouts + "~n~ ~n~Arrests performed: " + Settings.Arrests + "~n~ ~n~Times involved in pursuits: " + Settings.Pursuits + "~n~ ~n~Times Involved in fights: " + Settings.InvolvedInFights;
                pursuit = Functions.CreatePursuit();
                PursuitCreated= true;
                if (Settings.AutomaticBackup)
                {
                    Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                }
                Functions.AddPedToPursuit(pursuit, Suspect);
                Functions.AddCopToPursuit(pursuit, Cop);
                Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                while (Functions.IsPursuitStillRunning(pursuit)) { GameFiber.Wait(0); }
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

        private void SpeakToStoppedPed()
        {
            if (CalloutRunning)
            {
                Game.DisplayHelp("Press ~y~" + Settings.Dialog + " ~w~to Speak with the ~y~Driver");
                while (!Game.IsKeyDown(Settings.Dialog)) GameFiber.Wait(0);
                Cop.Tasks.ClearImmediately();
                CopDriveTask();
                CallHandler.DialogueWithoutAnim(TalkToDriverDialogue);
                Random r = new Random();
                int number = r.Next(3);
                if (number == 0) Fight();
                else if (number == 1) Flee();
                else
                {
                    Suspect.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                    Game.DisplayHelp("Press ~y~" + Settings.Dialog + " ~w~to Speak with the ~y~Driver");
                    while (!Game.IsKeyDown(Settings.Dialog)) GameFiber.Wait(0);
                    CallHandler.Dialogue(TalkToDriverDialogue1, Suspect);
                    Game.DisplayHelp("~y~" + Settings.InteractionKey1 + ":~b~ Take the Suspect yourself. ~y~" + Settings.InteractionKey2 + ":~b~ Call a police transport for Suspect.", true);
                    while (!Game.IsKeyDown(Settings.InteractionKey1) && !Game.IsKeyDown(Settings.InteractionKey2)) GameFiber.Wait(0);
                    if (Game.IsKeyDown(Settings.InteractionKey1)) { Game.HideHelp(); Game.DisplayHelp("Press ~y~" + Settings.EndCall + "~w~ to end the call."); while (!Game.IsKeyDown(Settings.EndCall)) GameFiber.Wait(0); End(); }
                    else if (Game.IsKeyDown(Settings.InteractionKey2)) { Game.HideHelp(); Functions.RequestSuspectTransport(Suspect); Game.DisplayHelp("Press ~y~" + Settings.EndCall + "~w~ to end the call."); while (!Game.IsKeyDown(Settings.EndCall)) GameFiber.Wait(0); End(); }
                }
            }
        }

        private void CopDriveTask()
        {
            if (CalloutRunning)
            {
                Cop.Tasks.DriveToPosition(CopCar, SuspectCar.GetOffsetPosition(new Vector3(8f, 0f, 0f)), 60f, VehicleDrivingFlags.DriveAroundObjects, 100f).WaitForCompletion();
                Cop.Tasks.CruiseWithVehicle(CopCar, 60f, VehicleDrivingFlags.FollowTraffic);
                CopBlip.SafelyDelete();
            }
        }

        private void DidntTakeTestVoid()
        {
            if (CalloutRunning)
            {
                Game.DisplayHelp("Go to the back of the van to take the test.");
                while (Drug3.DistanceTo(player) < 1f) GameFiber.Wait(0);
                Game.DisplayHelp("Press ~y~" + Settings.InteractionKey1 + "~w~ to take the drug test.");
                while (!Game.IsKeyDown(Settings.InteractionKey1)) GameFiber.Wait(0);
                // TODO: Play search animation
                Game.LocalPlayer.HasControl = false;
                Game.DisplaySubtitle("Searching", 3000);
                GameFiber.Wait(3000);
                Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "DynamicCallouts", "Drug Test Result", "You found out the drugs is: ~r~" + drug);
                Game.LocalPlayer.HasControl = true;
                Random r = new Random();
                int number = r.Next(3);
                if (number == 0) Fight();
                else if (number == 1) Flee();
                else
                {
                    Game.DisplayHelp("Deal with the situation as you want to.", 5000);
                    GameFiber.Wait(5000);
                    Game.DisplayHelp("Press ~y~" + Settings.EndCall + "~w~ to end the call."); while (!Game.IsKeyDown(Settings.EndCall)) GameFiber.Wait(0); End();
                }
            }
        }
    }
}
