using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace Strick.Temporal.Test;


class Program
{
	static void Main(string[] args)
	{
		ParPerson p = Par.promptForPerson();
		while (p != null)
		{
			Par.showPerson(p);
			p = Par.promptForPerson();
		}
		return;

		//var tc = TemporalComparerTests.EETest();
		//ShowRC(tc);

		ShowTableDiffs("Company", "ModifiedUserId", new[] { "id" });
		wl();
		ShowTableDiffs("PersonEmailAssn", null, new[] { "personid", "emailid" });
		ShowTableDiffs("PersonEmailAssn", null, new[] { "personid" }, "personid not in(25273,25207)"); //skip the IDs that have multiple "current" records

		wl();
		ShowTableDiffs("Person", "ModifiedUserId", new[] { "id" }, "id in(264)");
	}

	//private static ParPerson getPerson()
	//{
	//	string pid = rl("Enter a Person Id (0 or <enter> to exit): ");

	//	int id = 0;
	//	if (!string.IsNullOrWhiteSpace(pid) && int.TryParse(pid, out id) && id > 0)
	//	{ return ParPersonRepository.byID(id); }

	//	return null;
	//}

	//private static void showPerson(ParPerson person)
	//{
	//	Console.ForegroundColor = ConsoleColor.Red;
	//	wl($"*** Person: {person.PersonID} {person.FirstName} {person.LastName}");
	//	Console.ResetColor();

	//	showPersonHistory(person);
	//	wl();
	//}

	//public static void showPersonHistory(ParPerson person)
	//{
	//	if (person.ChangeHistory != null && person.ChangeHistory.Count > 0)
	//	{
	//		foreach (RowChange rc in person.ChangeHistory)
	//		{
	//			Console.ForegroundColor = ConsoleColor.Green;
	//			wl($"\t* Changes made by {rc.UserID} at:{rc.ChangeTime}");
	//			Console.ResetColor();

	//			foreach (ColChange cc in rc.ColumnChanges)
	//			{ wl($"\t\t{cc.Caption}  old:[{cc.OldValue}] new:[{cc.NewValue}]"); }
	//			wl();
	//		}
	//	}
	//	else
	//	{
	//		Console.ForegroundColor = ConsoleColor.Green;
	//		wl("\tThis person has no changes.");
	//		Console.ResetColor();
	//	}
	//}


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

