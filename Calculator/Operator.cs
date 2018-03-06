using System;
using System.Collections.Generic;

namespace martin2250.Calculator
{
	public abstract class Operator : Expression
	{
		public abstract int Precedence { get; }

		public List<Tuple<Expression, bool>> Operands = new List<Tuple<Expression, bool>>();
	}
}
