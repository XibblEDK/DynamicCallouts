using Rage;
using LSPD_First_Response.Mod.API;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Collections;
using Rage.Native;
using System.Windows.Forms;
using System.Deployment.Internal;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System.Windows;
using System.Linq;

namespace DynamicCallouts.Utilities
{
    class CallHandler
    {
        public static bool locationReturned = true;
        private static int count;
        public static Vector3 SpawnPoint;
        static Random random = new Random();
        public static string enteredText;
        private static string[] VehicleModels;
        public static bool IsDialogueDone = false;

        private static string[,] FemaleCopAnim = new string[,] {
            {"amb@world_human_cop_idles@female@base", "base"},
            {"amb@world_human_cop_idles@female@idle_a", "idle_a" },
            {"amb@world_human_cop_idles@female@idle_a", "idle_b" },
            {"amb@world_human_cop_idles@female@idle_a", "idle_c" },
            {"amb@world_human_cop_idles@female@idle_b", "idle_d" },
            {"amb@world_human_cop_idles@female@idle_b", "idle_e" },
        };
        private static string[,] MaleCopAnim = new string[,] {
            {"amb@world_human_cop_idles@male@base", "base"},
            {"amb@world_human_cop_idles@male@idle_a", "idle_a" },
            {"amb@world_human_cop_idles@male@idle_a", "idle_b" },
            {"amb@world_human_cop_idles@male@idle_a", "idle_c" },
            {"amb@world_human_cop_idles@male@idle_b", "idle_d" },
            {"amb@world_human_cop_idles@male@idle_b", "idle_e" },
        };
        private static string[,] FemaleRandoAnim = new string[,] {
            {"amb@world_human_hang_out_street@female_arm_side@idle_a", "idle_a"},
            {"amb@world_human_hang_out_street@female_arm_side@idle_a", "idle_b"},
            {"amb@world_human_hang_out_street@female_arm_side@idle_a", "idle_c"},
            {"amb@world_human_hang_out_street@female_arms_crossed@idle_a", "idle_a"},
            {"amb@world_human_hang_out_street@female_arms_crossed@idle_a", "idle_b"},
            {"amb@world_human_hang_out_street@female_arms_crossed@idle_a", "idle_c"},
        };
        private static string[,] MaleRandoAnim = new string[,] {
            {"amb@world_human_hang_out_street@male_a@base", "base"},
            {"amb@world_human_hang_out_street@male_b@base", "base"},
            {"amb@world_human_hang_out_street@male_c@base", "base"},
        };

        public static ArrayList StoreList = new ArrayList()
        {
            new Vector3(-47.29313f, -1758.671f, 29.42101f),
            new Vector3(289f, -1267f, 29.44f),
            new Vector3(818f, -1039f, 26.75f),
            new Vector3(289f, -1267f, 29.44f),
            new Vector3(1211.76f, -1390f, 35.37f),
            new Vector3(1164.94f, -324.3139f, 69.22092f),
            new Vector3(-530f, -1220f, 18.45f),
            new Vector3(-711f, -917f, 19.21f),
            new Vector3(-2073f, -327f, 13.32f),
            new Vector3(527f, -151f, 57.46f),
            new Vector3(643f, 264.4f, 103.3f),
            new Vector3(1959.956f, 3740.31f, 32.34f),
            new Vector3(-1442f, -1993f, 13.164f),
            new Vector3(-93f, 6410.87f, 31.65f),
            new Vector3(1696.867f, 4923.803f, 42.06f),
            new Vector3(2557.269f, 380.7113f, 108.6229f),
            new Vector3(-3038f, 483.778f, 7.91f),
            new Vector3(-2545.63f, 2316.986f, 33.21579f),
        };

        public enum MarkerType
        {
            UpsideDownCone = 0,
            VerticalCylinder = 1,
            ThickChevronUp = 2,
            ThinChevronUp = 3,
            CheckeredFlagRect = 4,
            CheckeredFlagCircle = 5,
            VerticleCircle = 6,
            PlaneModel = 7,
            LostMCDark = 8,
            LostMCLight = 9,
            Number0 = 10,
            Number1 = 11,
            Number2 = 12,
            Number3 = 13,
            Number4 = 14,
            Number5 = 15,
            Number6 = 16,
            Number7 = 17,
            Number8 = 18,
            Number9 = 19,
            ChevronUpx1 = 20,
            ChevronUpx2 = 21,
            ChevronUpx3 = 22,
            HorizontalCircleFat = 23,
            ReplayIcon = 24,
            HorizontalCircleSkinny = 25,
            HorizontalCircleSkinny_Arrow = 26,
            HorizontalSplitArrowCircle = 27,
            DebugSphere = 28,
            DallorSign = 29,
            HorizontalBars = 30,
            WolfHead = 31
        };

