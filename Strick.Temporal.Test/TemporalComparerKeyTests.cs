using System;
using System.ComponentModel;
using System.Data;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Strick.Temporal.Test;


[TestClass]
public class TemporalComparerKeyTests
{
	[TestMethod]
	public void Wassup()
	{
		var t = getDT();
		TemporalComparer tc = new(t);
		tc.KeyColumns.Add(t.Columns["ID"]);
		tc.KeyColumns.Add(t.Columns["PositionID"]);
		tc.UserIDColumn = t.Columns["ChangedBy"];

		var rowChanges = tc.Changes.ToList();

		Program.ShowRC(rowChanges);

		Assert.IsNotNull(rowChanges);
		Assert.AreEqual(7, rowChanges.Count());

		var rc = rowChanges[0];
		rc.CheckRowChange(9, 1, new object[] { 2, Employee.posIDHTech });
		rc.ColumnChanges[0].CheckColChange(2, 21.64m, 22.72m);

		rc = rowChanges[1];
		rc.CheckRowChange(8, 1, new object[] { 2, Employee.posIDHTech });
		rc.ColumnChanges[0].CheckColChange(2, 22.72m, 23.86m);

		rc = rowChanges[2];
		rc.CheckRowChange(7, 2, new object[] { 2, Employee.posIDHTech });
		rc.ColumnChanges[0].CheckColChange(4, DBNull.Value, new DateTime(2017, 6, 3));
		rc.ColumnChanges[1].CheckColChange(5, DBNull.Value, "Promotion");

		rc = rowChanges[3];
		rc.CheckRowChange(4, 1, new object[] { 1, Employee.posIDHTech });
		rc.ColumnChanges[0].CheckColChange(2, 22.12m, 23.23m);

		rc = rowChanges[4];
		rc.CheckRowChange(3, 2, new object[] { 1, Employee.posIDHTech });
		rc.ColumnChanges[0].CheckColChange(4, DBNull.Value, new DateTime(2016, 10, 31));
		rc.ColumnChanges[1].CheckColChange(5, DBNull.Value, "Promotion");

		rc = rowChanges[5];
		rc.CheckRowChange(1, 1, new object[] { 1, Employee.posIDHSuper });
		rc.ColumnChanges[0].CheckColChange(2, 25.55m, 26.83m);

		rc = rowChanges[6];
		rc.CheckRowChange(0, 2, new object[] { 1, Employee.posIDHSuper });
		rc.ColumnChanges[0].CheckColChange(4, DBNull.Value, new DateTime(2017, 5, 31));
		rc.ColumnChanges[1].CheckColChange(5, DBNull.Value, "Left for another job");
	}

	[TestMethod]
	public void WassupRev()
	{
		var t = getDT();
		TemporalComparer tc = new(t);
		tc.KeyColumns.Add(t.Columns["ID"]);
		tc.KeyColumns.Add(t.Columns["PositionID"]);
		tc.UserIDColumn = t.Columns["ChangedBy"];
		tc.ChangesSortDirection = ListSortDirection.Descending;

		var rowChanges = tc.Changes.ToList();

		Program.ShowRC(rowChanges);

		Assert.IsNotNull(rowChanges);
		Assert.AreEqual(7, rowChanges.Count());

		var rc = rowChanges[0];
		rc.CheckRowChange(0, 2, new object[] { 1, Employee.posIDHSuper });
		rc.ColumnChanges[0].CheckColChange(4, DBNull.Value, new DateTime(2017, 5, 31));
		rc.ColumnChanges[1].CheckColChange(5, DBNull.Value, "Left for another job");

		rc = rowChanges[1];
		rc.CheckRowChange(1, 1, new object[] { 1, Employee.posIDHSuper });
		rc.ColumnChanges[0].CheckColChange(2, 25.55m, 26.83m);

		rc = rowChanges[2];
		rc.CheckRowChange(3, 2, new object[] { 1, Employee.posIDHTech });
		rc.ColumnChanges[0].CheckColChange(4, DBNull.Value, new DateTime(2016, 10, 31));
		rc.ColumnChanges[1].CheckColChange(5, DBNull.Value, "Promotion");

		rc = rowChanges[3];
		rc.CheckRowChange(4, 1, new object[] { 1, Employee.posIDHTech });
		rc.ColumnChanges[0].CheckColChange(2, 22.12m, 23.23m);

		//***

		rc = rowChanges[4];
		rc.CheckRowChange(7, 2, new object[] { 2, Employee.posIDHTech });
		rc.ColumnChanges[0].CheckColChange(4, DBNull.Value, new DateTime(2017, 6, 3));
		rc.ColumnChanges[1].CheckColChange(5, DBNull.Value, "Promotion");

		rc = rowChanges[5];
		rc.CheckRowChange(8, 1, new object[] { 2, Employee.posIDHTech });
		rc.ColumnChanges[0].CheckColChange(2, 22.72m, 23.86m);

		rc = rowChanges[6];
		rc.CheckRowChange(9, 1, new object[] { 2, Employee.posIDHTech });
		rc.ColumnChanges[0].CheckColChange(2, 21.64m, 22.72m);
	}

