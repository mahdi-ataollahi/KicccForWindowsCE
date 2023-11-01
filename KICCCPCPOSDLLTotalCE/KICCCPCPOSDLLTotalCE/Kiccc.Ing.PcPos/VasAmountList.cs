using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Kiccc.Ing.PcPos
{
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[ComVisible(true)]
	[Guid("AC4177AF-0871-4ABF-BB09-EA41573FCDEC")]
	[ProgId("Kiccc.Ing.PcPos.VasAmountList")]
	[Serializable]
	public class VasAmountList : IVasAmountList
	{
		private readonly List<VasAmountItem> _amounts = new List<VasAmountItem>();

		public int Count
		{
			get
			{
				return this._amounts.Count;
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
				this._amounts.Sort((VasAmountItem p1, VasAmountItem p2) => p1.Order.CompareTo(p2.Order));
				string result = string.Empty;
				foreach (VasAmountItem vasAmountItem in this._amounts)
				{
					long amount = vasAmountItem.Amount;
					result = string.Concat(result, amount.ToString().PadLeft(12, '0'));
				}
				return result;
			}
		}

		public string TotalAmount
		{
			get
			{
				long result = (long)0;
				foreach (VasAmountItem vasAmountItem in this._amounts)
				{
					result += vasAmountItem.Amount;
				}
				return result.ToString();
			}
		}

		public VasAmountList()
		{
		}

		public void Add(long amount, int order)
		{
			VasAmountList.ValidateAmount(amount.ToString());
			VasAmountList.ValidateOrder(order.ToString());
			if (this._amounts.Count == this.MaxSize)
			{
				throw new Exception("Queue Is Full");
			}
			if (this._amounts.Find((VasAmountItem item) => item.Order == order) != null)
			{
				throw new Exception("order violated");
			}
			this._amounts.Add(new VasAmountItem()
			{
				Order = order,
				Amount = amount
			});
		}

		public bool Remove(long amount, int order)
		{
			bool flag;
			foreach (VasAmountItem vasAmountItem in this._amounts)
			{
				if ((vasAmountItem.Order != order ? false : vasAmountItem.Amount == amount))
				{
					this._amounts.Remove(vasAmountItem);
					flag = true;
					return flag;
				}
			}
			flag = false;
			return flag;
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

		private static void ValidateOrder(string order)
		{
			if (order == null)
			{
				throw new Exception("Invalid Order");
			}
			if (!Regex.IsMatch(order, "^[0-9]{1}$"))
			{
				throw new Exception("Invalid Order");
			}
		}
	}
}