using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;


namespace Strick.Temporal
{
	/// <summary>
	/// </para>Iterates related rows in a DataTable (e.g. from a system versioned/temporal table) and returns a sequence containing information about the changes between related rows.</para>
	/// <para>Use the KeyColumn property to indicate which column is the key that designates "related" rows.  All rows with the same Key are considered "related".</para>
	/// <para>If KeyColumn is null (the default), ALL rows in the DataTable are assumed to be related.</para>
	/// <para>Rows in the DataTable are assumed to be ordered descending on SysStartTime (newest to oldest).</para>
	/// </summary>
	public class TemporalComparer
	{

		protected static readonly string StartTimeColName = "SysStartTime";
		protected static readonly string EndTimeColName = "SysEndTime";


		public TemporalComparer(DataTable Table) : this(Table, null) { }

		/// <summary>
		/// </summary>
		/// <param name="Table">The DataTable containing the rows to compare.</param>
		/// <param name="StartTimeColumn"></param>
		public TemporalComparer(DataTable Table, DataColumn StartTimeColumn)
		{
			this.Table = Table;

			if (StartTimeColumn == null)
			{
				if (Table.Columns[StartTimeColName] != null && Table.Columns[StartTimeColName].DataType == typeof(System.DateTime))
				{ StartTimeColumn = Table.Columns[StartTimeColName]; }
				else
				{ throw new ArgumentNullException($"StartTimeColumn is not specified, and default column ({StartTimeColName}) not found."); }
			}
			else
			{
				//throw an exception if the column doesn't belong to Table
				IsValidCol(StartTimeColumn, true);

				if (StartTimeColumn.DataType != typeof(System.DateTime))
				{ throw new ArgumentException("StartTimeColumn must be of DateTime type."); }
			}

			this.StartTimeColumn = StartTimeColumn;

			if (Table.Columns[EndTimeColName] != null && Table.Columns[EndTimeColName].DataType == typeof(System.DateTime))
			{ EndTimeColumn = Table.Columns[EndTimeColName]; }

			IncludedColumns = new TemporalComparerColumnList(this);
			ExcludedColumns = new TemporalComparerColumnList(this);
			KeyColumns = new TemporalComparerColumnList(this);
		}


		public DataTable Table { get; }


		/// <summary>
		/// Designates the period start column.  Required.  Must be a DateTime column.  If a suitable column is not designated in the constructor call, it 
		/// will search the DataTable for a column of DateTime type named "SysStartTime".  
		/// If found, that column is assumed to be the period start column.  If NOT found, an exception is thrown.
		/// </summary>
		public DataColumn StartTimeColumn { get; }

		/// <summary>
		/// Gets/sets a value which indicates whether or not to include the column designated by StartTimeColumn in the comparison.  
		/// Default is false.
		/// </summary>
		public bool IncludeStartTimeColumn { get; set; } = false;


		protected DataColumn etcol;

		/// <summary>
		/// Optional.  Designates the period end column.  Upon instantiation, will search the DataTable for a column of DateTime type named "SysEndTime".  
		/// If found, that column is assumed to be the period end column.  If NOT found, EndTimeColumn remains set to null, but can be 
		/// overridden in user code.
		/// </summary>
		public DataColumn EndTimeColumn
		{
			get => etcol;

			set
			{
				if (value == null || IsValidCol(value, true))
				{ etcol = value; }
			}
		}

		/// <summary>
		/// Gets/sets a value which indicates whether or not to include the column designated by EndTimeColumn in the comparison.  
		/// Default is false.  Ignored if EndTimeColumn is null.
		/// </summary>
		public bool IncludeEndTimeColumn { get; set; } = false;

		public bool HasEndTimeColumn => EndTimeColumn != null;


		protected DataColumn uidcol;

		/// <summary>
		/// Designates the column containing an ID for the User who made the changes.  Leave null if there is no such column in the DataTable.
		/// </summary>
		public DataColumn UserIDColumn
		{
			get => uidcol;

			set
			{
				if (value == null || IsValidCol(value, true))
				{ uidcol = value; }
			}
		}

		/// <summary>
		/// Gets/sets a value which indicates whether or not to include the column designated by UserIDColumn in the comparison.  
		/// Default is false.  Ignored if UserIDColumn is NOT set.
		/// </summary>
		public bool IncludeUserIDColumn { get; set; } = false;

		public bool HasUserIDColumn => UserIDColumn != null;


		/// <summary>
		/// Indicates which column is the key that designates "related" rows.  All rows with the same Key are considered "related".  
		/// Only related rows will be compared to one another.  
		/// If null (the default), ALL rows in the DataTable are assumed to be related.
		/// ** Multi-column keys are not supported at this time. **
		/// </summary>
		public TemporalComparerColumnList KeyColumns { get; }

		public bool HasKey => KeyColumns != null && KeyColumns.Count > 0;

		/// <summary>
		/// Gets/sets a value which indicates whether or not to include the column designated by KeyColumn in the comparison.  
		/// Default is false.  Ignored if KeyColumn is null.
		/// </summary>
		public bool IncludeKeyColumn { get; set; } = false;