        public enum eCombatMovement
        {
            CM_Stationary,
            CM_Defensive,
            CM_WillAdvance,
            CM_WillRetreat
        };

        public static void Dialogue(List<string> dialogue, Ped animped = null, String animdict = "missfbi3_party_d", String animname = "stand_talk_loop_a_male1", float animspeed = -1, AnimationFlags animflag = AnimationFlags.Loop)
        {
            count = 0;
            while (count < dialogue.Count)
            {
                GameFiber.Yield();
                if (Game.IsKeyDown(Settings.Dialog))
                {
                    if (animped != null && animped.Exists())
                    {
                        try
                        {
                            animped.Tasks.PlayAnimation(animdict, animname, animspeed, animflag);
                        }
                        catch (Exception) { }
                    }
                    Game.DisplaySubtitle(dialogue[count]);
                    count++;
                }
            }
            IsDialogueDone = true;
        }

        public static void DialogueWithoutAnim(List<string> dialogue)
        {
            count = 0;
            while (count < dialogue.Count)
            {
                GameFiber.Yield();
                if (Game.IsKeyDown(Settings.Dialog))
                {
                    Game.DisplaySubtitle(dialogue[count]);
                    count++;
                }
            }
        }

        public static void IdleAction(Ped ped, bool iscop)
        {
            if (ped != null && ped.Exists())
            {
                if (iscop)
                {
                    if (ped.IsFemale)
                    {
                        int animation = random.Next(0, FemaleCopAnim.Length / 2);
                        ped.Tasks.PlayAnimation(FemaleCopAnim[animation, 0], FemaleCopAnim[animation, 1], -1, AnimationFlags.Loop);
                    }
                    else
                    {

                        int animation = random.Next(0, MaleCopAnim.Length / 2);
                        ped.Tasks.PlayAnimation(MaleCopAnim[animation, 0], MaleCopAnim[animation, 1], -1, AnimationFlags.Loop);
                    }
                }
                else
                {
                    if (ped.IsFemale)
                    {

                        int animation = random.Next(0, FemaleRandoAnim.Length / 2);
                        ped.Tasks.PlayAnimation(FemaleRandoAnim[animation, 0], FemaleRandoAnim[animation, 1], -1, AnimationFlags.Loop);
                    }
                    else
                    {

                        int animation = random.Next(0, MaleRandoAnim.Length / 2);
                        ped.Tasks.PlayAnimation(MaleRandoAnim[animation, 0], MaleRandoAnim[animation, 1], -1, AnimationFlags.Loop);
                    }
                }
            }
        }

        public static void OpenDoor(Vector3 doorlocation, Ped resident = null, string residentmodel = "")
        {
            Game.DisplayHelp("Press ~y~" + Settings.InteractionKey1 + "~w~ to ~b~Ring~w~ the Doorbell.");
            while (!Game.IsKeyDown(Settings.InteractionKey1)) GameFiber.Wait(0);
            GameFiber.Wait(2500);
            Game.FadeScreenOut(1500, true);
            Game.LocalPlayer.HasControl = false;
            if (resident != null)
            {
                if (residentmodel != "") resident = new Ped(residentmodel, doorlocation, Game.LocalPlayer.Character.Heading - 180);
                else resident = new Ped(doorlocation, Game.LocalPlayer.Character.Heading - 180);
                resident.Heading = Game.LocalPlayer.Character.Heading - 180;
                IdleAction(resident, false);
            }
            GameFiber.Wait(1500);
            Game.FadeScreenIn(1500, true);
            Game.LocalPlayer.HasControl = true;
        }

        public static void EnterHouse(Vector3 doorLocation, Vector3 teleportTo)
        {
            GameFiber.Wait(1250);
            Game.FadeScreenOut(1500, true);
            Game.LocalPlayer.HasControl = false;
            Game.LocalPlayer.Character.Position = teleportTo;
            GameFiber.Wait(2500);
            Game.FadeScreenIn(1500, true);
            Game.LocalPlayer.HasControl = true;
        }

