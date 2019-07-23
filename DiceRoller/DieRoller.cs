using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DiceRoller
{
    public class DiceRoller
    {
        private enum PatternType
        {
            ArithmeticOperator,
            ComparisonOperator,
            Fudge,
            NumberComparison,
            Explode,
            Die,
            DieFull,
            Operation,
            Notation,
            NumberDecimal
        }

        private Random randomEngine;
        private const int seed = 0;

        // Notation patterns
        private Dictionary<PatternType, string> notationPatterns = new Dictionary<PatternType, string>();

        public DiceRoller(int customSeed = seed)
        {
            randomEngine = new Random(seed);

            // Simple patterns
            notationPatterns[PatternType.ArithmeticOperator] = "[+\\-*\\/]";
            notationPatterns[PatternType.ComparisonOperator] = "[<>!]?={1,3}|[<>]";
            notationPatterns[PatternType.Fudge] = "F(?:\\.([12]))?";
            notationPatterns[PatternType.Explode] = "(!{1,2}p?)";
            notationPatterns[PatternType.NumberDecimal] = "\\d+(?:\\.\\d+)?";
            // Compound patterns
            notationPatterns[PatternType.NumberComparison] = $"({notationPatterns[PatternType.ComparisonOperator]})({notationPatterns[PatternType.NumberDecimal]})";
            notationPatterns[PatternType.Die] = $"([1-9]\\d*)?d([1-9]\\d*|%|{notationPatterns[PatternType.Fudge]})";
            notationPatterns[PatternType.DieFull] = $"{notationPatterns[PatternType.Die]}{notationPatterns[PatternType.Explode]}?(?:${notationPatterns[PatternType.NumberComparison]})?";
            notationPatterns[PatternType.Operation] = $"({notationPatterns[PatternType.ArithmeticOperator]})({notationPatterns[PatternType.NumberDecimal]}(?!\\d*d)|H|L)";
            notationPatterns[PatternType.Notation] = $"({notationPatterns[PatternType.ArithmeticOperator]})?{notationPatterns[PatternType.DieFull]}((?:{notationPatterns[PatternType.Operation]})*)";
        }

        /// <summary>
        /// Roll a standard die with the specified number of faces
        /// </summary>
        /// <param name="sides"></param>
        /// <returns></returns>
        private int Standard(int sides)
        {
            return randomEngine.Next(1, sides);
        }

        delegate int RollDelegate(int sides);

        /// <summary>
        /// Roll a fudge die with the specified number of non-blank faces
        /// </summary>
        /// <param name="numNonBlanks"></param>
        /// <returns></returns>
        private int Fudge(int numNonBlanks)
        {
            int total = 0;

            if (numNonBlanks == 2)
            {
                total = randomEngine.Next(1, 3) - 2;
            }
            else if (numNonBlanks == 1)
            {
                int num = randomEngine.Next(1, 6);
                if (num == 1)
                {
                    total = -1;
                }
                else if (num == 6)
                {
                    total = 1;
                }
            }

            return total;
        }

        /// <summary>
        /// Compares a to b with the given operator
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="o">A valid comparison operator (=, <, >, <=, >=, !=) </param>
        /// <returns></returns>
        private bool CompareNumbers(int a, int b, string o)
        {
            bool result;

            switch (o)
            {
                case "=":
                case "==":
                    result = a == b;
                    break;
                case "<":
                    result = a < b;
                    break;
                case ">":
                    result = a > b;
                    break;
                case "<=":
                    result = a <= b;
                    break;
                case ">=":
                    result = a >= b;
                    break;
                case "!":
                case "!=":
                    result = a != b;
                    break;
                default:
                    result = false;
                    break;
            }

            return result;

        }

        /// <summary>
        /// Check whether value matches the given compare point
        /// </summary>
        /// <param name="comparePoint"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsComparePoint(ComparePoint comparePoint, int value)
        {
            return comparePoint.Operator != null ? CompareNumbers(value, comparePoint.Value, comparePoint.Operator) : false;
        }


        private List<Die> ParseDice(string notation)
        {
            // Parse the notation and find valid dice and any attributes
            List<Die> parsed = new List<Die>();
            string pattern = notationPatterns[PatternType.Notation];

            Regex regex = new Regex(pattern);
            Match notationMatch;

            while ((notationMatch = regex.Match(notation)) != null)
            {
                // Number of times to roll the die
                int quantity = 1;
                if (notationMatch.Captures[2].Length > 0 && int.TryParse(notationMatch.Captures[2].Value, out int temp))
                {
                    quantity = temp;
                }

                // How many sides the die has
                int sides = 0;
                string sidesString = "";
                if (int.TryParse(notationMatch.Captures[3].Value, out temp))
                {
                    sides = temp;
                }
                else
                {
                    sidesString = notationMatch.Captures[3].Value;
                }

                // If the number of sides was not a number it must been a fudge die (or a percentage, but we'll check that later)
                bool fudge = false;
                string fudgeString = "";
                if (sidesString.Length > 0)
                {
                    fudge = Regex.IsMatch(sidesString, notationPatterns[PatternType.Fudge]);
                    if (fudge)
                    {
                        fudgeString = Regex.Match(sidesString, notationPatterns[PatternType.Fudge]).Captures[0].Value;
                    }
                }

                // Whether to explode the dice rolls
                bool explode = notationMatch.Captures[5].Length > 0;

                // Whether to penetrate the dice rolls
                bool penetrate = new string[] { "!p", "!!p" }.Contains(notationMatch.Captures[5].Value);

                // Whether to compound exploding dice rolls
                bool compound = new string[] { "!!", "!!p" }.Contains(notationMatch.Captures[5].Value);

                // The compare point for exploding/penetrating dice
                ComparePoint comparePoint = new ComparePoint();
                if (notationMatch.Captures[6].Length > 0)
                {
                    comparePoint.Operator = notationMatch.Captures[6].Value;
                    if (!int.TryParse(notationMatch.Captures[7].Value, out comparePoint.Value))
                    {
                        comparePoint.Value = 0;
                    }
                }
                else if (explode)
                {
                    comparePoint.Operator = "=";
                    comparePoint.Value = fudge ? 1 : ((sidesString == "%") ? 100 : sides);
                }
                parsed.Add(new Die()
                {
                    Quantity = quantity,
                    Sides = sides,
                    SidesString = sidesString,
                    Fudge = fudge,
                    FudgeString = fudgeString,
                    Explode = explode,
                    Penetrate = penetrate,
                    Compound = compound,
                    ComparePoint = comparePoint
                });
            }

            return parsed;
        }

        /// <summary>
        /// Roll a single die for its quantity and return an array with the results
        /// </summary>
        /// <param name="die"></param>
        /// <returns></returns>
        private List<int> RollDie(Die die)
        {
            List<int> dieRolls = new List<int>();
            RollDelegate callback = this.Standard;

            int sides = die.Sides;
            // Ensure roll quantity is valid
            die.Quantity = (die.Quantity > 0) ? die.Quantity : 1;

            // Check for non-numerical dice formats
            if (die.Fudge)
            {
                // We have a fudge dice. Set the callback to fudge
                callback = this.Fudge;

                // Set the sides to the correct value for the fudge type
                if (!int.TryParse(die.FudgeString[1].ToString(), out sides))
                {
                    // Well, it's not a number. Use default
                    sides = 2;
                }
            }
            else if (die.SidesString == "%")
            {
                // It's a percentile
                sides = 100;
            }

            if (sides > 0)
            {
                List<int> rerolls;
                int rollCount = 0;
                int rerollIndex = 0;
                int roll = 0;
                // Loop through and roll for the quantity
                for (int i = 0; i < die.Quantity; ++i)
                {
                    rerolls = new List<int> ();
                    rollCount = 0;
                    roll = 0;
                    rerollIndex = 0;

                    // Roll the die once, then check if it exploded and keep rolling until it stops
                    do
                    {
                        rerollIndex = rerolls.Count;
                        // Get total roll on this die
                         roll = callback.Invoke(sides);

                        // Add the roll to our list
                        rerolls[rerollIndex] = rerolls[rerollIndex] + roll;

                        // Subtract 1 from penetrated rolls (only consecutive rolls, after initial roll are not subtracted)
                        if (die.Penetrate && rollCount > 0)
                        {
                            rerolls[rerollIndex]--;
                        }

                        rollCount++;
                    } while (die.Explode && IsComparePoint(die.ComparePoint, roll));

                    // Add the rolls
                    dieRolls.AddRange(rerolls);
                }
            }

            return dieRolls;
        }

        /// <summary>
        /// Roll dice according to the specified string, then sum and return the result
        /// </summary>
        /// <param name="rollString">A string following the Dice notation: https://en.wikipedia.org/wiki/Dice_notation </param>
        /// <returns></returns>
        public int RollDice(string rollString/*, out string rollBreakdown*/)
        {
            int result = 0;

            // Separate the roll string into parentheses groups
            string[] parenthesesGroups = rollString.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

            // Separate each parenthesesGroup into single rollable elements or modifiers (i.e. separate in arithmetic operations)
            List<List<string>> rollableElements = new List<List<string>>();
            parenthesesGroups.ToList<string>().ForEach(g =>
            {
                rollableElements.Add(Regex.Split(g, @"([+\-*\/])").Where(e => !string.IsNullOrEmpty(e)).ToList());
            });

            // Roll each element in each group and add the result
            rollableElements.ForEach(group =>
            {
                group.ForEach(element =>
                {

                });
            });

            return result;
        }
    }
}
