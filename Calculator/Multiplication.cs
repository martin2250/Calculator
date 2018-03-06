using System;
using System.Collections.Generic;
using System.Linq;

namespace martin2250.Calculator
{
	/// <summary>
	/// Represents a combination of multiplications and divisions
	/// bool in Operands.Item2 indicates if expressions should be inverted (divided by)
	/// </summary>
	public class Multiplication : Operator
	{
		public override int Precedence { get { return 1; } }

		public override double GetValue(Dictionary<string, double> variables)
		{
			if (Operands.Count == 0)
				throw new Exception("no operands");

			double value = 1.0;

			foreach (var operand in Operands)
			{
				if (operand.Item2)
					value /= operand.Item1.GetValue(variables);
				else
					value *= operand.Item1.GetValue(variables);
			}

			return value;
		}

		public override string ToString()
		{
			string s;

			if (Operands.First().Item2)
				s = "1";
			else
				s = "";

			for (int i = 0; i < Operands.Count; i++)
			{
				var operand = Operands[i];

				if (i != 0 || operand.Item2)
					s += operand.Item2 ? "/" : "*";

				if ((operand.Item1 as Operator)?.Precedence < this.Precedence)
					s += "(" + operand.Item1.ToString() + ")";
				else
					s += operand.Item1.ToString();
			}

			return s;
		}

		public override string ToStringDebug()
		{
			string s = "[";
			foreach(var operand in Operands)
			{
				s += operand.Item2 ? "/" : "*";
				s += operand.Item1.ToStringDebug();
			}
			return s + "]";
		}

		public override Expression Simplify()
		{
			// for a single operand that is not inverted, return the operand itself
			if (Operands.Count == 1 && !Operands.First().Item2)
				return Operands.First().Item1.Simplify();

			for(int i = 0; i < Operands.Count; i++)
			{
				// simplify each operand
				var operand = Operands[i];
				Operands[i] = new Tuple<Expression, bool>(operand.Item1.Simplify(), operand.Item2);

				// if operand is also a Multiplication, merge its operands with Operands
				if (operand.Item1 is Multiplication multioperand)
				{
					// remove original operand
					Operands.RemoveAt(i);
					i--;

					// insert the nested operands
					foreach(var nestedoperand in multioperand.Operands)
					{
						Operands.Insert(++i, new Tuple<Expression, bool>(nestedoperand.Item1.Simplify(), operand.Item2 ^ nestedoperand.Item2));
					}
				}
			}

			return this;
		}
	}
}
