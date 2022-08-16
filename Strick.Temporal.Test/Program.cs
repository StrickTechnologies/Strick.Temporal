using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Drawing;

using static Strick.PlusCon.Helpers;


namespace Strick.Temporal.Test;


class Program
{
	static void Main(string[] args)
	{
		//var tc = TemporalComparerTests.EETest();
		//ShowRC(tc);

		using SqlConnection conn = GetDBConnection();

		ShowTableDiffs(conn, "Company", "ModifiedUserId", new[] { "id" });
		WL();
		ShowTableDiffs(conn, "PersonEmailAssn", null, new[] { "personid" });
		ShowTableDiffs(conn, "PersonEmailAssn", null, new[] { "personid" }, "personid not in(25273,25207)"); //skip the IDs that have multiple "current" records

		WL();
		ShowTableDiffs(conn, "Person", "ModifiedUserId", new[] { "id" }, "id in(264)");
	}

	public static SqlConnection GetDBConnection() => new SqlConnection("Server=(localdb)\\Temporal; Integrated Security=True; Database=ParDb");

	private static void Test1()
	{
		DataTable t1 = new DataTable();
		t1.Columns.Add("ID", typeof(int));
		t1.Columns.Add("Title", typeof(string));
		t1.Columns.Add("Amount", typeof(decimal));
		t1.Columns.Add("Date", typeof(DateTime));
		t1.Columns.Add("SysStartTime", typeof(DateTime));
		t1.Columns.Add("SysEndTime", typeof(DateTime));

		t1.Rows.Add(1, "Test 1", 123.45, new DateTime(2020, 1, 1, 10, 0, 0), new DateTime(2020, 1, 1, 10, 0, 0), new DateTime(9999, 12, 31, 23, 59, 59));
		t1.Rows.Add(1, "Test 1", 456.78, new DateTime(2019, 12, 1, 8, 10, 20), new DateTime(2020, 1, 1, 9, 0, 0), new DateTime(2020, 1, 1, 9, 59, 59));

		t1.Rows.Add(2, "Test 2", 123.45, new DateTime(2020, 1, 1, 10, 0, 0), new DateTime(2020, 1, 1, 10, 0, 0), new DateTime(9999, 12, 31, 23, 59, 59));
		t1.Rows.Add(2, "Test 2", 456.78, new DateTime(2019, 12, 1, 8, 10, 20), new DateTime(2020, 1, 1, 9, 0, 0), new DateTime(2020, 1, 1, 9, 59, 59));


		TemporalComparer tc = new TemporalComparer(t1);
		tc.KeyColumns.Add(t1.Columns["ID"]);
		ShowRC(tc);
	}

	private static void TestMKey()
	{
		var t = TemporalComparerKeyTests.getDT();
		TemporalComparer tc = new(t);
		tc.KeyColumns.Add(t.Columns["ID"]);
		tc.KeyColumns.Add(t.Columns["PositionID"]);
		tc.UserIDColumn = t.Columns["ChangedBy"];
		ShowRC(tc);
	}


	private static void ShowTableDiffs(SqlConnection conn, string tblName, string UserIDColName, IEnumerable<string> keyColumns) => ShowTableDiffs(conn, tblName, UserIDColName, keyColumns, null);

	private static void ShowTableDiffs(SqlConnection conn, string tblName, string UserIDColName, IEnumerable<string> keyColumns, string rowFilter)
	{
		using var tbl = GetDT(conn, tblName, keyColumns, rowFilter);

		TemporalComparer tc = new(tbl);

		if (!string.IsNullOrWhiteSpace(UserIDColName))
		{ tc.UserIDColumn = tbl.Columns[UserIDColName]; }

		if (keyColumns != null && keyColumns.Any())
		{ tc.KeyColumns.AddRange(keyColumns); }

		WL($"*** CHANGES IN [{tblName}] TABLE ***", Color.Red);
		if (!string.IsNullOrWhiteSpace(rowFilter))
		{ WL($"\tfilter is [{rowFilter}]", Color.LimeGreen); }

		ShowRC(tc);
	}

	public static void ShowRC(TemporalComparer tc) => ShowRC(tc.Changes);

	public static void ShowRC(IEnumerable<RowChange> rowChanges)
	{
		foreach (RowChange rc in rowChanges)
		{
			W($"* Row Change: Index:{rc.RowIndex} At:{rc.ChangeTime}");

			if (rc.Key != null)
			{ W($" Key:{string.Join(".", rc.Key)} "); }

			if (rc.UserID != null)
			{ W($" by User ID:{rc.UserID}"); }

			W($" (row end time: {rc.PeriodEndTime})");


			WL("\n  Col changes:");

			ShowCC(rc);
			WL();
		}

	}

	public static void ShowCC(RowChange rowChange)
	{
		foreach (ColChange cc in rowChange.ColumnChanges)
		{ WL($"    {cc.ColumnName} ({cc.ColumnIndex})  old:[{cc.OldValue}] new:[{cc.NewValue}]", Color.CornflowerBlue, null, true); }
	}

	private static DataTable GetDT(SqlConnection conn, string tblName, IEnumerable<string> keyColumns, string rowFilter)
	{
		string where = !string.IsNullOrWhiteSpace(rowFilter) ? $"where {rowFilter}" : "";
		string order;
		if (keyColumns != null && keyColumns.Any())
		{ order = $"order by {string.Join(",", keyColumns)},SysEndTime desc"; }
		else
		{ order = $"order by SysEndTime desc"; }

		conn.Open();

		using SqlCommand cmd = new($"SELECT *, SysStartTime, SysEndTime FROM {tblName} for system_time all {where} {order}", conn);
		using SqlDataAdapter da = new(cmd);

		DataTable tbl = new();
		da.Fill(tbl);

		conn.Close();

		return tbl;
	}


}
