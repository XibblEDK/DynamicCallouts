using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace DynamicCallouts.Utilities
{
    internal class Pos4
    {
        private Vector4 posandheading;

        internal Ped CreatePed(Model model)
        {
            return new Ped(model, this.Position, this.Heading);
        }

        internal Vehicle CreateVehicle(Model model)
        {
            var returnveh = new Vehicle(model, this.Position, this.Heading);
            returnveh.RandomizePlate();
            return returnveh;
        }

        internal Pos4(float x, float y, float z, float heading)
        {
            posandheading = new Vector4(x, y, z, heading);
        }

        internal Pos4(Vector3 pos, float heading)
        {
            posandheading = new Vector4(pos, heading);
        }

        internal Pos4(string[] strarray)
        {
            if (strarray.Length != 4)
            {
                throw new Exception("Cannot convert stringarray with a length other than 4 to Pos4.");
            }
            var fltlist = new List<float>();

            foreach (string item in strarray)
            {
                var currentfloat = new float();
                if (!float.TryParse(item, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out currentfloat))
                {
                    throw new Exception("Cannot convert string in stringarray to float for use in Pos4.");
                }
                fltlist.Add(currentfloat);
            }
            posandheading = new Vector4(fltlist[0], fltlist[1], fltlist[2], fltlist[3]);
        }

        internal Pos4 Copy()
        {
            var newPos4 = new Pos4(this.X, this.Y, this.Z, this.Heading);
            return newPos4;
        }

        internal float X
        {
            get { return posandheading.X; }
            set { posandheading.X = value; }
        }

        internal float Y
        {
            get { return posandheading.Y; }
            set { posandheading.Y = value; }
        }

        internal float Z
        {
            get { return posandheading.Z; }
            set { posandheading.Z = value; }
        }

        internal float Heading
        {
            get { return posandheading.W; }
            set { posandheading.W = value; }
        }

        internal Vector3 Position
        {
            get { return new Vector3(posandheading.X, posandheading.Y, posandheading.Z); }
            set
            {
                posandheading.X = value.X;
                posandheading.Y = value.Y;
                posandheading.Z = value.Z;
            }
        }
    }
}
