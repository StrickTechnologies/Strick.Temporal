using System;
using System.Collections.Generic;
using System.Text;


namespace Strick.Temporal
{
	public class RowChange
	{
		public RowChange(int RowIndex, DateTime ChangeTime)
		{
			this.RowIndex = RowIndex;
			this.ChangeTime = ChangeTime;
		}


		/// <summary>
		/// The index of the row within the Rows collection of the Datatable object.
		/// </summary>
		public int RowIndex { get; }

		public DateTime ChangeTime { get; }

		public DateTime PeriodEndTime { get; set; }

		public object UserID { get; set; }

		public object Key { get; set; }

		public List<ColChange> ColumnChanges { get; } = new List<ColChange>();
	}
}
