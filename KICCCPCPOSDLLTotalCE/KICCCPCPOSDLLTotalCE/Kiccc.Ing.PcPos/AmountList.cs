using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Kiccc.Ing.PcPos
{
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[ComVisible(true)]
	[Guid("B7DE252E-8AB0-4972-BB1B-186A5C8899CB")]
	[ProgId("Kiccc.Ing.PcPos.AmountList")]
	[Serializable]
	public class AmountList : IAmountList
	{
		private Queue _q = new Queue();

		public string AmountDeduction
		{
			get
			{
				int num = int.Parse(this.TotalAmount) / 1000;
				return num.ToString();
			}
		}

		public int Count
		{
			get
			{
				return this._q.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public int MaxSize
		{
			get
			{
				return 10;
			}
		}

		public string MultiplexAmount
		{
			get
			{
				string result = string.Empty;
				object[] array = this._q.ToArray();
				for (int i = 0; i < (int)array.Length; i++)
				{
					object amount = array[i];
					result = string.Concat(result, amount.ToString().PadLeft(12, '0'));
				}
				return result;
			}
		}

		public string TotalAmount
		{
			get
			{
				int result = 0;
				object[] array = this._q.ToArray();
				for (int i = 0; i < (int)array.Length; i++)
				{
					result += Convert.ToInt32(array[i]);
				}
				return result.ToString();
			}
		}

		public AmountList()
		{
		}

		public void Add(string amount)
		{
			AmountList.ValidateAmount(amount);
			this._q.Enqueue(amount);
		}

		public string Remove()
		{
			return (string)this._q.Dequeue();
		}

		private static void ValidateAmount(string amount)
		{
			if (amount == null)
			{
				throw new Exception("Invalid Amount");
			}
			if (!Regex.IsMatch(amount, "^[0-9]{1,12}$"))
			{
				throw new Exception("Invalid Amount");
			}
		}
	}
}