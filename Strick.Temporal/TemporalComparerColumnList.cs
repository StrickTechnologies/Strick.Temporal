using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace Strick.Temporal
{
	/// <summary>
	/// Represent a sequence of columns for the DataTable specified in the given <see cref="TemporalComparer"/> object.
	/// </summary>
	public class TemporalComparerColumnList : ICollection<DataColumn>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TemporalComparerColumnList"/> class.
		/// </summary>
		/// <param name="comparer">The <see cref="TemporalComparer"/> object that contains the DataTable. Any column added to this collection must belong to that DataTable.</param>
		public TemporalComparerColumnList(TemporalComparer comparer) => (this.Comparer) = (comparer);


		private readonly List<DataColumn> columns = new List<DataColumn>();

		private TemporalComparer Comparer { get; }


		#region ICOLLECTION Interface

		/// <summary>
		/// The number of coumns contained in the collection.
		/// </summary>
		public int Count => columns.Count;

		/// <summary>
		/// <inheritdoc cref="ICollection{T}.IsReadOnly"/>
		/// </summary>
		public bool IsReadOnly => false;

		/// <summary>
		/// Adds a column to the collection. If the column does not belong to the DataTable contained in 
		/// </summary>
		public void Add(DataColumn column)
		{
			if (!Comparer.IsValidCol(column, false))
			{ throw new ArgumentException("Column does not belong to this table"); }

			columns.Add(column);
		}

		/// <summary>
		/// Removes all columns from the collection.
		/// </summary>
		public void Clear() => columns.Clear();

		/// <summary>
		/// Removes the first occurrence of <paramref name="column"/> from the collection.
		/// </summary>
		/// <param name="column"></param>
		/// <returns>true if the column is successfully removed. false if the column was not successfully remove, or was not found in the collection.</returns>
		public bool Remove(DataColumn column) => columns.Remove(column);


		/// <summary>
		/// Returns a boolean indicating whether or not <paramref name="column"/> is found in the collection.
		/// </summary>
		/// <param name="column"></param>
		public bool Contains(DataColumn column) => columns.Contains(column);


		/// <summary>
		/// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		public void CopyTo(DataColumn[] array, int arrayIndex) => columns.CopyTo(array, arrayIndex);

		/// <summary>
		/// Returns an enumerator that enumerates through the collection.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<DataColumn> GetEnumerator() => columns.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => columns.GetEnumerator();

		#endregion ICOLLECTION Interface


		/// <summary>
		/// Adds the column(s) represented by <paramref name="columnNames"/> to the collection. 
		/// See <see cref="Add(DataColumn)"/>
		/// </summary>
		/// <param name="columnNames">A sequence of column names.</param>
		public void AddRange(IEnumerable<string> columnNames)
		{
			foreach(string cNm in columnNames)
			{
				Add(Comparer.Table.Columns[cNm]);
			}
		}
	}
}
