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
            DiceRoller.RollResult result;
            result = diceRoller.RollDice("(1d6+2)*3+2d4");
            //result = diceRoller.RollDice("1d6+2");
            //result = diceRoller.RollDice("4d6-L");
            Console.WriteLine(result);
        }
    }
}
