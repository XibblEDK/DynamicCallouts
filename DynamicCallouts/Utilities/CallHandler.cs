using Rage;
using LSPD_First_Response.Mod.API;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Collections;

namespace DynamicCallouts.Utilities
{
    class CallHandler
    {
        public static bool locationReturned = true;
        private static int count;
        public static Vector3 SpawnPoint;
        static Random random = new Random();

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
                        //Game.LogTrivial("YOBBINCALLOUTS: There are "+MaleCopAnim.Length+"animations");
                        //Game.LogTrivial(MaleCopAnim[animation, 0]);
                        //Game.LogTrivial(MaleCopAnim[animation, 1]);
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
            else //test this (else wasn't here before)
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
    }
}