using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace martin2250.Calculator
{
	public abstract class Expression
	{
		/// <summary>
		/// Evalutate the Expression to a double
		/// </summary>
		/// <param name="variables">a dictionary containing the variables, (name, value)</param>
		/// <returns>the result of the evaluation</returns>
		public abstract double GetValue(Dictionary<string, double> variables);

		/// <summary>
		/// Simplify Expressions, removes operations that only contain one operand, merges nested operations
		/// </summary>
		/// <returns>the simplified expression</returns>
		public abstract Expression Simplify();
	
		public abstract string ToStringDebug();

		/// <summary>
		/// the number format that is used for parsing numerical constants
		/// </summary>
		public static NumberFormatInfo NumberFormat = new NumberFormatInfo() { NumberDecimalSeparator = "." };		

		/// <summary>
		/// Convert a string to an Expression
		/// resulting Expression is already simplified
		/// </summary>
		/// <param name="input">the string to be parsed</param>
		/// <returns>the expression represented by input</returns>
		public static Expression Parse(string input)
		{
			input = new string(input.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).Select(c => Char.ToUpper(c)).ToArray());

			return ParseInternal(input).Simplify();
		}

		// only used internally
		enum CurrentlyParsing
		{
			None,
			Variable,
			Bracket
		}

		private static string OperandChars = "+-*/";
		private static string VariableChars = "0123456789.ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		
		// actual parser, not exposed directly because the input string is not cleaned
		private static Expression ParseInternal(string input)
		{
			// always start with an addition, this gets returned
			Addition addition = new Addition();

			CurrentlyParsing currentlyParsing = CurrentlyParsing.None;  // which kind of sub'module' is currently parsed
			int currentstart = 0;                                       // where the current sub'module' starts, only used when currently parsing bracket or variable
			int bracketdepth = 0;                                       // level of nested brackets, only used when parsing brackets
			char currentOperator = 'X';                                 // operator to be applied to the next operand, 'X' means operand not set

			// Add a new sub'module' to the final return value (addition)
			void AddThing(Expression ex)
			{
				if (addition.Operands.Count == 0)
				{
					// this is the first item to be added, only allow operators +, - and no operator
					if (currentOperator == '-')
						addition.Operands.Add(new Tuple<Expression, bool>(ex, true));
					else if ("X+".Contains(currentOperator))
						addition.Operands.Add(new Tuple<Expression, bool>(ex, false));
					else
						throw new Exception("no value left of operand");
				}
				else
				{
					// there are already operands in addition
					if (currentOperator == 'X')
						throw new Exception("no operand (this shouldn't happen I think)");
					else if ("*/".Contains(currentOperator))
					{
						// add a submodule to be multiplied
						if (addition.Operands.Last().Item1 is Multiplication)
						{
							// last operand is multiplication => append operand to multiplication
							Multiplication multi = addition.Operands.Last().Item1 as Multiplication;
							multi.Operands.Add(new Tuple<Expression, bool>(ex, currentOperator == '/'));
						}
						else
						{
							// last operand is not a multiplication => take last operand of addition, move it to new multiplication, add multiplication to original addition
							var left = addition.Operands.Last();
							addition.Operands.RemoveAt(addition.Operands.Count - 1);

							Multiplication multi = new Multiplication();
							multi.Operands.Add(new Tuple<Expression, bool>(left.Item1, false));
							multi.Operands.Add(new Tuple<Expression, bool>(ex, currentOperator == '/'));

							addition.Operands.Add(new Tuple<Expression, bool>(multi, left.Item2));
						}
					}
					else
						// operator is + or -, add new operand to main addition
						addition.Operands.Add(new Tuple<Expression, bool>(ex, currentOperator == '-'));
				}
				currentOperator = 'X';
			}

			// iterate through string, go up to input.Length (one further than normal), the last case is only used to finish a variable which is at the end of the string
			for (int i = 0; i <= input.Length; i++)
			{
				if (currentlyParsing == CurrentlyParsing.Variable)
				{
					// automatically terminate variable at the end of the string
					if (i == input.Length || !VariableChars.Contains(input[i]))
					{
						currentlyParsing = CurrentlyParsing.None;
						Variable variable = new Variable(input.Substring(currentstart, i - currentstart));
						AddThing(variable);
						// don't continue, so the following operator is parsed
					}
					else
						continue;
				}

				// last index of for loop is only used for variables, stop here
				if (i == input.Length)
					break;

				char c = input[i];

				if (currentlyParsing == CurrentlyParsing.Bracket)
				{
					// in/decrease bracket counter to find matching closing bracket
					if (c == ')')
					{
						bracketdepth--;

						// matching bracket found, pass content of bracket back to this function recursively
						if (bracketdepth == 0)
						{
							string bracket = input.Substring(currentstart, i - currentstart);
							AddThing(ParseInternal(bracket));
							currentlyParsing = CurrentlyParsing.None;
						}
					}
					else if (c == '(')
						bracketdepth++;

					continue;
				}

				if (VariableChars.Contains(c))
				{
					// start parsing a variable
					currentlyParsing = CurrentlyParsing.Variable;
					currentstart = i;
				}
				else if (OperandChars.Contains(c))
				{
					// set operand or throw exception
					if (currentOperator != 'X')
						throw new Exception("two successive operands");
					else
						currentOperator = c;
				}
				else if (c == '(')
				{
					// start parsing bracket
					currentlyParsing = CurrentlyParsing.Bracket;
					currentstart = i + 1;
					bracketdepth = 1;
				}
				else if (c == ')')
					// closing bracket found outside bracket parsing mode
					throw new Exception("no matching opening bracket found");
				else
					// other character not contained in allowed chars found
					throw new Exception("character not recognized");
			}

			// operator still set after parsing entire string
			if (currentOperator != 'X')
				throw new Exception("trailing operator found");

			// bracket was not finished
			if (bracketdepth != 0)
				throw new Exception("no matching closing bracket found");

			return addition;
		}
	}
}