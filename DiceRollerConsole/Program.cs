using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiceRollerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            DiceRoller.DiceRoller diceRoller = new DiceRoller.DiceRoller();
            diceRoller.RollDice("(1d6+2)*3+2d4");
        }
    }
}
