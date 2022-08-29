using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;

namespace DynamicCallouts.Utilities
{
    class EndCalloutHandler
    {
        public static bool CalloutForcedEnd = false;
        public static void EndCallout()
        {
            if (Settings.LeaveCalloutsRunning && !CalloutForcedEnd)
            {
                GameFiber.Wait(2000);
                Game.DisplayHelp("Press ~y~" + Settings.EndCall + " ~w~to ~b~Finish~w~ the Callout.");
                while (!Game.IsKeyDown(Settings.EndCall)) GameFiber.Wait(0);
                //End the Callout
            }
            else
            {
                if (!CalloutForcedEnd) GameFiber.Wait(2000);
                //End the Callout
            }
            CalloutForcedEnd = false;
        }
    }
}