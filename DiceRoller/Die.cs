using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceRoller
{
    public struct Die
    {
        public int Quantity;
        public int Sides;
        public string SidesString;
        public bool Fudge;
        public string FudgeString;
        public bool Explode;
        public bool Penetrate;
        public bool Compound;
        public ComparePoint ComparePoint;

        /// <summary>
        /// Check if this is a pool die (i.e. we have a compare point but are not exploding)
        /// </summary>
        /// <returns></returns>
        public bool Pool()
        {
            return !Explode && ComparePoint.Operator != null;
        }
    }
}
