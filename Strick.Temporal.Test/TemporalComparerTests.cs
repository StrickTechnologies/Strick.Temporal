using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Strick.Temporal.Test;


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
		EE_NoSalaryChanges(tc);
		EE_JobTitleChanges(tc);
		EE_TerminationChanges(tc);
		EE_NoTerminationChanges(tc);
		EE_AllChanges_Descending(tc);
	}

	[TestMethod]
	public void TestColumnCaption()
	{
		TemporalComparer tc = EETest();
		var rowChanges = tc.Changes.ToList();
		Assert.IsNotNull(rowChanges);
		Assert.AreEqual(7, rowChanges.Count());

		Assert.AreEqual("Salary", rowChanges[0].ColumnChanges[0].Caption);
		Assert.AreEqual("Salary", rowChanges[0].ColumnChanges[0].ColumnName);

		Assert.AreEqual("Job Title", rowChanges[2].ColumnChanges[0].Caption);
		Assert.AreEqual("JobTitle", rowChanges[2].ColumnChanges[0].ColumnName);
		Assert.AreEqual("Salary", rowChanges[2].ColumnChanges[1].Caption);
		Assert.AreEqual("Salary", rowChanges[2].ColumnChanges[1].ColumnName);

		Assert.AreEqual("Job Title", rowChanges[4].ColumnChanges[0].Caption);
		Assert.AreEqual("JobTitle", rowChanges[4].ColumnChanges[0].ColumnName);
		Assert.AreEqual("Salary", rowChanges[4].ColumnChanges[1].Caption);
		Assert.AreEqual("Salary", rowChanges[4].ColumnChanges[1].ColumnName);

		Assert.AreEqual("Termination Date", rowChanges[6].ColumnChanges[0].Caption);
		Assert.AreEqual("TerminationDate", rowChanges[6].ColumnChanges[0].ColumnName);
		Assert.AreEqual("Termination Reason", rowChanges[6].ColumnChanges[1].Caption);
		Assert.AreEqual("TerminationReason", rowChanges[6].ColumnChanges[1].ColumnName);
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
		//rc.CheckRowChange(7, 1, 2, new DateTime(2016, 3, 31, 16, 35, 17), new DateTime(2017, 3, 31, 13, 49, 36), "Jack");
		rc.CheckRowChange(7, 1, new object[] { 2 });
		rc.ColumnChanges[0].CheckColChange(3, 21.64m, 22.72m);

		rc = rowChanges[1];
		rc.CheckRowChange(6, 1, new object[] { 2 });
		rc.ColumnChanges[0].CheckColChange(3, 22.72m, 23.86m);

		rc = rowChanges[2];
		rc.CheckRowChange(5, 2, new object[] { 2 });
		rc.ColumnChanges[0].CheckColChange(2, Employee.jobTitleHTech, Employee.jobTitleHSuper);
		rc.ColumnChanges[1].CheckColChange(3, 23.86m, 26.25m);

		rc = rowChanges[3];
		rc.CheckRowChange(3, 1, new object[] { 1 });
		rc.ColumnChanges[0].CheckColChange(3, 22.12m, 23.23m);

		rc = rowChanges[4];
		rc.CheckRowChange(2, 2, new object[] { 1 });
		rc.ColumnChanges[0].CheckColChange(2, Employee.jobTitleHTech, Employee.jobTitleHSuper);
		rc.ColumnChanges[1].CheckColChange(3, 23.23m, 25.55m);

		rc = rowChanges[5];
		rc.CheckRowChange(1, 1, new object[] { 1 });
		rc.ColumnChanges[0].CheckColChange(3, 25.55m, 26.83m);

		rc = rowChanges[6];
		rc.CheckRowChange(0, 2, new object[] { 1 });
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
		rc.CheckRowChange(7, 1, new object[] { 2 });
		rc.ColumnChanges[0].CheckColChange(3, 21.64m, 22.72m);

		rc = rowChanges[1];
		rc.CheckRowChange(6, 1, new object[] { 2 });
		rc.ColumnChanges[0].CheckColChange(3, 22.72m, 23.86m);

		rc = rowChanges[2];
		rc.CheckRowChange(5, 1, new object[] { 2 });
		rc.ColumnChanges[0].CheckColChange(3, 23.86m, 26.25m);

		rc = rowChanges[3];
		rc.CheckRowChange(3, 1, new object[] { 1 });
		rc.ColumnChanges[0].CheckColChange(3, 22.12m, 23.23m);

		rc = rowChanges[4];
		rc.CheckRowChange(2, 1, new object[] { 1 });
		rc.ColumnChanges[0].CheckColChange(3, 23.23m, 25.55m);

		rc = rowChanges[5];
		rc.CheckRowChange(1, 1, new object[] { 1 });
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
		rc.CheckRowChange(5, 1, new object[] { 2 });
		rc.ColumnChanges[0].CheckColChange(2, Employee.jobTitleHTech, Employee.jobTitleHSuper);

		rc = rowChanges[1];
		rc.CheckRowChange(2, 1, new object[] { 1 });
		rc.ColumnChanges[0].CheckColChange(2, Employee.jobTitleHTech, Employee.jobTitleHSuper);
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
		tc.ExcludedColumns.Add(tc.Table.Columns["TerminationReason"]);
		Assert.AreEqual(2, tc.ExcludedColumns.Count);

		var rowChanges = tc.Changes.ToList();
		Assert.IsNotNull(rowChanges);
		Assert.AreEqual(6, rowChanges.Count());

		var rc = rowChanges[0];
		//rc.CheckRowChange(7, 1, 2, new DateTime(2016, 3, 31, 16, 35, 17), new DateTime(2017, 3, 31, 13, 49, 36), "Jack");
		rc.CheckRowChange(7, 1, new object[] { 2 });
		rc.ColumnChanges[0].CheckColChange(3, 21.64m, 22.72m);

		rc = rowChanges[1];
		rc.CheckRowChange(6, 1, new object[] { 2 });
		rc.ColumnChanges[0].CheckColChange(3, 22.72m, 23.86m);

		rc = rowChanges[2];
		rc.CheckRowChange(5, 2, new object[] { 2 });
		rc.ColumnChanges[0].CheckColChange(2, Employee.jobTitleHTech, Employee.jobTitleHSuper);
		rc.ColumnChanges[1].CheckColChange(3, 23.86m, 26.25m);

		rc = rowChanges[3];
		rc.CheckRowChange(3, 1, new object[] { 1 });
		rc.ColumnChanges[0].CheckColChange(3, 22.12m, 23.23m);

		rc = rowChanges[4];
		rc.CheckRowChange(2, 2, new object[] { 1 });
		rc.ColumnChanges[0].CheckColChange(2, Employee.jobTitleHTech, Employee.jobTitleHSuper);
		rc.ColumnChanges[1].CheckColChange(3, 23.23m, 25.55m);

		rc = rowChanges[5];
		rc.CheckRowChange(1, 1, new object[] { 1 });
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
		rc.CheckRowChange(5, 1, new object[] { 2 });
		rc.ColumnChanges[0].CheckColChange(2, Employee.jobTitleHTech, Employee.jobTitleHSuper);

		rc = rowChanges[1];
		rc.CheckRowChange(2, 1, new object[] { 1 });
		rc.ColumnChanges[0].CheckColChange(2, Employee.jobTitleHTech, Employee.jobTitleHSuper);

		rc = rowChanges[2];
		rc.CheckRowChange(0, 2, new object[] { 1 });
		rc.ColumnChanges[0].CheckColChange(5, DBNull.Value, new DateTime(2017, 5, 31));
		rc.ColumnChanges[1].CheckColChange(6, DBNull.Value, "Left for another job");
	}

	//ALL changes
	private void EE_AllChanges_Descending(TemporalComparer tc)
	{
		//ensure temporal comparer state is correct
		ResetComparerState(tc);

		tc.ChangesSortDirection = ListSortDirection.Descending;
		var rowChanges = tc.Changes.ToList();

		Assert.IsNotNull(rowChanges);
		Assert.AreEqual(7, rowChanges.Count());

		//Program.ShowRC(rowChanges);

		var rc = rowChanges[0];
		rc.CheckRowChange(0, 2, new object[] { 1 });
		rc.ColumnChanges[0].CheckColChange(5, DBNull.Value, new DateTime(2017, 5, 31));
		rc.ColumnChanges[1].CheckColChange(6, DBNull.Value, "Left for another job");

		rc = rowChanges[1];
		rc.CheckRowChange(1, 1, new object[] { 1 });
		rc.ColumnChanges[0].CheckColChange(3, 25.55m, 26.83m);

		rc = rowChanges[2];
		rc.CheckRowChange(2, 2, new object[] { 1 });
		rc.ColumnChanges[0].CheckColChange(2, Employee.jobTitleHTech, Employee.jobTitleHSuper);
		rc.ColumnChanges[1].CheckColChange(3, 23.23m, 25.55m);

		rc = rowChanges[3];
		rc.CheckRowChange(3, 1, new object[] { 1 });
		rc.ColumnChanges[0].CheckColChange(3, 22.12m, 23.23m);


		rc = rowChanges[4];
		rc.CheckRowChange(5, 2, new object[] { 2 });
		rc.ColumnChanges[0].CheckColChange(2, Employee.jobTitleHTech, Employee.jobTitleHSuper);
		rc.ColumnChanges[1].CheckColChange(3, 23.86m, 26.25m);

		rc = rowChanges[5];
		rc.CheckRowChange(6, 1, new object[] { 2 });
		rc.ColumnChanges[0].CheckColChange(3, 22.72m, 23.86m);

		rc = rowChanges[6];
		rc.CheckRowChange(7, 1, new object[] { 2 });
		rc.ColumnChanges[0].CheckColChange(3, 21.64m, 22.72m);
	}


	public static TemporalComparer EETest()
	{
		bool useDB = false;
		DataTable tbl;

		if (useDB)
		{
			//connect to our sample db and grab the data

			using (SqlConnection conn = new("Server=(localdb)\\Temporal; Integrated Security=True; Database=EmployeeTest"))
			using (SqlCommand cmd = new("SELECT * FROM Employees for system_time all order by id, sysendtime desc", conn))
			using (SqlDataAdapter da = new(cmd))
			{
				conn.Open();
				tbl = new DataTable();
				da.Fill(tbl);
			}
		}
		else
		{
			//build a datatable to simulate temporal data (no db or connection required)

			//Build a simple DataTable
			tbl = EmployeeTestData.getDataTable();

			//Add some simulated temporal data to the table
			EmployeeTestData.EEData(tbl);

			//sort into the proper order...
			tbl.DefaultView.Sort = "ID, SysEndTime desc";
			tbl = tbl.DefaultView.ToTable();
		}


		//find the changes
		TemporalComparer tc = new TemporalComparer(tbl);
		tc.KeyColumns.Add(tbl.Columns["ID"]);
		tc.UserIDColumn = tbl.Columns["ChangedBy"];
		return tc;
	}
}