        public static void locationChooser(ArrayList list, float maxdistance = 600f, float mindistance = 25f)
        {
            ArrayList closeLocations = new ArrayList();
            Random random = new Random();
            for (int i = 1; i < list.Count; i++)
            {
                float distance = Vector3.Distance(Game.LocalPlayer.Character.Position, (Vector3)list[i]);
                if (distance <= maxdistance && distance >= mindistance)
                {
                    closeLocations.Add(list[i]);
                }
            }
            if (closeLocations.Count == 0)
            {
                locationReturned = false;
            }
            else
            {
                SpawnPoint = (Vector3)closeLocations[random.Next(0, closeLocations.Count)];
                locationReturned = true;
            }
        }

        public static Vector3 chooseNearestLocation(List<Vector3> list)
        {
            Vector3 closestLocation = list[0];
            float closestDistance = Vector3.Distance(Game.LocalPlayer.Character.Position, list[0]);
            for (int i = 1; i < list.Count; i++)
            {
                if (Vector3.Distance(Game.LocalPlayer.Character.Position, list[i]) <= closestDistance)
                {
                    closestDistance = Vector3.Distance(Game.LocalPlayer.Character.Position, list[i]);
                    closestLocation = list[i];
                }
            }
            return closestLocation;
        }

        public static int nearestLocationIndex(List<Vector3> list)
        {
            int closestLocationIndex = 0;
            float closestDistance = Vector3.Distance(Game.LocalPlayer.Character.Position, list[0]);
            for (int i = 1; i < list.Count; i++)
            {
                if (Vector3.Distance(Game.LocalPlayer.Character.Position, list[i]) <= closestDistance)
                {
                    closestDistance = Vector3.Distance(Game.LocalPlayer.Character.Position, list[i]);
                    closestLocationIndex = i;
                }
            }
            return closestLocationIndex;
        }

        public static void DrawMarker(MarkerType type, Vector3 position, Vector3 direction, Vector3 rotation, Vector3 scale, Color color, bool bobUpAndDown, bool faceCamera, bool rotate, bool drawOnEntities)
        {
            NativeFunction.Natives.DRAW_MARKER((int)type, position, direction, rotation, scale, color.R, color.G, color.B, color.A, bobUpAndDown, faceCamera, 2, rotate, 0, 0, drawOnEntities);
        }

        internal static string OpenTextInput(string windowTitle, string defaultText, int maxLength)
        {
            NativeFunction.Natives.DISABLE_ALL_CONTROL_ACTIONS(2);
            NativeFunction.Natives.DISPLAY_ONSCREEN_KEYBOARD(true, windowTitle, 0, defaultText, 0, 0, 0, maxLength);

            while (NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>() == 0)
            {
                GameFiber.Yield();
            }

            NativeFunction.Natives.ENABLE_ALL_CONTROL_ACTIONS(2);
            return NativeFunction.Natives.GET_ONSCREEN_KEYBOARD_RESULT<string>();
        }
        public static Vehicle SpawnVehicle(Vector3 SpawnPoint, float Heading, bool persistent = true)
        {
            VehicleModels = new string[63] {"asbo", "blista", "dilettante", "panto", "prairie", "cogcabrio", "exemplar", "f620", "felon", "felon2", "jackal", "oracle", "oracle2", "sentinel", "sentinel2",
            "zion", "zion2", "baller", "baller2", "baller3", "cavalcade", "fq2", "granger", "gresley", "habanero", "huntley", "mesa", "radi", "rebla", "rocoto", "seminole", "serrano", "xls", "asea", "asterope",
            "emporor", "fugitive", "ingot", "intruder", "premier", "primo", "primo2", "regina", "stanier", "stratum", "surge", "tailgater", "washington", "bestiagts", "blista2", "buffalo", "schafter2", "euros",
            "sadler", "bison", "bison2", "bison3", "burrito", "burrito2", "minivan", "minivan2", "paradise", "pony"};
            int model = random.Next(0, VehicleModels.Length);
            var veh = new Vehicle(VehicleModels[model], SpawnPoint, Heading);
            if (persistent) veh.IsPersistent = true; //vehicle is marked as persistent by default
            return veh;
        }
    }
}