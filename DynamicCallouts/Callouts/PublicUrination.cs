using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using System.Collections;
using DynamicCallouts.Utilities;
using System.Drawing;

namespace DynamicCallouts.Callouts
{
    [CalloutInfo("[DYNC] Public Urination", CalloutProbability.High)]
    public class PublicUrination : Callout
    {


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

        private Vector3 SuspectSpawnPoint;
        private string Zone;

        private Blip SuspectBlip;

        private Ped Victim;
        private Ped Suspect;
        private Ped player => Game.LocalPlayer.Character;

        private int MainScenario;
        private int SuspectAction;

        private bool CalloutRunning = false;

        private LHandle MainPursuit;

        private List<string> SuspectModels = new List<string>() { "u_m_y_militarybum", "a_m_o_soucent_02", "a_m_o_soucent_03", "a_m_o_tramp_01", "a_m_m_trampbeac_01" };

        public override bool OnBeforeCalloutDisplayed()
        {
            if (!Settings.PublicUrination)
            {
                Game.LogTrivial("[LOG]: User has disabled PublicUrination, returning false.");
                Game.LogTrivial("[LOG]: To enable the callout please check it in the menu or change false to true in the .ini file.");
                return false;
            }

            Zone = Functions.GetZoneAtPosition(player.Position).GameName;
            CallHandler.locationChooser(list);
            if (CallHandler.locationReturned) { SuspectSpawnPoint = CallHandler.SpawnPoint; } else { return false; }
            ShowCalloutAreaBlipBeforeAccepting(SuspectSpawnPoint, 75f);
            AddMinimumDistanceCheck(30f, SuspectSpawnPoint);
            Functions.PlayScannerAudio("CITIZENS_REPORT_01 CRIME_DISTURBING_THE_PEACE_01");
            CalloutMessage = "[DYNC] Public Urination";
            CalloutPosition= SuspectSpawnPoint;
            CalloutAdvisory = "Nothing yet.";

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
            Suspect = new Ped(SuspectModels[new Random().Next((int)SuspectModels.Count)], SuspectSpawnPoint, 0f);
            Suspect.IsPersistent= true;
            Suspect.BlockPermanentEvents = true;
            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.IsFriendly = false;
            SuspectBlip.IsRouteEnabled = true;
            SuspectBlip.RouteColor = Color.Yellow;
            SuspectBlip.Scale = 0.65f;
            SuspectBlip.Name = "Suspect";

            if (!CalloutRunning) Callout(); CalloutRunning = true;
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            Suspect.Delete();
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
        }
    }
}
