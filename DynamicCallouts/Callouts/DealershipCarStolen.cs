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
using CAPI = CalloutInterfaceAPI;

namespace DynamicCallouts.Callouts
{
    [CAPI.CalloutInterface("[DYNC] Dealership Car Stolen", CalloutProbability.Low, "A distress call has been received from a local car dealership reporting a daring theft. A bold suspect crashed through the showroom window and made off with the latest car in their collection.", "Code 2")]
    public class DealershipCarStolen : Callout
    {
        private Ped player => Game.LocalPlayer.Character;

        private Ped CarDealerMan;
        private Ped Suspect;
        private Ped Suspect2;

        private Vehicle Car;

        private Vector3 calloutLocation = new Vector3(-58.161064f, -1099.174f, 25.10041f);
        private Vector3 CarSpawnLocation;
        private Vector3 lastSeenLocation;

        private Blip calloutLocationBlip;
        private Blip CarDealerManBlip;
        private Blip CarBlip;

        private LHandle pursuit;

        private string CarModel = string.Empty;
        private string CarLicensePlate = string.Empty;

        private bool CalloutRunning = false;
        private bool found = false;

        private Rage.Object des_showroom_end;

        private GameFiber updateBlip;

        private readonly List<string> CarDealerDialogue1 = new List<string>()
        {
            "~b~Distressed Car Dealer:~w~ Officer, I'm in desperate need of your assistance!",
            "~g~You:~w~ Take a breath. I'm here to help. What's the urgency?",
            "~b~Distressed Car Dealer:~w~ I can't stay calm! Someone crashed through the window and sped away with our newest addition to the collection!",
            "~b~Distressed Car Dealer:~w~ It's a nightmare! Our prized possession is gone!",
            "~g~You:~w~ I understand. Before I act, can you provide more details about the stolen car?"
        };

        private readonly List<string> CarDealerDialogue2 = new List<string>()
        {
            "~b~Panicked Car Dealer:~w~ Officer! It's a nightmare! Someone broke into our showroom and drove away with our newest car!",
            "~g~You:~w~ Take a deep breath. I'm here to help. Can you describe what happened?",
            "~b~Panicked Car Dealer:~w~ They smashed the window, hopped into the latest model, and sped away! I can't believe it!",
            "~b~Panicked Car Dealer:~w~ This is a disaster for our business! You have to catch them!",
            "~g~You:~w~ I'll do my best. Before I start, can you give me more details about the stolen car?"
        };
        
        private readonly List<string> CarDealerDialogue3 = new List<string>()
        {
            "~b~Distraught Car Dealer:~w~ Officer! We're in trouble! Someone broke into the dealership and drove off with our brand-new car!",
            "~g~You:~w~ I'm here to help. Please calm down and tell me what happened.",
            "~b~Distraught Car Dealer:~w~ They smashed through the showroom window, got into the latest model, and took off like a rocket!",
            "~b~Distraught Car Dealer:~w~ This is a nightmare! Our valuable asset is gone. You need to catch them!",
            "~g~You:~w~ I'll investigate, but first, can you provide more information about the car that was stolen?"
        };

