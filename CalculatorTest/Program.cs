using System;
using System.Collections.Generic;
using martin2250.Calculator;

namespace CalculatorTest
{
	class Program
	{
		static void Main(string[] args)
		{
			Dictionary<string, double> vars = new Dictionary<string, double>();
			vars.Add("test", 42);
			vars.Add("pi", Math.PI);
			vars.Add("e", Math.E);
			
			Console.WriteLine("-------------------------");

			while (true)
			{
				string input = Console.ReadLine();

				if (input == "q" || input == "exit")
					break;

				try
				{
					Expression ex = Expression.Parse(input);
					Console.WriteLine("Parsed To:");
					Console.WriteLine(ex.ToString());
					Console.WriteLine("Value: {0}", ex.GetValue(vars));
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}

				Console.WriteLine("-------------------------");
			}
		}
	}
}
