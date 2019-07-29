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
            notationPatterns[PatternType.DieFull] = $"{notationPatterns[PatternType.Die]}{notationPatterns[PatternType.Explode]}?(?:{notationPatterns[PatternType.NumberComparison]})?";
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
            return randomEngine.Next(1, sides + 1);
        }

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
                // Default fudge, 2 of each non-blank (1d3 - 2)
                total = randomEngine.Next(1, 4) - 2;
            }
            else if (numNonBlanks == 1)
            {
                // Only 1 of each non-blank
                // On a 1d6 a roll of 1 = -1, 6 = +1, others = 0
                int num = randomEngine.Next(1, 7);
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

        delegate int RollDelegate(int sides);

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


        private RollResult CalculateResult(List<object> dice, string initialSymbol = "+")
        {
            string symbol = initialSymbol;
            if (string.IsNullOrEmpty(initialSymbol))
            {
                symbol = "+";
            }
            int num = 0;

            RollResult result = new RollResult();

            // At this point we have a list of dice (Die), numbers (int) and operators (string)
            foreach (object element in dice)
            {
                if (element is List<object>)
                {
                    // It's a parenthesis group
                    RollResult parenthesisGroup = CalculateResult(element as List<object>, symbol);
                    result.RolledNotation += $"({parenthesisGroup.RolledNotation})";
                    result.Result += parenthesisGroup.Result;
                }
                else if (element is Die)
                {
                    // It's a die, roll it
                    Die d = (Die)element;
                    List<int> rolls = RollDie(d);
                    num = rolls.Sum();
                    string rollNotation = "[";
                    int compoundSum = 0;

                    if (d.Explode || d.Penetrate)
                    { 
                        for (int i = 0; i < rolls.Count; ++i)
                        {
                            // Write comma if needed
                            if (i > 0)
                            {
                                if (d.Compound)
                                {
                                    if (!IsComparePoint(d.ComparePoint, rolls[i - 1]))
                                    {
                                        // Die is exploding/penetrating and last number did not match compare point, add a comma because this is a different roll
                                        rollNotation += ",";
                                    }
                                }
                                else
                                {
                                    // There was a number before, add a coma
                                    rollNotation += ",";
                                }
                            }

                            if (!d.Compound)
                            {
                                // Write every number
                                rollNotation += rolls[i].ToString();
                            }
                            else
                            {
                                // Sum the number, we'll write it when the number doesn't match the compare point (last roll of the chain)
                                compoundSum += rolls[i];
                            }
                            // Die continues exploding/penetrating and is not compount
                            if (!d.Compound && IsComparePoint(d.ComparePoint, rolls[i]))
                            {
                                // It matches the compare point, add explode/penetrate symbol
                                rollNotation += d.Explode ? "!" : "!p";
                            }
                            else if (d.Compound && !IsComparePoint(d.ComparePoint, rolls[i]))
                            {
                                // Compound dice has finished exploding/penetrating, write number sum and add appropriate symbol
                                rollNotation += compoundSum.ToString();
                                if (compoundSum > d.Sides)
                                {
                                    rollNotation += d.Explode ? "!!" : "!!p";
                                }
                                compoundSum = 0;
                            }
                        }
                    }
                    else
                    {
                        // Do a simple parsing of the rolls
                        rollNotation += string.Join(",", rolls);
                    }


                    rollNotation += "]";

                    if (d.Compound)
                    {
                        // Instead replace
                    }

                    result.RolledNotation += rollNotation;
                }
                // It's a string that encodes a number or an operator
                else if (int.TryParse(element as string, out int temp))
                {
                    // It's a number
                    num = temp;
                    result.RolledNotation += num;
                }
                else
                {
                    symbol = element as string;
                    result.RolledNotation += symbol;
                }

                if (symbol != "" && num != 0)
                {
                    // Use last operator to combine with result
                    switch (symbol)
                    {
                        case ("+"):
                            result.Result += num;
                            break;
                        case ("-"):
                            result.Result -= num;
                            break;
                        case ("*"):
                            result.Result *= num;
                            break;
                        case ("/"):
                        case ("\\"):
                            result.Result /= num;
                            break;
                        default:
                            result.Result += num;
                            break;
                    }
                    // And clear the last symbol
                    symbol = "";
                    num = 0;
                }
            }

            return result;
        }

        public RollResult RollDice(string notation)
        {
            List<object> dice = ParseNotation(notation);
            RollResult result = CalculateResult(dice);
            result.OriginalNotation = notation;
            return result;
        }

        private List<object> ParseNotation(string notation)
        {
            List<object> parsed = new List<object>();
            
            // Only continue if a notation was passed
            if (!string.IsNullOrWhiteSpace(notation) && notation != "")
            {
                // Split the notation into its parenthesis groupings
                List<string> matches = Regex.Split(notation, "(\\(.*?\\))").ToList().Where(s => !String.IsNullOrWhiteSpace(s)).ToList();

                // Loop through each group and parse it
                for (int i = 0; i < matches.Count; ++i)
                {
                    if (matches[i][0] == '(')
                    {
                        // This is a match within the parenthesis group (i.e. "3d6+2" in "(3d6+2)*4"
                        // Recursively parse it in case it has nested parenthesis
                        matches[i] = matches[i].Replace("(", "").Replace(")", "");
                        parsed.Add(ParseNotation(matches[i]));
                    }
                    else
                    {
                        // This is a match outside of a parenthesis group (i.e. the "+2" in "(3d6)+2", or "d6" in "(2d4+2)*d6")
                        // or it could be that no parenthesis group exists (i.e. the whole notation in `3d6+2`)
                        // This also happens when recursively parsing down to a level with no parenthesis

                        // Split the notation by operator (include operators in the returned segments)
                        string[] segments = Regex.Split(matches[i], $"({notationPatterns[PatternType.ArithmeticOperator]})");

                        for (int j = 0; j < segments.Length; ++j)
                        {
                            // Determine if the segment is a die or not
                            if (Regex.IsMatch(segments[j], notationPatterns[PatternType.DieFull]))
                            {
                                // This is a die. Parse it into an object and add it to the list
                                var dice = ParseDie(segments[j]);
                                dice.ForEach(die =>
                                {
                                    parsed.Add(die);
                                });
                            }
                            else
                            {
                                // Not a die (i.e. number or operator)
                                /*if (int.TryParse(segments[j], out _))
                                {
                                    parsed.Add(segments[j]);
                                }*/
                                parsed.Add(segments[j]);

                            }                          
                        }
                    }
                }

            }
            return parsed;
        }

        private List<Die> ParseDie(string notation)
        {
            // Parse the notation and find valid dice and any attributes
            List<Die> parsed = new List<Die>();
            string pattern = notationPatterns[PatternType.Notation];

            MatchCollection notationMatches;
            notationMatches = Regex.Matches(notation, pattern);

            for (int i = 0; i < notationMatches.Count; ++i)
            {
                Match notationMatch = notationMatches[i];
                // Number of times to roll the die
                int quantity = 1;
                if (notationMatch.Groups[2].Length > 0 && int.TryParse(notationMatch.Groups[2].Value, out int temp))
                {
                    quantity = temp;
                }

                // How many sides the die has
                int sides = 0;
                string sidesString = "";
                if (int.TryParse(notationMatch.Groups[3].Value, out temp))
                {
                    sides = temp;
                }
                else
                {
                    sidesString = notationMatch.Groups[3].Value;
                }

                // If the number of sides was not a number it must been a fudge die (or a percentage, but we'll check that later)
                bool fudge = false;
                string fudgeString = "";
                if (sidesString.Length > 0)
                {
                    fudge = Regex.IsMatch(sidesString, notationPatterns[PatternType.Fudge]);
                    if (fudge)
                    {
                        fudgeString = Regex.Match(sidesString, notationPatterns[PatternType.Fudge]).Groups[0].Value;
                    }
                }

                // Whether to explode the dice rolls
                bool explode = notationMatch.Groups[5].Length > 0;

                // Whether to penetrate the dice rolls
                bool penetrate = new string[] { "!p", "!!p" }.Contains(notationMatch.Groups[5].Value);

                // Whether to compound exploding dice rolls
                bool compound = new string[] { "!!", "!!p" }.Contains(notationMatch.Groups[5].Value);

                // The compare point for exploding/penetrating dice
                ComparePoint comparePoint = new ComparePoint();
                if (notationMatch.Groups[6].Length > 0)
                {
                    comparePoint.Operator = notationMatch.Groups[6].Value;
                    if (!int.TryParse(notationMatch.Groups[7].Value, out comparePoint.Value))
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
                if (die.FudgeString.Length < 2 || !int.TryParse(die.FudgeString[1].ToString(), out sides))
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
                        //rerolls[rerollIndex] = rerolls[rerollIndex] + roll;
                        rerolls.Add(roll);

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
    }
}
