using System;
using System.Collections.Generic;
using System.Text;


namespace Strick.Temporal
{
	public class ColChg
	{
		public ColChg(int ColumnIndex,string ColumnName, object OldValue, object NewValue)
		{
			this.ColumnIndex = ColumnIndex;
			this.ColumnName = ColumnName;
			this.OldValue = OldValue;
			this.NewValue = NewValue;
		}

		/// <summary>
		/// The index of the column within the Columns collection of the Datatable object.  
		/// This also correlates to the index of the column within each Datarow object in the Datatable.Rows collection.
		/// </summary>
		public int ColumnIndex { get; }

		public string ColumnName { get; }

		public object OldValue { get; }

		public object NewValue { get; }
	}
}
