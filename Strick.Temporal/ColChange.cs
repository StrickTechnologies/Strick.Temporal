using System;
using System.Collections.Generic;
using System.Data;
using System.Text;


namespace Strick.Temporal
{
	/// <summary>
	/// Represents a change in a column between two related rows in your DataTable.
	/// </summary>
	public class ColChange
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ColChange"/> class using the provided values.
		/// </summary>
		/// <param name="ColumnIndex"></param>
		/// <param name="ColumnName"></param>
		/// <param name="Caption"></param>
		/// <param name="OldValue"></param>
		/// <param name="NewValue"></param>
		public ColChange(int ColumnIndex,string ColumnName, string Caption, object OldValue, object NewValue)
		{
			this.ColumnIndex = ColumnIndex;
			this.ColumnName = ColumnName;
			this.Caption = Caption;
			this.OldValue = OldValue;
			this.NewValue = NewValue;
		}

		/// <summary>
		/// The index of the column within the Columns collection of the Datatable object.  
		/// This also correlates to the index of the column within each Datarow object in the Datatable.Rows collection.
		/// </summary>
		public int ColumnIndex { get; }

		/// <summary>
		/// The name of the column. See <see cref="DataColumn.ColumnName"/>.
		/// </summary>
		public string ColumnName { get; }

		/// <summary>
		/// The caption of the column. See <see cref="DataColumn.Caption"/>. 
		/// If the Caption property is not specifically set in the DataColumn, it will default to the same value as <see cref="ColumnName"/>.
		/// </summary>
		public string Caption { get; }

		/// <summary>
		/// The column's value <b>before</b> the change.
		/// </summary>
		public object OldValue { get; }

		/// <summary>
		/// The column's value <b>after</b> the change.
		/// </summary>
		public object NewValue { get; }
	}
}
