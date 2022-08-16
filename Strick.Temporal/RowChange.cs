using System;
using System.Collections.Generic;
using System.Text;


namespace Strick.Temporal
{
	/// <summary>
	/// Represents a row in a datatable that has changed from its prior version
	/// </summary>
	public class RowChange
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RowChange"/> class.
		/// </summary>
		/// <param name="RowIndex">The index of the row within the Rows collection of the Datatable object.</param>
		/// <param name="ChangeTime">The time the row was updated</param>
		public RowChange(int RowIndex, DateTime ChangeTime)
		{
			this.RowIndex = RowIndex;
			this.ChangeTime = ChangeTime;
		}


		/// <summary>
		/// The index of the row within the Rows collection of the Datatable object.
		/// </summary>
		public int RowIndex { get; }

		/// <summary>
		/// The time the row was updated, or the start of the period for which the row was valid.
		/// </summary>
		public DateTime ChangeTime { get; }

		/// <summary>
		/// The end of the period for which the row was valid.
		/// </summary>
		public DateTime PeriodEndTime { get; set; }

		/// <summary>
		/// The value from the column specified by the <see cref="TemporalComparer.UserIDColumn"/>. 
		/// If a column was not specified for <see cref="TemporalComparer.UserIDColumn"/>, this value will be null.
		/// </summary>
		public object UserID { get; set; }

		/// <summary>
		/// The value(s) that comprise the key value for the row. The values are from the column(s) specified by <see cref="TemporalComparer.KeyColumns"/>. 
		/// If no columns were specified for <see cref="TemporalComparer.KeyColumns"/>, this value will be null.
		/// </summary>
		public List<object> Key { get; set; }

		/// <summary>
		/// A sequence containing the individual column changes for the row.
		/// </summary>
		public List<ColChange> ColumnChanges { get; } = new List<ColChange>();
	}
}
