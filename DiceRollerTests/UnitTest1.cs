using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiceRoller;
using System.Text.RegularExpressions;

namespace DiceRollerTests
{
    public static class General
    {
        private static DiceRoller.DiceRoller dieRoller;
        public static void Prepare()
        {
            dieRoller = new DiceRoller.DiceRoller();
        }
        public static void RollTest(string roll, string regex, int min, int max)
        {
            RollResult result = dieRoller.RollDice(roll);
            Assert.IsTrue(min <= result.Result && result.Result <= max);
            Assert.IsTrue(Regex.IsMatch(result.RolledNotation, regex));
        }

        public static void RollTest(string roll, string regex, double min, double max)
        {
            RollResult result = dieRoller.RollDice(roll);
            Assert.IsTrue(min <= result.Result && result.Result <= max);
            Assert.IsTrue(Regex.IsMatch(result.RolledNotation, regex));
        }
    }

    [TestClass]
    public class SingleDice
    {
        [TestInitialize]
        public void DiceRollerInitialize()
        {
            General.Prepare();
        }        

        [TestMethod]
        public void Die2()
        {
            General.RollTest("1d2", @"\[\d\]", 1, 2);
        }

        [TestMethod]
        public void Die4()
        {
            General.RollTest("1d4", @"\[\d\]", 1, 4);
        }

        [TestMethod]
        public void Die6()
        {
            General.RollTest("1d6", @"\[\d\]", 1, 6);
        }

        [TestMethod]
        public void Die8()
        {
            General.RollTest("1d8", @"\[\d\]", 1, 8);
        }

        [TestMethod]
        public void Die10()
        {
            General.RollTest("1d10", @"\[\d{1,2}\]", 1, 10);
        }

        [TestMethod]
        public void Die12()
        {
            General.RollTest("1d12", @"\[\d{1,2}\]", 1, 12);
        }

        [TestMethod]
        public void Die20()
        {
            General.RollTest("1d20", @"\[\d{1,2}\]", 1, 20);
        }

        [TestMethod]
        public void Die100()
        {
            General.RollTest("1d100", @"\[\d{1,3}\]", 1, 100);
        }

        [TestMethod]
        public void DiePercentage()
        {
            General.RollTest("1d%", @"\[\d{1,3}\]", 1, 100);
        }

        [TestMethod]
        public void Die2Fudge()
        {
            General.RollTest("1dF", @"\[-?\d\]", -1, 1);
        }

        [TestMethod]
        public void Die4Fudge()
        {
            General.RollTest("1dF.1", @"\[-?\d\]", -1, 1);
        }
    }

    [TestClass]
    public class MultipleDie
    {
        [TestInitialize]
        public void DiceRollerInitialize()
        {
            General.Prepare();
        }

        [TestMethod]
        public void Dice2()
        {
            General.RollTest("2d2", @"\[\d(,\d)\]", 2, 4);
        }

        [TestMethod]
        public void Dice4()
        {
            General.RollTest("3d4", @"\[\d(,\d){2}\]", 3, 12);
        }

        [TestMethod]
        public void Dice6()
        {
            General.RollTest("4d6", @"\[\d(,\d){3}\]", 4, 24);
        }

        [TestMethod]
        public void Dice8()
        {
            General.RollTest("5d8", @"\[\d(,\d){4}\]", 5, 40);
        }

        [TestMethod]
        public void Dice10()
        {
            General.RollTest("6d10", @"\[\d{1,2}(,\d{1,2}){5}\]", 6, 60);
        }

        [TestMethod]
        public void Dice12()
        {
            General.RollTest("7d12", @"\[\d{1,2}(,\d{1,2}){6}\]", 7, 84);
        }

        [TestMethod]
        public void Dice20()
        {
            General.RollTest("8d20", @"\[\d{1,2}(,\d{1,2}){7}\]", 8, 160);
        }

        [TestMethod]
        public void Dice100()
        {
            General.RollTest("9d100", @"\[\d{1,3}(,\d{1,3}){8}\]", 9, 900);
        }

        [TestMethod]
        public void DicePercentage()
        {
            General.RollTest("10d%", @"\[\d{1,3}(,\d{1,3}){9}\]", 10, 1000);
        }


        [TestMethod]
        public void Dice2Fudge()
        {
            General.RollTest("11dF", @"\[-?\d(,-?\d){10}\]", -11, 11);
        }

        [TestMethod]
        public void Dice4Fudge()
        {
            General.RollTest("12dF.1", @"\[-?\d(,-?\d){11}\]", -12, 12);
        }
    }

    [TestClass]
    public class ModifiedDice
    {
        [TestInitialize]
        public void DiceRollerInitialize()
        {
            General.Prepare();
        }

        [TestMethod]
        public void DicePlus()
        {
            General.RollTest("2d6+2", @"\[\d\]\+2", 4, 14);
        }

        [TestMethod]
        public void DieMinus()
        {
            General.RollTest("5d10-10", @"\[\d{1,2}(,\d{1,2}){4}\]-10", 0, 40);
        }

        [TestMethod]
        public void DieTimes()
        {
            General.RollTest("3d%*3", @"\[\d{1,3}(,\d{1,3}){2}\]\*3", 9, 900);
        }

        [TestMethod]
        public void DieDivided()
        {
            General.RollTest("1d2/2", @"\[[12]\]\/2", 0.5, 1);
        }
    }
}