		/// <summary>
		/// Returns a List&lt;DataColumn&gt; which contains the columns to process.  
		/// If no columns are specified (the default), all columns are processed.  
		/// See also ExcludedColumns property.
		/// If BOTH IncludedColumns and ExcludedColumns have specified columns, IncludedColumns takes precedent, and ExcludedColumns is ignored.
		/// </summary>
		public TemporalComparerColumnList IncludedColumns { get; }


		/// <summary>
		/// Returns a List&lt;DataColumn&gt; which contains the columns to skip.  
		/// See also IncludedColumns property.
		/// If BOTH IncludedColumns and ExcludedColumns have specified columns, IncludedColumns takes precedent, and ExcludedColumns is ignored.
		/// </summary>
		public TemporalComparerColumnList ExcludedColumns { get; }


		protected IEnumerable<DataColumn> Cols;

		protected IEnumerable<DataColumn> GetCols()
		{
			if (IncludedColumns != null && IncludedColumns.Count > 0)
			{
				//The columns to compare have been specified...
				return (from DataColumn c in Table.Columns select c).Where(c => IncludedColumns.Contains(c) || IncludeCol(c, false));
			}

			if (ExcludedColumns != null && ExcludedColumns.Count > 0)
			{
				//Compare all columns except those specified
				return (from DataColumn c in Table.Columns select c).Where(c => !ExcludedColumns.Contains(c) && IncludeCol(c, true));
			}


			return (from DataColumn c in Table.Columns select c).Where(c => IncludeCol(c, true));
		}

		protected bool IncludeCol(DataColumn Col, bool Default)
		{
			if (!IsValidCol(Col, false))
			{ return false; }


			if (Col == UserIDColumn)
			{ return IncludeUserIDColumn; }

			if (Col == StartTimeColumn)
			{ return IncludeStartTimeColumn; }

			if (Col == EndTimeColumn)
			{ return IncludeEndTimeColumn; }

			//todo: what to do about this???
			//if (Col == KeyColumns)
			//{ return IncludeKeyColumn; }


			return Default;
		}


		/// <summary>
		/// <para>Returns an <see cref="IEnumerable{}"/> of type <see cref="RowChange"/> containing information about the changes between related rows in the DataTable.</para>
		/// <para><b>** The returned sequence of changes is NOT cached -- the comparison is run each time this property is accessed.</b>  
		/// If you need to access the returned changes multiple times, cache the returned value, for example: 
		/// <code>IEnumerable&lt;RowChg&gt; MyChanges = myTemporalComparer.Changes;</code>
		/// </para>
		/// </summary>
		public IEnumerable<RowChange> Changes => GetChanges();

		protected IEnumerable<RowChange> GetChanges()
		{
			//if no table or only one record
			if (Table == null || Table.Rows.Count < 2)
			{ yield break; }

			Cols = GetCols();

			//assumes rows ordered newest to oldest...
			for (int r = Table.Rows.Count - 1; r > 0; r--)
			{
				if (HasKey)
				{
					//when key changes, find next two matching rows
					while (r > 0 && !KeysEqual(Table.Rows[r], Table.Rows[r - 1]))
					{ r--; }
					if (r == 0)
					{ break; }
				}

				RowChange chg = CompareRows(Table.Rows[r], Table.Rows[r - 1]);
				if (chg != null)
				{ yield return chg; }
			}
		}


		private RowChange CompareRows(DataRow OldRow, DataRow NewRow)
		{
			RowChange rc = null;

			foreach (DataColumn col in Cols)
			{
				if (!NewRow[col.Ordinal].Equals(OldRow[col.Ordinal]))
				{
					if (rc == null)
					{
						rc = new RowChange(NewRow.Table.Rows.IndexOf(NewRow), (DateTime)NewRow[StartTimeColumn]);

						if (EndTimeColumn != null)
						{ rc.PeriodEndTime = (DateTime)NewRow[EndTimeColumn]; }

						if (UserIDColumn != null)
						{ rc.UserID = NewRow[UserIDColumn]; }

						//todo:do we need this???
						//if (KeyColumns != null)
						//{ rc.Key = NewRow[KeyColumns]; }
					}

					rc.ColumnChanges.Add(new ColChange(NewRow.Table.Columns.IndexOf(col), col.ColumnName, OldRow[col], NewRow[col]));
				}
			}

			return rc;
		}

		private bool KeysEqual(DataRow OldRow, DataRow NewRow)
		{
			//should NOT get here in this case, so just a failsafe...
			if(!HasKey)
			{ return true; }

			foreach(DataColumn col in KeyColumns)
			{
				if (!NewRow[col.Ordinal].Equals(OldRow[col.Ordinal]))
				{ return false; }
			}

			return true;
		}


		/// <summary>
		/// Checks Col for null and that it belongs to Table.  
		/// If the Throw argument is true, throws an exception if Col is null or does not belong to Table.
		/// If the Throw argument is false, returns true if Col is not null and belongs to Table, and returns false if either condition is not met.
		/// </summary>
		internal bool IsValidCol(DataColumn Col, bool Throw)
		{
			if (Col == null)
			{
				if (Throw)
				{ throw new ArgumentNullException("Null column is invalid"); }
				else
				{ return false; }
			}

			if (Col.Table != Table) //ensure the column belongs to Table
			{
				if (Throw)
				{ throw new ArgumentException("Column does not belong to this table"); }
				else
				{ return false; }
			}


			return true;
		}
	}
}
