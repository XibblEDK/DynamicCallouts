using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicCallouts.Utilities
{
    /* HOW TO USE:
      
         First make a new marker:
            private Marker m;

         To make the marker do:
            m = new Marker(Position, Color, true);

         To delete the marker do:
            m.Dispose();
     */

    internal class MarkerDrawer
    {
        public Color Color { get; set; }
        public bool Visible { get; set; }
        public Vector3 Position { get; set; }

        private bool loop = true;
        private GameFiber process;

        private enum MarkerTypes
        {
            MarkerTypeUpsideDownCone = 0,
            MarkerTypeVerticalCylinder = 1,
            MarkerTypeThickChevronUp = 2,
            MarkerTypeThinChevronUp = 3,
            MarkerTypeCheckeredFlagRect = 4,
            MarkerTypeCheckeredFlagCircle = 5,
            MarkerTypeVerticleCircle = 6,
            MarkerTypePlaneModel = 7,
            MarkerTypeLostMCDark = 8,
            MarkerTypeLostMCLight = 9,
            MarkerTypeNumber0 = 10,
            MarkerTypeNumber1 = 11,
            MarkerTypeNumber2 = 12,
            MarkerTypeNumber3 = 13,
            MarkerTypeNumber4 = 14,
            MarkerTypeNumber5 = 15,
            MarkerTypeNumber6 = 16,
            MarkerTypeNumber7 = 17,
            MarkerTypeNumber8 = 18,
            MarkerTypeNumber9 = 19,
            MarkerTypeChevronUpx1 = 20,
            MarkerTypeChevronUpx2 = 21,
            MarkerTypeChevronUpx3 = 22,
            MarkerTypeHorizontalCircleFat = 23,
            MarkerTypeReplayIcon = 24,
            MarkerTypeHorizontalCircleSkinny = 25,
            MarkerTypeHorizontalCircleSkinny_Arrow = 26,
            MarkerTypeHorizontalSplitArrowCircle = 27,
            MarkerTypeDebugSphere = 28
        };

        public MarkerDrawer(Vector3 pos, Color color, bool visible = true)
        {
            Position = pos;
            Color = color;

            Visible = visible;
            process = new GameFiber(Process);
            process.Start();
        }

        public void Dispose()
        {
            loop = false;
            process.Abort();
        }

        private void Process()
        {
            while (loop)
            {
                GameFiber.Yield();

                if (!Visible) continue;

                Rage.Native.NativeFunction.CallByName<uint>(
                    "DRAW_MARKER",
                    (int)MarkerTypes.MarkerTypeUpsideDownCone,

                    Position.X,
                    Position.Y,
                    Position.Z,

                    Position.X,
                    Position.Y,
                    Position.Z,

                    0.0f, 0.0f, 0.0f,

                    1.0f, 1.0f, 1.0f,

                    (int)Color.R,
                    (int)Color.G,
                    (int)Color.B,
                    100,
                    false, true,
                    "", "",
                    false);
            }
        }
    }
}