	private static void Test2()
	{
		//string fname = @"..\..\..\..\Test Data\Copy of Temporal Test.dt";
		//DataSet ds = new DataSet();
		//ds.ReadXml(fname);
		//DataTable dt = ds.Tables[0];

		var dt = CreateDT();
		//PrintSt(dt);

		//sort rows descending (newest->oldest)
		dt.DefaultView.Sort = "SysStartTime desc";
		dt = dt.DefaultView.ToTable();

		//wl();
		//PrintSt(dt);

		TemporalComparer tc = new TemporalComparer(dt);
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


	private static void ShowTableDiffs(string tblName, string UserIDColName, IEnumerable<string> keyColumns) => ShowTableDiffs(tblName, UserIDColName, keyColumns, null);

	private static void ShowTableDiffs(string tblName, string UserIDColName, IEnumerable<string> keyColumns, string rowFilter)
	{
		using var tbl = GetDT(tblName, keyColumns, rowFilter);

		TemporalComparer tc = new(tbl);

		if (!string.IsNullOrWhiteSpace(UserIDColName))
		{ tc.UserIDColumn = tbl.Columns[UserIDColName]; }

		if (keyColumns != null && keyColumns.Count() > 0)
		{ tc.KeyColumns.AddRange(keyColumns); }

		wl($"*** CHANGES IN {tblName} TABLE ***");
		if (!string.IsNullOrWhiteSpace(rowFilter))
		{ wl($"\tfilter is \"{rowFilter}\""); }

		ShowRC(tc);
	}

	private static DataTable GetDT(string tblName, IEnumerable<string> keyColumns, string rowFilter)
	{
		string where = !string.IsNullOrWhiteSpace(rowFilter) ? $"where {rowFilter}" : "";
		string order;
		if (keyColumns != null && keyColumns.Count() > 0)
		{ order = $"order by {string.Join(",", keyColumns)},SysEndTime desc"; }
		else
		{ order = $"order by SysEndTime desc"; }

		using SqlConnection conn = GetConn();
		using SqlCommand cmd = new($"SELECT *, SysStartTime, SysEndTime FROM {tblName} for system_time all {where} {order}", conn);
		using SqlDataAdapter da = new(cmd);

		DataTable tbl = new();
		da.Fill(tbl);

		return tbl;
	}

	private static SqlConnection GetConn()
	{
		return new SqlConnection("Server=(localdb)\\Temporal; Integrated Security=True; Database=ParDb");
	}


	private static void PrintSt(DataTable dt)
	{
		foreach (DataRow r in dt.Rows)
		{ wl($"{r["SysStartTime"]}"); }
	}

	private static DataTable CreateDT()
	{
		string fname = @"..\..\..\..\..\Test Data\Copy of Temporal Test.csv";

		DataTable t1 = new DataTable();
		t1.Columns.Add("CompanyName", typeof(string));
		t1.Columns.Add("CompanyDeletedOn", typeof(DateTime));
		t1.Columns.Add("PersonId", typeof(int));
		t1.Columns.Add("FirstName", typeof(string));
		t1.Columns.Add("LastName", typeof(string));
		t1.Columns.Add("JobTitle", typeof(string));
		t1.Columns.Add("PersonDeletedOn", typeof(string));
		t1.Columns.Add("Line1", typeof(string));
		t1.Columns.Add("Line2", typeof(string));
		t1.Columns.Add("Floor", typeof(string));
		t1.Columns.Add("Suite", typeof(string));
		t1.Columns.Add("City", typeof(string));
		t1.Columns.Add("State", typeof(string));
		t1.Columns.Add("ZipCode", typeof(string));
		t1.Columns.Add("AddressDeletedOn", typeof(DateTime));
		t1.Columns.Add("EmailAddress", typeof(string));
		t1.Columns.Add("DirectLine", typeof(string));
		t1.Columns.Add("Extension", typeof(string));
		t1.Columns.Add("Fax", typeof(string));
		t1.Columns.Add("Mobile", typeof(string));
		t1.Columns.Add("SysStartTime", typeof(DateTime));

		t1.TableName = "Copy of Temporal Test";

		var lines = File.ReadAllLines(fname).ToList();
		lines.RemoveAt(0); //remove header row
		foreach (string ln in lines)
		{
			var r = t1.Rows.Add();

			string[] cols = ln.Split(',');
			for (int i = 0; i < cols.Count(); i++)
			{
				if (!cols[i].Equals("NULL"))
				{ r[i] = cols[i]; }
			}
		}
		//t1.WriteXml(Path.Combine(Path.GetDirectoryName(fname), "Copy of Temporal Test.dt"));
		return t1;
	}



	public static void ShowRC(TemporalComparer tc) => ShowRC(tc.Changes);

	public static void ShowRC(IEnumerable<RowChange> rowChanges)
	{
		foreach (RowChange rc in rowChanges)
		{
			w($"* Row Change: Index:{rc.RowIndex} At:{rc.ChangeTime}");

			if (rc.Key != null)
			{ w($" Key:{string.Join(".", rc.Key)} "); }

			if (rc.UserID != null)
			{ w($" by User ID:{rc.UserID}"); }

			w($" (row end time: {rc.PeriodEndTime})");


			wl("\n  Col changes:");

			foreach (ColChange cc in rc.ColumnChanges)
			{ wl($"    {cc.ColumnName} ({cc.ColumnIndex})  old:[{cc.OldValue}] new:[{cc.NewValue}]"); }
			wl();
		}

	}


	public static void rk(string prompt = null)
	{
		if (!string.IsNullOrWhiteSpace(prompt))
		{ w(prompt); }

		Console.ReadKey();
	}

	public static string rl(string prompt = null)
	{
		if (!string.IsNullOrWhiteSpace(prompt))
		{ w(prompt); }

		return Console.ReadLine();
	}

	public static void w(string message) => Console.Write(message);
	public static void wl() => Console.WriteLine();
	public static void wl(string message) => Console.WriteLine(message);

	public static void w(string message, ConsoleColor color, bool stripDelimiters = true)
	{
		//use { and } for delimiters since some data we're testing with has [ and ] in it
		//var segments = Regex.Split(message, @"(\[[^\]]*\])");
		var segments = Regex.Split(message, @"({[^}]*})");
		for (int x = 0; x < segments.Length; x++)
		{
			string segment = segments[x];

			//if (segment.StartsWith("[") && segment.EndsWith("]"))
			if (segment.StartsWith("{") && segment.EndsWith("}"))
			{
				Console.ForegroundColor = color;

				if (stripDelimiters)
				{ segment = segment.Substring(1, segment.Length - 2); }
			}

			w(segment);
			Console.ResetColor();
		}
	}
	public static void wl(string message, ConsoleColor color, bool stripDelimiters = true)
	{
		w(message, color, stripDelimiters);
		wl();
	}
}
