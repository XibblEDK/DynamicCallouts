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
using LSPD_First_Response.Engine.Scripting.Entities;
using System.Linq;

namespace DynamicCallouts.Callouts
{
    [CalloutInfo("[DYNC] House Raid", CalloutProbability.Low)]
    public class HouseRaid : Callout
    {
        List<Vector3> list = new List<Vector3>
        {
            new Vector3(171.012f, -1871.422f, 24.400f),
            new Vector3(114.228f, -1960.694f, 21.334f),
            new Vector3(20.977f, -1843.878f, 24.602f)
        };

        private Vector3 CalloutPlace;

        private Ped Suspect;
        private Ped Suspect1;
        private Ped Suspect2;
        private Ped DeadPerson;
        private Ped player = Game.LocalPlayer.Character;

        private Blip CalloutPlaceBlip;

        private int Scenario;

        private bool CalloutRunning = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutPlace = CallHandler.chooseNearestLocation(list);
            CalloutMessage = "[DYNC] House Raid";
            CalloutPosition = CalloutPlace;
            CalloutAdvisory = "Nothing Yet";

            World.GetAllPeds().ToList().ForEach(p => {
                if (p.DistanceTo(CalloutPlace) > 7f) p.Delete();
            });

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutDisplayed()
        {
            if (Main.CalloutInterface)
            {
                CalloutInterfaceFunctions.SendCalloutDetails(this, "CODE 3", "SWAT");
            }
            else
            {
                Game.DisplayNotification("Respond with ~r~Code 3~w~.");
            }

            base.OnCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            /*Suspect = new Ped(new Vector3(260.696f, -1003.191f, -99.009f), 52.376f);
            Suspect1 = new Ped(new Vector3(258.078f, -997.562f, -99.009f), 247.440f);
            Suspect2 = new Ped(new Vector3(259.116f, -998.805f, -99.009f), 274.179f);
            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;
            Suspect1.IsPersistent = true;
            Suspect1.BlockPermanentEvents = true;
            Suspect2.IsPersistent = true;
            Suspect2.BlockPermanentEvents = true;*/

            Random rYUY = new Random();
            Scenario = rYUY.Next(0, 3);
            if (Scenario == 0)
            {
                Suspect = new Ped(new Vector3(260.696f, -1003.191f, -99.009f), 52.376f);
                Suspect1 = new Ped(new Vector3(258.078f, -997.562f, -99.009f), 247.440f);
                Suspect2 = new Ped(new Vector3(259.116f, -998.805f, -99.009f), 274.179f);
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;
                Suspect1.IsPersistent = true;
                Suspect1.BlockPermanentEvents = true;
                Suspect2.IsPersistent = true;
                Suspect2.BlockPermanentEvents = true;
                Suspect.Inventory.GiveNewWeapon(WeaponHash.Pistol, 500, true);
                Suspect1.Inventory.GiveNewWeapon(WeaponHash.MicroSMG, 500, true);
                Suspect2.Inventory.GiveNewWeapon(WeaponHash.APPistol, 500, true);
                new RelationshipGroup("Suspects");
                new RelationshipGroup("Player");
                Suspect.RelationshipGroup = "Suspects";
                Suspect1.RelationshipGroup = "Suspects";
                Suspect2.RelationshipGroup = "Suspects";
                player.RelationshipGroup = "Player";
                Game.SetRelationshipBetweenRelationshipGroups("Suspects", "Player", Relationship.Hate);
            }
            else
            {
                Suspect = new Ped(new Vector3(260.696f, -1003.191f, -99.009f), 52.376f);
                Suspect1 = new Ped(new Vector3(258.078f, -997.562f, -99.009f), 247.440f);
                Suspect2 = new Ped(new Vector3(259.116f, -998.805f, -99.009f), 274.179f);
                Suspect.IsPersistent = true;
                Suspect.BlockPermanentEvents = true;
                Suspect1.IsPersistent = true;
                Suspect1.BlockPermanentEvents = true;
                Suspect2.IsPersistent = true;
                Suspect2.BlockPermanentEvents = true;
            }

            CalloutPlaceBlip = new Blip(CalloutPlace, 20f);
            CalloutPlaceBlip.Color = Color.Yellow;
            CalloutPlaceBlip.EnableRoute(Color.Yellow);
            CalloutPlaceBlip.Alpha = 0.65f;
            CalloutPlaceBlip.Name = "Search Area";

            //CallHandler.DrawMarker(CallHandler.MarkerType.VerticalCylinder, CalloutPlace, CalloutPlace, CalloutPlace, CalloutPlace.ExtensionAround(3f), Color.Blue, false, false, false, false);
            //NativeFunction.Natives.DRAW_MARKER(1, CalloutPlace.X, CalloutPlace.Y, CalloutPlace.Z, 0f, 0f, 0f, 0f, 0f, 0f, 1f, 1f, 1f, 255f, 255, 0, 4, false, false, 2, false, false, false, false);

            if (!CalloutRunning) Callout(); CalloutRunning = true;
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (Suspect) Suspect.Delete();
            if (Suspect1) Suspect1.Delete();
            if (Suspect2) Suspect2.Delete();
            if (CalloutPlaceBlip) CalloutPlaceBlip.Delete();
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
                        while (player.DistanceTo(CalloutPlace) >= 3f && !Game.IsKeyDown(Settings.EndCall)) GameFiber.Wait(0);
                        if (Game.IsKeyDown(Settings.EndCall)) { EndCalloutHandler.CalloutForcedEnd = true; break; }
                        OutsideDoor();
                        break;
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

        private void OutsideDoor()
        {
            if (CalloutRunning)
            {
                Game.DisplayHelp("Press ~y~" + Settings.InteractionKey1 + " ~w~to ~r~breach through door~w~.");
                while (!Game.IsKeyDown(Settings.InteractionKey1)) GameFiber.Wait(0);
                CallHandler.EnterHouse(CalloutPlace, new Vector3(266.139f, -1007.465f, -101.009f));
                if (Scenario == 0) { ShooterFight(); }
                else { SuspectsArentReadyHehe(); }
            }
        }

        private void ShooterFight()
        {
            /*NativeFunction.Natives.SET_PED_COMBAT_MOVEMENT(Suspect, CallHandler.eCombatMovement.CM_Defensive);
            NativeFunction.Natives.SET_PED_COMBAT_MOVEMENT(Suspect1, CallHandler.eCombatMovement.CM_Defensive);
            NativeFunction.Natives.SET_PED_COMBAT_MOVEMENT(Suspect2, CallHandler.eCombatMovement.CM_Defensive);*/
            Suspect.Tasks.FightAgainstClosestHatedTarget(1000f);
            Suspect1.Tasks.FightAgainstClosestHatedTarget(1000f);
            Suspect2.Tasks.FightAgainstClosestHatedTarget(1000f);
        }

        private void SuspectsArentReadyHehe()
        {
            End();
        }

        public override void End()
        {
            base.End();
            if (CalloutRunning)
            {
                Game.DisplayNotification("~g~Code 4~w~, return to patrol.");
                Functions.PlayScannerAudio("ATTENTION_ALL_UNITS WE_ARE_CODE_4");
            }

            if (Suspect) Suspect.Delete();
            if (Suspect1) Suspect1.Delete();
            if (Suspect2) Suspect2.Delete();
            if (CalloutPlaceBlip) CalloutPlaceBlip.Delete();
        }
    }
}
