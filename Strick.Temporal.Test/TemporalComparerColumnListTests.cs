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
			//todo:write unit tests here...
		}
	}
}
