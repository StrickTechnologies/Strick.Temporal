using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Strick.Temporal.Test
{
	[TestClass]
	public class TemporalComparerTests
	{
		[TestMethod]
		public void TestMethod1()
		{
			//Assert.AreEqual(tbl.Rows[rc.RowIndex + 1][cc.ColumnIndex], (decimal)cc.OldValue);
			//Assert.AreEqual(tbl.Rows[rc.RowIndex][cc.ColumnIndex], (decimal)cc.NewValue);

			TemporalComparer tc = EETest();
			Assert.IsNotNull(tc);

			var tbl = tc.Table;
			Assert.IsNotNull(tbl);


			EE_AllChanges(tc);
			EE_SalaryChanges(tc);
			EE_JobTitleChanges(tc);
			EE_TerminationChanges(tc);
			EE_NoTerminationChanges(tc);
		}

		private void ResetComparerState(TemporalComparer tc)
		{
			//ensure temporal comparer state is correct
			tc.IncludedColumns.Clear();
			Assert.AreEqual(0, tc.IncludedColumns.Count);
			tc.ExcludedColumns.Clear();
			Assert.AreEqual(0, tc.ExcludedColumns.Count);
		}


		//ALL changes
		private void EE_AllChanges(TemporalComparer tc)
		{
			//ensure temporal comparer state is correct
			ResetComparerState(tc);

			var rowChanges = tc.Changes.ToList();

			Assert.IsNotNull(rowChanges);
			Assert.AreEqual(7, rowChanges.Count());

			var rc = rowChanges[0];
			rc.CheckRowChange(7, 1, 2, new DateTime(2016, 3, 31, 16, 35, 17), new DateTime(2017, 3, 31, 13, 49, 36), "Jack");
			rc.ColumnChanges[0].CheckColChange(3, 21.64m, 22.72m);

			rc = rowChanges[1];
			rc.CheckRowChange(6, 1, 2);
			rc.ColumnChanges[0].CheckColChange(3, 22.72m, 23.86m);

			rc = rowChanges[2];
			rc.CheckRowChange(5, 2, 2);
			rc.ColumnChanges[0].CheckColChange(2, jtHTech, jtHSuper);
			rc.ColumnChanges[1].CheckColChange(3, 23.86m, 26.25m);

			rc = rowChanges[3];
			rc.CheckRowChange(3, 1, 1);
			rc.ColumnChanges[0].CheckColChange(3, 22.12m, 23.23m);

			rc = rowChanges[4];
			rc.CheckRowChange(2, 2, 1);
			rc.ColumnChanges[0].CheckColChange(2, jtHTech, jtHSuper);
			rc.ColumnChanges[1].CheckColChange(3, 23.23m, 25.55m);

			rc = rowChanges[5];
			rc.CheckRowChange(1, 1, 1);
			rc.ColumnChanges[0].CheckColChange(3, 25.55m, 26.83m);

			rc = rowChanges[6];
			rc.CheckRowChange(0, 2, 1);
			rc.ColumnChanges[0].CheckColChange(5, DBNull.Value, new DateTime(2017, 5, 31));
			rc.ColumnChanges[1].CheckColChange(6, DBNull.Value, "Left for another job");
		}

		//Salary changes
		private void EE_SalaryChanges(TemporalComparer tc)
		{
			ResetComparerState(tc);
			tc.IncludedColumns.Add(tc.Table.Columns["Salary"]);
			Assert.AreEqual(1, tc.IncludedColumns.Count);

			var rowChanges = tc.Changes.ToList();
			Assert.IsNotNull(rowChanges);
			Assert.AreEqual(6, rowChanges.Count);

			var rc = rowChanges[0];
			rc.CheckRowChange(7, 1, 2);
			rc.ColumnChanges[0].CheckColChange(3, 21.64m, 22.72m);

			rc = rowChanges[1];
			rc.CheckRowChange(6, 1, 2);
			rc.ColumnChanges[0].CheckColChange(3, 22.72m, 23.86m);

			rc = rowChanges[2];
			rc.CheckRowChange(5, 1, 2);
			rc.ColumnChanges[0].CheckColChange(3, 23.86m, 26.25m);

			rc = rowChanges[3];
			rc.CheckRowChange(3, 1, 1);
			rc.ColumnChanges[0].CheckColChange(3, 22.12m, 23.23m);

			rc = rowChanges[4];
			rc.CheckRowChange(2, 1, 1);
			rc.ColumnChanges[0].CheckColChange(3, 23.23m, 25.55m);

			rc = rowChanges[5];
			rc.CheckRowChange(1, 1, 1);
			rc.ColumnChanges[0].CheckColChange(3, 25.55m, 26.83m);
		}

		//Job title changes
		private void EE_JobTitleChanges(TemporalComparer tc)
		{
			ResetComparerState(tc);
			tc.IncludedColumns.Add(tc.Table.Columns["JobTitle"]);
			Assert.AreEqual(1, tc.IncludedColumns.Count);

			var rowChanges = tc.Changes.ToList();
			Assert.IsNotNull(rowChanges);
			Assert.AreEqual(2, rowChanges.Count());

			var rc = rowChanges[0];
			rc.CheckRowChange(5, 1, 2);
			rc.ColumnChanges[0].CheckColChange(2, jtHTech, jtHSuper);

			rc = rowChanges[1];
			rc.CheckRowChange(2, 1, 1);
			rc.ColumnChanges[0].CheckColChange(2, jtHTech, jtHSuper);
		}

		//Terminations
		private void EE_TerminationChanges(TemporalComparer tc)
		{
			ResetComparerState(tc);
			tc.IncludedColumns.Add(tc.Table.Columns["TerminationDate"]);
			Assert.AreEqual(1, tc.IncludedColumns.Count);

			var rowChanges = tc.Changes.ToList();
			Assert.IsNotNull(rowChanges);
			Assert.AreEqual(1, rowChanges.Count());
		}

		//EXCLUDE Terminations
		private void EE_NoTerminationChanges(TemporalComparer tc)
		{
			ResetComparerState(tc);

			tc.ExcludedColumns.Add(tc.Table.Columns["TerminationDate"]);
			tc.ExcludedColumns.Add(tc.Table.Columns["TermReason"]);
			Assert.AreEqual(2, tc.ExcludedColumns.Count);

			var rowChanges = tc.Changes.ToList();
			Assert.IsNotNull(rowChanges);
			Assert.AreEqual(6, rowChanges.Count());

			var rc = rowChanges[0];
			rc.CheckRowChange(7, 1, 2, new DateTime(2016, 3, 31, 16, 35, 17), new DateTime(2017, 3, 31, 13, 49, 36), "Jack");
			rc.ColumnChanges[0].CheckColChange(3, 21.64m, 22.72m);

			rc = rowChanges[1];
			rc.CheckRowChange(6, 1, 2);
			rc.ColumnChanges[0].CheckColChange(3, 22.72m, 23.86m);

			rc = rowChanges[2];
			rc.CheckRowChange(5, 2, 2);
			rc.ColumnChanges[0].CheckColChange(2, jtHTech, jtHSuper);
			rc.ColumnChanges[1].CheckColChange(3, 23.86m, 26.25m);

			rc = rowChanges[3];
			rc.CheckRowChange(3, 1, 1);
			rc.ColumnChanges[0].CheckColChange(3, 22.12m, 23.23m);

			rc = rowChanges[4];
			rc.CheckRowChange(2, 2, 1);
			rc.ColumnChanges[0].CheckColChange(2, jtHTech, jtHSuper);
			rc.ColumnChanges[1].CheckColChange(3, 23.23m, 25.55m);

			rc = rowChanges[5];
			rc.CheckRowChange(1, 1, 1);
			rc.ColumnChanges[0].CheckColChange(3, 25.55m, 26.83m);
		}

		//EXCLUDE Salary changes
		private void EE_NoSalaryChanges(TemporalComparer tc)
		{
			ResetComparerState(tc);
			tc.ExcludedColumns.Add(tc.Table.Columns["Salary"]);
			Assert.AreEqual(1, tc.ExcludedColumns.Count);

			var rowChanges = tc.Changes.ToList();
			Assert.IsNotNull(rowChanges);
			Assert.AreEqual(3, rowChanges.Count());

			var rc = rowChanges[0];
			rc.CheckRowChange(5, 1, 2);
			rc.ColumnChanges[0].CheckColChange(2, jtHTech, jtHSuper);

			rc = rowChanges[1];
			rc.CheckRowChange(2, 1, 1);
			rc.ColumnChanges[0].CheckColChange(2, jtHTech, jtHSuper);

			rc = rowChanges[2];
			rc.CheckRowChange(0, 2, 1);
			rc.ColumnChanges[0].CheckColChange(5, DBNull.Value, new DateTime(2017, 5, 31));
			rc.ColumnChanges[1].CheckColChange(6, DBNull.Value, "Left for another job");
		}


		private const string jtHTech = "Helpdesk Tech";

		private const string jtHSuper = "Helpdesk Supervisor";


		public static TemporalComparer EETest()
		{
			//Build a simple DataTable
			DataTable t = EEDataTable();

			//Add some simulated temporal data to the table
			EEData(t);

			//sort into the proper order...
			t.DefaultView.Sort = "ID, SysEndTime desc";
			t = t.DefaultView.ToTable();

			//find the changes
			TemporalComparer tc = new TemporalComparer(t);
			tc.KeyColumn = t.Columns["ID"];
			tc.UserIDColumn = t.Columns["ChangedBy"];
			return tc;
		}


		private static void EEData(DataTable tbl)
		{
			EEDataMary(tbl);
			EEDataJoe(tbl);
		}

		private static void EEDataMary(DataTable tbl)
		{
			DateTime start;
			DateTime end;

			//Mary is hired
			start = new DateTime(2015, 3, 1, 8, 1, 23);
			var Mary = new EE(1, "Mary Martin", jtHTech, (decimal)22.12, start.Date);
			end = new DateTime(2016, 2, 28, 14, 15, 47);
			AddEERow(tbl, Mary, start, end, "Jack");

			//Mary gets a raise at her 1-year anniversary
			Mary.Raise(5);
			start = end;
			end = new DateTime(2016, 10, 31, 10, 34, 56);
			AddEERow(tbl, Mary, start, end, "Jack");

			//Mary gets a promotion and raise
			Mary.Promote(jtHSuper, 10);
			start = end;
			end = new DateTime(2017, 2, 28, 13, 04, 47);
			AddEERow(tbl, Mary, start, end, "Jack");

			//Mary gets a raise at her 2-year anniversary
			Mary.Raise(5);
			start = end;
			end = new DateTime(2017, 5, 31, 17, 32, 24);
			AddEERow(tbl, Mary, start, end, "Jill");

			//Mary leaves for another job
			Mary.Terminate(end.Date, "Left for another job");
			start = end;
			end = new DateTime(9999, 12, 31, 23, 59, 59, 999);
			AddEERow(tbl, Mary, start, end, "Jack");

		}

		private static void EEDataJoe(DataTable tbl)
		{
			DateTime start;
			DateTime end;

			//Joe is hired
			start = new DateTime(2015, 4, 1, 8, 42, 16);
			var Joe = new EE(2, "Joe Jones", jtHTech, (decimal)21.64, start.Date);
			end = new DateTime(2016, 3, 31, 16, 35, 17);
			AddEERow(tbl, Joe, start, end, "Jill");

			//Joe gets a raise at his 1-year anniversary
			Joe.Raise(5);
			start = end;
			end = new DateTime(2017, 3, 31, 13, 49, 36);
			AddEERow(tbl, Joe, start, end, "Jack");

			//Joe gets a raise at his 2-year anniversary
			Joe.Raise(5);
			start = end;
			end = new DateTime(2017, 6, 3, 9, 1, 6);
			AddEERow(tbl, Joe, start, end, "Jill");

			//Joe gets a promotion and raise
			Joe.Promote(jtHSuper, 10);
			start = end;
			end = new DateTime(9999, 12, 31, 23, 59, 59, 999);
			AddEERow(tbl, Joe, start, end, "Jack");
		}

		private static DataTable EEDataTable()
		{
			//Build a simple Employee DataTable
			DataTable t = new DataTable("Employees");
			t.Columns.Add("ID", typeof(int));
			t.Columns.Add("Name", typeof(string));
			t.Columns.Add("JobTitle", typeof(string));
			t.Columns.Add("Salary", typeof(decimal));
			t.Columns.Add("HireDate", typeof(DateTime));
			t.Columns.Add("TerminationDate", typeof(DateTime));
			t.Columns.Add("TermReason", typeof(string));
			t.Columns.Add("SysStartTime", typeof(DateTime));
			t.Columns.Add("SysEndTime", typeof(DateTime));
			t.Columns.Add("ChangedBy", typeof(string));

			return t;
		}

		private static void AddEERow(DataTable tbl, EE emp, DateTime SysStartTime, DateTime SysEndTime, string changedBy) => AddEERow(tbl, emp.ID, emp.Name, emp.JobTitle, emp.Salary, emp.HireDate, emp.TerinationDate, emp.TerminationReason, SysStartTime, SysEndTime, changedBy);

		private static void AddEERow(DataTable tbl, int ID, string Name, string JobTitle, decimal Salary, DateTime Hire, DateTime? Termination, string TermReason, DateTime SysStartTime, DateTime SysEndTime, string changedBy)
		{ tbl.Rows.Add(ID, Name, JobTitle, Salary, Hire, Termination, TermReason, SysStartTime, SysEndTime, changedBy); }


		private class EE
		{
			public EE(int id, string name, string jobTitle, decimal salary, DateTime hire) => (ID, Name, JobTitle, Salary, HireDate) = (id, name, jobTitle, salary, hire);

			public int ID { get; set; }
			public string Name { get; set; }
			public string JobTitle { get; set; }
			public decimal Salary { get; set; }
			public DateTime HireDate { get; set; }
			public DateTime? TerinationDate { get; set; } = null;
			public string TerminationReason { get; set; }

			public void Raise(decimal IncreasePercentage) => Salary = CalcSalIncrease(Salary, IncreasePercentage);

			public void Promote(string NewJobTitle, decimal IncreasePercentage) => (JobTitle, Salary) = (NewJobTitle, CalcSalIncrease(Salary, IncreasePercentage));

			public void Terminate(DateTime TermDate, string TermReason) => (TerinationDate, TerminationReason) = (TermDate, TermReason);

			private decimal CalcSalIncrease(decimal Sal, decimal Inc) => Math.Round(Salary * (1 + (Inc / 100)), 2, MidpointRounding.AwayFromZero);
		}

		private static DataTable SimpleDt()
		{
			//Build a simple DataTable
			DataTable t = new DataTable("Simple");
			t.Columns.Add("ID", typeof(int));
			t.Columns.Add("Title", typeof(string));
			t.Columns.Add("Amount", typeof(decimal));
			t.Columns.Add("Date", typeof(DateTime));
			t.Columns.Add("SysStartTime", typeof(DateTime));
			t.Columns.Add("SysEndTime", typeof(DateTime));

			return t;
		}
	}
}