        public override bool OnBeforeCalloutDisplayed()
        {
            if (!Settings.DealershipCarStolen)
            {
                Game.LogTrivial("[LOG]: User has disabled DealershipCarStolen, returning false.");
                Game.LogTrivial("[LOG]: To enable the callout please change false to true in the .ini file.");
                return false;
            }

            ShowCalloutAreaBlipBeforeAccepting(calloutLocation, 75f);
            CalloutMessage = "[DYNC] Dealership Car Stolen";
            CalloutAdvisory = "A distress call has been received from a local car dealership reporting a daring theft. A bold suspect crashed through the showroom window and made off with the latest car in their collection.";
            CalloutPosition = calloutLocation;
            Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS_01 WE_HAVE_01 A_01 CRIME_GRAND_THEFT_AUTO_01 UNITS_RESPOND_CODE_02_01", calloutLocation);

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
            NativeFunction.Natives.REMOVE_IPL("shr_int");
            NativeFunction.Natives.REMOVE_IPL("fakeint");

            NativeFunction.Natives.REQUEST_IPL("carshowroom_broken");
            try
            {
                bool boarded = NativeFunction.Natives.IS_IPL_ACTIVE<bool>("carshowroom_boarded");
                Game.LogTrivial("Car Showroom boarded = " + boarded.ToString());

                bool shrint = NativeFunction.Natives.IS_IPL_ACTIVE<bool>("shr_int");
                Game.LogTrivial("v_carshowroom = " + shrint.ToString());

                bool boards = NativeFunction.Natives.IS_IPL_ACTIVE<bool>("fakeint_boards");
                Game.LogTrivial("Car Showroom fake interior boards = " + boards.ToString());

                bool fakeint = NativeFunction.Natives.IS_IPL_ACTIVE<bool>("fakeint");
                Game.LogTrivial("Car Showroom fake interior = " + fakeint.ToString());
            }
            catch (Exception ex)
            {
                Game.LogTrivial(ex.ToString());
            }

            des_showroom_end = new Rage.Object("des_showroom_end", new Vector3(0f, 0f, 0f));
            des_showroom_end.Position = new Vector3(-56.1673f, -1097.576f, 25.1720f);
            des_showroom_end.Rotation = new Rotator(0f, -0f, -0.4282f);

            CarDealerMan = new Ped("IG_SiemonYetarian", new Vector3(-60.58188f, -1099.592f, 26.41835f), 124.5284f);
            CarDealerMan.IsPersistent = true;
            CarDealerMan.BlockPermanentEvents = true;

            calloutLocationBlip = new Blip(calloutLocation, 75f);
            calloutLocationBlip.Color = Color.Yellow;
            calloutLocationBlip.EnableRoute(Color.Yellow);
            calloutLocationBlip.Alpha = 0.65f;
            calloutLocationBlip.Name = "Search Area";

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
                        while (player.DistanceTo(CarDealerMan) >= 25f && !Game.IsKeyDown(Settings.EndCall)) GameFiber.Wait(0);
                        CallHandler.SafelyDelete(calloutLocationBlip);
                        CarDealerManBlip = new Blip(calloutLocation);
                        CarDealerManBlip.Color = Color.Blue;
                        CarDealerManBlip.Scale = 0.65f;
                        CarDealerManBlip.Name = "Car Dealer";
                        Game.DisplayHelp("Speak to the ~b~Car Dealer");

                        NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(CarDealerMan, player, -1);
                        while (player.DistanceTo(CarDealerMan) >= 5f) GameFiber.Wait(0);
                        Game.DisplayHelp("Press ~y~" + Settings.Dialog + " ~w~to Speak with the ~b~Car Dealer");
                        while (!Game.IsKeyDown(Settings.Dialog)) GameFiber.Wait(0);

                        CarSpawnLocation = World.GetNextPositionOnStreet(player.Position.Around(1000f));

                        Vector3 coords = player.Position;
                        Vector3 closestVehicleNodeCoords;
                        float roadHeading;

                        NativeFunction.Natives.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING<Vector3>(coords.X, coords.Y, coords.Z, out closestVehicleNodeCoords, out roadHeading, 0, 3, 0);

                        Suspect = new Ped();
                        Suspect.IsPersistent = true;
                        Suspect.BlockPermanentEvents = true;

                        Car = CallHandler.SpawnVehicle(CarSpawnLocation, roadHeading);
                        Car.IsStolen = true;
                        if (Car.IsInWater)
                        {
                            Car.Position = World.GetNextPositionOnStreet(player.Position.Around(750f));
                        }
                        Functions.SetVehicleOwnerName(Car, "Simeon Yetarian");
                        Suspect.WarpIntoVehicle(Car, -1);
                        Suspect.Tasks.CruiseWithVehicle(Car, 45f, VehicleDrivingFlags.Emergency);
                        System.Random r = new System.Random();
                        int OpeningDialogue = r.Next(0, 3);
                        if (OpeningDialogue == 0)
                        {
                            CallHandler.Dialogue(CarDealerDialogue1, CarDealerMan);
                        }
                        else if (OpeningDialogue == 1)
                        {
                            CallHandler.Dialogue(CarDealerDialogue2, CarDealerMan);
                        }
                        else
                        {
                            CallHandler.Dialogue(CarDealerDialogue3, CarDealerMan);
                        }
                        CallHandler.Dialogue("~b~Car Dealer:~w~ The model is a " + Car.Model.Name + " and the license is " + Car.LicensePlate);
                        CallHandler.SafelyDelete(CarDealerManBlip);
                        Game.DisplayHelp("Go looking for the stolen Vehicle!");
                        CarDealerMan.Tasks.Clear();

                        SearchForVehicle();
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

        private void SearchForVehicle()
        {
            CarBlip = new Blip(Car.Position, 400f);
            CarBlip.Alpha = 0.65f;
            CarBlip.Color = Color.Yellow;
            updateBlip = GameFiber.StartNew(UpdateBlip);
            while (!CheckIfCloseToSuspect())
            {
                GameFiber.Yield();
            }
            ChooseEnding();
        }

        private void ChooseEnding()
        {
            int ending = new Random().Next(0, 1);
            if (ending == 0)
            {
                pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(pursuit, Suspect);
                Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                while (!Functions.IsPursuitStillRunning(pursuit))
                {
                    GameFiber.Yield();
                }
            }
            else
            {
                Suspect2 = CallHandler.SpawnMalePed(Car);
                Suspect2.Inventory.GiveNewWeapon(WeaponHash.APPistol,7000, true);
                pursuit = Functions.CreatePursuit();
                Suspect2.KeepTasks = true;
                Suspect2.Tasks.FightAgainst(player);
                Functions.AddPedToPursuit(pursuit, Suspect);
                Functions.AddPedToPursuit(pursuit, Suspect2);
                Functions.SetPursuitIsActiveForPlayer(pursuit, true);
                while (!Functions.IsPursuitStillRunning(pursuit))
                {
                    GameFiber.Yield();
                }
            }

            End();
        }

        private void UpdateBlip()
        {
            while (CalloutRunning && CarBlip != null)
            {
                GameFiber.Yield();
                if (CarBlip && CarBlip.Position.DistanceTo(Car) > 400f)
                {
                    CarBlip.Position = Car.Position;
                    Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Dispatch", "DynamicCallouts", "The stolen car was spotted by another unit. Redirect ASAP!");
                }
                if (found)
                {
                    CallHandler.SafelyDelete(CarBlip);
                    updateBlip.Abort();
                }
            }
        }

        private bool CheckIfCloseToSuspect()
        {
            if ((double) player.DistanceTo(Car) <= 15.0 && (double) ZDistanceToCar(player.Position.Z, Car.Position.Z) < 4.0)
            {
                Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Dispatch", "DynamicCallouts", "You found the stolen car! Apprehend the suspect!");
                found = true;
                return true;
            }
            return false;
        }

        private float ZDistanceToCar(float CarValue, float PlayerValue)
        {
            float num = CarValue - PlayerValue;
            return (float) Math.Sqrt((double) num * (double) num);
        }

        public override void End()
        {
            CallHandler.SafelyDelete(CarDealerMan);
            CallHandler.SafelyDelete(calloutLocationBlip);
            CallHandler.SafelyDelete(CarDealerManBlip);
            CallHandler.SafelyDelete(CarBlip);
            CallHandler.SafelyDelete(des_showroom_end);

            NativeFunction.Natives.REMOVE_IPL("carshowroom_broken");
            NativeFunction.Natives.REQUEST_IPL("shr_int");
            NativeFunction.Natives.REQUEST_IPL("fakeint");

            CalloutRunning = false;
            base.End();
        }
    }
}