	public static DataTable getDT()
	{
		//Build a simple Employee DataTable
		DataTable t = new DataTable("EmployeePositions");
		t.Columns.Add("ID", typeof(int));
		t.Columns.Add("PositionID", typeof(string));
		t.Columns.Add("Salary", typeof(decimal));
		t.Columns.Add("HireDate", typeof(DateTime));
		t.Columns.Add("TerminationDate", typeof(DateTime));
		t.Columns.Add("TerminationReason", typeof(string));
		t.Columns.Add("SysStartTime", typeof(DateTime));
		t.Columns.Add("SysEndTime", typeof(DateTime));
		t.Columns.Add("ChangedBy", typeof(string));

		EEDataMary(t);
		EEDataJoe(t);

		//sort into the proper order...
		t.DefaultView.Sort = "ID, PositionID, SysEndTime desc";
		t = t.DefaultView.ToTable();

		return t;
	}

	public static readonly DateTime EndOfTime = new(9999, 12, 31, 23, 59, 59, 999);

	public static void EEDataMary(DataTable tbl)
	{
		DateTime start;
		DateTime end;

		//Mary is hired
		start = new DateTime(2015, 3, 1, 8, 1, 23);
		end = new DateTime(2016, 2, 28, 14, 15, 47);
		var Mary = new Employee(1, "Mary Martin", Employee.jobTitleHTech, (decimal)22.12, start.Date);
		AddEERow(tbl, Mary.ID, Employee.posIDHTech, Mary.Salary, Mary.HireDate, null, null, start, end, "Jack");

		//Mary gets a raise at her 1-year anniversary
		Mary.Raise(5);
		start = end;
		end = new DateTime(2016, 10, 31, 10, 34, 56);
		AddEERow(tbl, Mary.ID, Employee.posIDHTech, Mary.Salary, Mary.HireDate, null, null, start, end, "Jill");

		//Mary gets a promotion and raise
		start = end;
		end = new DateTime(2017, 2, 28, 13, 04, 47);
		AddEERow(tbl, Mary.ID, Employee.posIDHTech, Mary.Salary, Mary.HireDate, start.Date, "Promotion", start, EndOfTime, "Jack");
		Mary.Promote(Employee.jobTitleHSuper, 10);
		Mary.HireDate = start.Date;
		AddEERow(tbl, Mary.ID, Employee.posIDHSuper, Mary.Salary, Mary.HireDate, null, null, start, end, "Jack");

		//Mary gets a raise at her 2-year anniversary
		Mary.Raise(5);
		start = end;
		end = new DateTime(2017, 5, 31, 17, 32, 24);
		AddEERow(tbl, Mary.ID, Employee.posIDHSuper, Mary.Salary, Mary.HireDate, null, null, start, end, "Jill");

		//Mary leaves for another job
		Mary.Terminate(end.Date, "Left for another job");
		start = end;
		end = EndOfTime;
		AddEERow(tbl, Mary.ID, Employee.posIDHSuper, Mary.Salary, Mary.HireDate, Mary.TerinationDate, Mary.TerminationReason, start, end, "Jack");
	}

	public static void EEDataJoe(DataTable tbl)
	{
		DateTime start;
		DateTime end;

		//Joe is hired
		start = new DateTime(2015, 4, 1, 8, 42, 16);
		var Joe = new Employee(2, "Joe Jones", Employee.jobTitleHTech, (decimal)21.64, start.Date);
		end = new DateTime(2016, 3, 31, 16, 35, 17);
		AddEERow(tbl, Joe.ID, Employee.posIDHTech, Joe.Salary, Joe.HireDate.Date, null, null, start, end, "Jill");

		//Joe gets a raise at his 1-year anniversary
		Joe.Raise(5);
		start = end;
		end = new DateTime(2017, 3, 31, 13, 49, 36);
		AddEERow(tbl, Joe.ID, Employee.posIDHTech, Joe.Salary, Joe.HireDate.Date, null, null, start, end, "Jack");

		//Joe gets a raise at his 2-year anniversary
		Joe.Raise(5);
		start = end;
		end = new DateTime(2017, 6, 3, 9, 1, 6);
		AddEERow(tbl, Joe.ID, Employee.posIDHTech, Joe.Salary, Joe.HireDate.Date, null, null, start, end, "Jack");

		//Joe gets a promotion and raise
		start = end;
		end = new DateTime(9999, 12, 31, 23, 59, 59, 999);
		AddEERow(tbl, Joe.ID, Employee.posIDHTech, Joe.Salary, Joe.HireDate.Date, start.Date, "Promotion", start, end, "Jill");

		Joe.Promote(Employee.jobTitleHSuper, 10);
		Joe.HireDate = start;
		AddEERow(tbl, Joe.ID, Employee.posIDHSuper, Joe.Salary, Joe.HireDate.Date, null, null, start, end, "Jill");
	}

	private static void AddEERow(DataTable tbl, int EEID, string PositionID, decimal Salary, DateTime Hire, DateTime? Termination, string TermReason, DateTime SysStartTime, DateTime SysEndTime, string changedBy)
	{ tbl.Rows.Add(EEID, PositionID, Salary, Hire, Termination, TermReason, SysStartTime, SysEndTime, changedBy); }
}
