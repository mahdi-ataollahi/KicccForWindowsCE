using System;
using System.Text;

namespace Kiccc.Ing.PcPos.Multiplex
{
	internal sealed class Verhoeff
	{
		private static Verhoeff _instance;

		private int[][] op = new int[10][];

		private int[] inv = new int[] { 0, 4, 3, 2, 1, 5, 6, 7, 8, 9 };

		private int[][] F = new int[8][];

		private static Verhoeff Instance
		{
			get
			{
				if (Verhoeff._instance == null)
				{
					Verhoeff._instance = new Verhoeff();
				}
				return Verhoeff._instance;
			}
		}

		static Verhoeff()
		{
			Verhoeff._instance = null;
		}

		private Verhoeff()
		{
			this.op[0] = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			this.op[1] = new int[] { 1, 2, 3, 4, 0, 6, 7, 8, 9, 5 };
			this.op[2] = new int[] { 2, 3, 4, 0, 1, 7, 8, 9, 5, 6 };
			this.op[3] = new int[] { 3, 4, 0, 1, 2, 8, 9, 5, 6, 7 };
			this.op[4] = new int[] { 4, 0, 1, 2, 3, 9, 5, 6, 7, 8 };
			this.op[5] = new int[] { 5, 9, 8, 7, 6, 0, 4, 3, 2, 1 };
			this.op[6] = new int[] { 6, 5, 9, 8, 7, 1, 0, 4, 3, 2 };
			this.op[7] = new int[] { 7, 6, 5, 9, 8, 2, 1, 0, 4, 3 };
			this.op[8] = new int[] { 8, 7, 6, 5, 9, 3, 2, 1, 0, 4 };
			this.op[9] = new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
			this.F[0] = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			this.F[1] = new int[] { 1, 5, 7, 6, 2, 8, 3, 0, 9, 4 };
			for (int index1 = 2; index1 < 8; index1++)
			{
				this.F[index1] = new int[10];
				for (int index2 = 0; index2 < 10; index2++)
				{
					this.F[index1][index2] = this.F[index1 - 1][this.F[1][index2]];
				}
			}
		}

		private int[] _AppendCheckDigit(int[] input)
		{
			int num = this._CalculateCheckDigit(input);
			int[] numArray = new int[(int)input.Length + 1];
			input.CopyTo(numArray, 0);
			numArray[(int)numArray.Length - 1] = num;
			return numArray;
		}

		private int _CalculateCheckDigit(int[] input)
		{
			int[] numArray = new int[(int)input.Length];
			for (int index = 0; index < (int)input.Length; index++)
			{
				numArray[index] = input[(int)input.Length - (index + 1)];
			}
			int index1 = 0;
			for (int index2 = 0; index2 < (int)numArray.Length; index2++)
			{
				index1 = this.op[index1][this.F[(index2 + 1) % 8][numArray[index2]]];
			}
			return this.inv[index1];
		}

		private bool _Check(int[] input)
		{
			int[] numArray = new int[(int)input.Length];
			for (int index = 0; index < (int)input.Length; index++)
			{
				numArray[index] = input[(int)input.Length - (index + 1)];
			}
			int index1 = 0;
			for (int index2 = 0; index2 < (int)numArray.Length; index2++)
			{
				index1 = this.op[index1][this.F[index2 % 8][numArray[index2]]];
			}
			return index1 == 0;
		}

		private bool _Check(int[] input, int checkDigit)
		{
			int[] input1 = new int[(int)input.Length + 1];
			input.CopyTo(input1, 0);
			input1[(int)input1.Length - 1] = checkDigit;
			return this._Check(input1);
		}

		private static int[] _ConvertToIntArray(string input)
		{
			int[] numArray = new int[input.Length];
			for (int startIndex = 0; startIndex < input.Length; startIndex++)
			{
				numArray[startIndex] = Convert.ToInt32(input.Substring(startIndex, 1));
			}
			return numArray;
		}

		private static int[] _ConvertToIntArray(long input)
		{
			return Verhoeff._ConvertToIntArray(input.ToString());
		}

		private static int[] _ConvertToIntArray(int input)
		{
			return Verhoeff._ConvertToIntArray(input.ToString());
		}

		private static long _ConvertToLong(int[] input)
		{
			long num1 = (long)0;
			long num2 = (long)1;
			for (int index = 0; index < (int)input.Length; index++)
			{
				num1 = num1 + (long)input[(int)input.Length - (index + 1)] * num2;
				num2 *= (long)10;
			}
			return num1;
		}

		public static string AppendCheckDigit(string input)
		{
			int[] numArray = Verhoeff.Instance._AppendCheckDigit(Verhoeff._ConvertToIntArray(input));
			StringBuilder stringBuilder = new StringBuilder();
			for (int index = 0; index < (int)numArray.Length; index++)
			{
				stringBuilder.Append(numArray[index]);
			}
			return stringBuilder.ToString();
		}

		public static long AppendCheckDigit(long input)
		{
			long num = Verhoeff._ConvertToLong(Verhoeff.Instance._AppendCheckDigit(Verhoeff._ConvertToIntArray(input)));
			return num;
		}

		public static int AppendCheckDigit(int input)
		{
			int num = (int)Verhoeff._ConvertToLong(Verhoeff.Instance._AppendCheckDigit(Verhoeff._ConvertToIntArray(input)));
			return num;
		}

		public static int[] AppendCheckDigit(int[] input)
		{
			return Verhoeff.Instance._AppendCheckDigit(input);
		}

		public static int CalculateCheckDigit(string input)
		{
			return Verhoeff.Instance._CalculateCheckDigit(Verhoeff._ConvertToIntArray(input));
		}

		public static int CalculateCheckDigit(long input)
		{
			return Verhoeff.Instance._CalculateCheckDigit(Verhoeff._ConvertToIntArray(input));
		}

		public static int CalculateCheckDigit(int input)
		{
			return Verhoeff.Instance._CalculateCheckDigit(Verhoeff._ConvertToIntArray(input));
		}

		public static int CalculateCheckDigit(int[] input)
		{
			return Verhoeff.Instance._CalculateCheckDigit(input);
		}

		public static bool Check(string input)
		{
			return Verhoeff.Instance._Check(Verhoeff._ConvertToIntArray(input));
		}

		public static bool Check(long input)
		{
			return Verhoeff.Instance._Check(Verhoeff._ConvertToIntArray(input));
		}

		public static bool Check(int input)
		{
			return Verhoeff.Instance._Check(Verhoeff._ConvertToIntArray(input));
		}

		public static bool Check(int[] input)
		{
			return Verhoeff.Instance._Check(input);
		}

		public static bool Check(string input, int checkDigit)
		{
			bool flag = Verhoeff.Instance._Check(Verhoeff._ConvertToIntArray(input), checkDigit);
			return flag;
		}

		public static bool Check(long input, int checkDigit)
		{
			bool flag = Verhoeff.Instance._Check(Verhoeff._ConvertToIntArray(input), checkDigit);
			return flag;
		}

		public static bool Check(int input, int checkDigit)
		{
			bool flag = Verhoeff.Instance._Check(Verhoeff._ConvertToIntArray(input), checkDigit);
			return flag;
		}

		public static bool Check(int[] input, int checkDigit)
		{
			return Verhoeff.Instance._Check(input, checkDigit);
		}
	}
}