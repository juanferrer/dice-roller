using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiceRollerTests
{
    [TestClass]
    public class SimpleDice
    {
        private DiceRoller.DiceRoller dieRoller;

        [TestInitialize]
        public void DiceRollerInitialize()
        {
            dieRoller = new DiceRoller.DiceRoller();
        }

        [TestMethod]
        public void TestSingleDice()
        {
            dieRoller.RollDice("1d6");
        }
    }
}
