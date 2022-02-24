using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Strick.Temporal.Test
{
	public static class TemporalComparerTestExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="rowChange"></param>
		/// <param name="rowIndex"></param>
		/// <param name="colChangeCount"></param>
		/// <param name="key"></param>
		/// <param name="changeTime"></param>
		/// <param name="periodEndTime"></param>
		/// <param name="userID">If NOT null, checks the passed value against the UserID property of rowChange. Pass DBNull.Value to check for a null value in the UserID property that comes from the datatable. If null, userID is not tested. Default is null.</param>
		public static void CheckRowChange(this RowChange rowChange, int rowIndex, int colChangeCount = 1, object key = null, DateTime? changeTime = null, DateTime? periodEndTime = null, object userID = null)
		{
			Assert.IsNotNull(rowChange);

			Assert.AreEqual(rowIndex, rowChange.RowIndex);
			Assert.AreEqual(colChangeCount, rowChange.ColumnChanges.Count);

			//todo: fix after TemporalComparer changes complete...
			//if (key != null)
			//{ Assert.AreEqual(key, rowChange.Key); }

			if (changeTime != null)
			{ Assert.AreEqual(changeTime, rowChange.ChangeTime); }

			if (periodEndTime != null)
			{ Assert.AreEqual(periodEndTime, rowChange.PeriodEndTime); }

			if (userID != null)
			{ Assert.AreEqual(userID, rowChange.UserID); }
		}

		public static void CheckColChange(this ColChange colChange, int colIndex, object oldValue, object newValue)
		{
			Assert.IsNotNull(colChange);

			Assert.AreEqual(oldValue, colChange.OldValue);
			Assert.AreEqual(newValue, colChange.NewValue);
		}
	}
}
