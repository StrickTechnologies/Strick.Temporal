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
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
			TemporalComparer tc = EETest();

			Assert.IsFalse(tc == null);

			List<RowChg> Chgs = tc.Changes.ToList();

			Assert.IsFalse(Chgs == null);
			Assert.AreEqual(7, tc.Changes.Count());


			//look for salary changes
			tc.IncludedColumns.Add(tc.Table.Columns["Salary"]);
			Assert.IsFalse(tc == null);

			Chgs = tc.Changes.ToList();

			Assert.IsFalse(Chgs == null);
			Assert.AreEqual(6, tc.Changes.Count());

			//look for job title changes
			tc.IncludedColumns.Clear();
			tc.IncludedColumns.Add(tc.Table.Columns["JobTitle"]);
			Chgs = tc.Changes.ToList();

			Assert.IsFalse(Chgs == null);
			Assert.AreEqual(2, tc.Changes.Count());


			//only terminations
			tc.IncludedColumns.Clear();
			tc.IncludedColumns.Add(tc.Table.Columns["TerminationDate"]);

			Chgs = tc.Changes.ToList();

			Assert.IsFalse(Chgs == null);
			Assert.AreEqual(1, tc.Changes.Count());

			//exclude terminations
			tc.IncludedColumns.Clear();
			tc.ExcludedColumns.Add(tc.Table.Columns["TerminationDate"]);
			tc.ExcludedColumns.Add(tc.Table.Columns["TermReason"]);

			Chgs = tc.Changes.ToList();

			Assert.IsFalse(Chgs == null);
			Assert.AreEqual(6, tc.Changes.Count());
		}

		public static TemporalComparer EETest()
		{
			//Build a simple DataTable
			DataTable t = EEDt();

			//Add some simulated temporal data to the table
			EEData(t);

			//sort into the proper order...
			t.DefaultView.Sort = "ID, SysEndTime desc";
			t = t.DefaultView.ToTable();

			//find the changes
			TemporalComparer tc = new TemporalComparer(t);
			tc.KeyColumn = t.Columns["ID"];
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
			var Mary = new EE(1, "Mary Martin", "Helpdesk Tech", (decimal)22.12, start.Date);
			end = new DateTime(2016, 2, 28, 14, 15, 47);
			AddEERow(tbl, Mary, start, end);

			//Mary gets a raise at her 1-year anniversary
			Mary.Raise(5);
			start = end;
			end = new DateTime(2016, 10, 31, 10, 34, 56);
			AddEERow(tbl, Mary, start, end);

			//Mary gets a promotion and raise
			Mary.Promote("Helpdesk Supervisor", 10);
			start = end;
			end = new DateTime(2017, 2, 28, 13, 04, 47);
			AddEERow(tbl, Mary, start, end);

			//Mary gets a raise at her 2-year anniversary
			Mary.Raise(5);
			start = end;
			end = new DateTime(2017, 5, 31, 17, 32, 24);
			AddEERow(tbl, Mary, start, end);

			//Mary leaves for another job
			Mary.Terminate(end.Date, "Left for another job");
			start = end;
			end = new DateTime(9999, 12, 31, 23, 59, 59, 999);
			AddEERow(tbl, Mary, start, end);

		}
	
		private static void EEDataJoe(DataTable tbl)
		{
			DateTime start;
			DateTime end;

			//Joe is hired
			start = new DateTime(2015, 4, 1, 8, 42, 16);
			var Joe = new EE(2, "Joe Jones", "Helpdesk Tech", (decimal)21.64, start.Date);
			end = new DateTime(2016, 3, 31, 16, 35, 17);
			AddEERow(tbl, Joe, start, end);

			//Joe gets a raise at his 1-year anniversary
			Joe.Raise(5);
			start = end;
			end = new DateTime(2017, 3, 31, 13, 49, 36);
			AddEERow(tbl, Joe, start, end);

			//Joe gets a raise at his 2-year anniversary
			Joe.Raise(5);
			start = end;
			end = new DateTime(2017, 6, 3, 9, 1, 6);
			AddEERow(tbl, Joe, start, end);

			//Joe gets a promotion and raise
			Joe.Promote("Helpdesk Supervisor", 10);
			start = end;
			end = new DateTime(9999, 12, 31, 23, 59, 59, 999);
			AddEERow(tbl, Joe, start, end);
		}

		private static DataTable EEDt()
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

			return t;
		}

		private static void AddEERow(DataTable tbl, EE emp, DateTime SysStartTime, DateTime SysEndTime) => AddEERow(tbl, emp.ID, emp.Name, emp.JobTitle, emp.Salary, emp.HireDate, emp.TerinationDate, emp.TerminationReason, SysStartTime, SysEndTime);

		private static void AddEERow(DataTable tbl, int ID, string Name, string JobTitle, decimal Salary, DateTime Hire, DateTime? Termination, string TermReason, DateTime SysStartTime, DateTime SysEndTime)
		{ tbl.Rows.Add(ID, Name, JobTitle, Salary, Hire, Termination, TermReason, SysStartTime, SysEndTime); }


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
