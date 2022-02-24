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
	public class TemporalComparerColumnList : ICollection<DataColumn>
	{
		public TemporalComparerColumnList(TemporalComparer comparer) => (this.comparer) = (comparer);


		protected List<DataColumn> columns = new List<DataColumn>();

		protected TemporalComparer comparer { get; }


		#region ICOLLECTION Interface

		public int Count => columns.Count;

		public bool IsReadOnly => false;

		public void Add(DataColumn item)
		{
			if (!comparer.IsValidCol(item, false))
			{ throw new ArgumentException("Column does not belong to this table"); }

			columns.Add(item);
		}

		public void Clear() => columns.Clear();

		public bool Remove(DataColumn item) => columns.Remove(item);


		public bool Contains(DataColumn item) => columns.Contains(item);


		public void CopyTo(DataColumn[] array, int arrayIndex) => columns.CopyTo(array, arrayIndex);

		public IEnumerator<DataColumn> GetEnumerator() => columns.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => columns.GetEnumerator();

		#endregion ICOLLECTION Interface


		public void AddRange(IEnumerable<string> columnNames)
		{
			foreach(string cNm in columnNames)
			{
				Add(comparer.Table.Columns[cNm]);
			}
		}
	}
}
