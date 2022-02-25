using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Strick.Temporal.Test
{
	[TestClass]
	public class TemporalComparerColumnListTests
	{
		[TestMethod]
		public void LetsGo()
		{
			TemporalComparer tc = new TemporalComparer(EmployeeTestData.getDataTable());

			Assert.IsNotNull(tc);
			Assert.IsNotNull(tc.Table);


			var cols = tc.IncludedColumns;

			Assert.IsNotNull(cols);
			Assert.AreEqual(0, cols.Count);

			cols.Add(tc.Table.Columns[0]);
			Assert.AreEqual(1, cols.Count);


			Assert.ThrowsException<ArgumentException>(() => cols.Add(tc.Table.Columns["bogus"]));
			Assert.AreEqual(1, cols.Count);

			cols.Remove(tc.Table.Columns[0]);
			Assert.AreEqual(0, cols.Count);

			cols.Add(tc.Table.Columns[0]);
			Assert.AreEqual(1, cols.Count);
			cols.Add(tc.Table.Columns[1]);
			Assert.AreEqual(2, cols.Count);
			cols.Clear();
			Assert.AreEqual(0, cols.Count);
		}


		[TestMethod]
		public void AddRange()
		{
			TemporalComparer tc = new TemporalComparer(EmployeeTestData.getDataTable());

			Assert.IsNotNull(tc);
			Assert.IsNotNull(tc.Table);

			var cols = tc.IncludedColumns;
			Assert.IsNotNull(cols);
			Assert.AreEqual(0, cols.Count);

			tc.IncludedColumns.AddRange(new[] { "ID" });
			Assert.AreEqual(1, cols.Count);

			tc.IncludedColumns.Clear();
			Assert.IsNotNull(cols);
			Assert.AreEqual(0, cols.Count);

			tc.IncludedColumns.AddRange(new[] { "ID", "Name" });
			Assert.AreEqual(2, cols.Count);

			tc.IncludedColumns.Clear();
			Assert.IsNotNull(cols);
			Assert.AreEqual(0, cols.Count);

			Assert.ThrowsException<ArgumentException>(() => tc.IncludedColumns.AddRange(new[] { "bogus" }));
			Assert.IsNotNull(cols);
			Assert.AreEqual(0, cols.Count);

			Assert.ThrowsException<ArgumentException>(() => tc.IncludedColumns.AddRange(new[] { "ID", "bogus" }));
			Assert.IsNotNull(cols);
			//Note: the ID column gets added before it hits the bogus column.
			//      Not sure if this is how it *should* work or not, but this test checks that it's working the way it's coded.
			//      If this is ever determined to be a bug, or something we want to change, this unit test will obviously have to change
			Assert.AreEqual(1, cols.Count);

			tc.IncludedColumns.Clear();
			Assert.IsNotNull(cols);
			Assert.AreEqual(0, cols.Count);
			
			Assert.ThrowsException<ArgumentException>(() => tc.IncludedColumns.AddRange(new[] { "bogus", "ID" }));
			Assert.IsNotNull(cols);
			Assert.AreEqual(0, cols.Count);
		}
	}
}
