using System;
using System.Collections.Generic;
using System.Globalization;

namespace martin2250.Calculator
{
	public class Variable : Expression
	{
		public object Value { get; private set; }

		public Variable(object value)
		{
			Value = value;
		}

		public override double GetValue(Dictionary<string, double> variables)
		{
			if (Value is string)
			{
				string valuestring = Value as string;

				if (variables.ContainsKey(valuestring))
					return variables[valuestring];

				if (double.TryParse(valuestring, NumberStyles.Float, NumberFormat, out double val))
					return val;

				throw new Exception("could not resolve variable");
			}
			else
				return Convert.ToDouble(Value, NumberFormat);
			
		}

		public override string ToString()
		{
			return Convert.ToString(Value, NumberFormat);
		}

		public override string ToStringDebug()
		{
			string s = "[";
			s += ToString();
			return s + "]";
		}

		public override Expression Simplify()
		{
			return this;
		}
	}
}
