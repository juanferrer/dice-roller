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
        public void SingleDie2()
        {
            General.RollTest("1d2", @"\[\d\]", 1, 2);
        }

        [TestMethod]
        public void SingleDie4()
        {
            General.RollTest("1d4", @"\[\d\]", 1, 4);
        }

        [TestMethod]
        public void SingleDie6()
        {
            General.RollTest("1d6", @"\[\d\]", 1, 6);
        }

        [TestMethod]
        public void SingleDie8()
        {
            General.RollTest("1d8", @"\[\d\]", 1, 8);
        }

        [TestMethod]
        public void SingleDie10()
        {
            General.RollTest("1d10", @"\[\d{1,2}\]", 1, 10);
        }

        [TestMethod]
        public void SingleDie12()
        {
            General.RollTest("1d12", @"\[\d{1,2}\]", 1, 12);
        }

        [TestMethod]
        public void SingleDie20()
        {
            General.RollTest("1d20", @"\[\d{1,2}\]", 1, 20);
        }

        [TestMethod]
        public void SingleDie100()
        {
            General.RollTest("1d100", @"\[\d{1,3}\]", 1, 100);
        }

        [TestMethod]
        public void SingleDiePercentage()
        {
            General.RollTest("1d%", @"\[\d{1,3}\]", 1, 100);
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
        public void MultipleDie2()
        {
            General.RollTest("2d2", @"\[\d(,\d)\]", 2, 4);
        }

        [TestMethod]
        public void MultipleDie4()
        {
            General.RollTest("3d4", @"\[\d(,\d){2}\]", 3, 12);
        }

        [TestMethod]
        public void MultipleDie6()
        {
            General.RollTest("4d6", @"\[\d(,\d){3}\]", 4, 24);
        }

        [TestMethod]
        public void MultipleDie8()
        {
            General.RollTest("5d8", @"\[\d(,\d){4}\]", 5, 40);
        }

        [TestMethod]
        public void MultipleDie10()
        {
            General.RollTest("6d10", @"\[\d{1,2}(,\d{1,2}){5}\]", 6, 60);
        }

        [TestMethod]
        public void MultipleDie12()
        {
            General.RollTest("7d12", @"\[\d{1,2}(,\d{1,2}){6}\]", 7, 84);
        }

        [TestMethod]
        public void MultipleDie20()
        {
            General.RollTest("8d20", @"\[\d{1,2}(,\d{1,2}){7}\]", 8, 160);
        }

        [TestMethod]
        public void MultipleDie100()
        {
            General.RollTest("9d100", @"\[\d{1,3}(,\d{1,3}){8}\]", 9, 900);
        }

        [TestMethod]
        public void MultiplePercentage()
        {
            General.RollTest("10d%", @"\[\d{1,3}(,\d{1,3}){9}\]", 10, 1000);
        }
    }
}
