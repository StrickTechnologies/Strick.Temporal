using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strick.Temporal.Test
{
	class Program
	{
		static void Main(string[] args)
		{
			//var tc = TemporalComparerTests.EETest();
			//ShowRC(tc);

			using var tbl = GetDT("Company", "id");
			TemporalComparer tc = new(tbl);
			tc.KeyColumn = tbl.Columns["id"];
			ShowRC(tc);
		}

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
			tc.KeyColumn = t1.Columns["ID"];
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

			//wl("");
			//PrintSt(dt);

			TemporalComparer tc = new TemporalComparer(dt);
			ShowRC(tc);
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

		private static DataTable GetDT(string tblName, string key)
		{
			using SqlConnection conn = GetConn();
			using SqlCommand cmd = new($"SELECT *, SysStartTime, SysEndTime FROM {tblName} for system_time all order by {key}, SysEndTime desc", conn);
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


		private static void ShowRC(TemporalComparer tc)
		{
			foreach (RowChange rc in tc.Changes)
			{
				w($"* Row Change: Index:{rc.RowIndex} At:{rc.ChangeTime}");
				if (tc.HasKeyColumn)
				{ w($" Key:{rc.Key} "); }

				if (tc.HasUserIDColumn)
				{ w($" by User ID:{rc.UserID} (row end time: {rc.PeriodEndTime})"); }

				if (tc.HasEndTimeColumn)
				{ w($" (row end time: {rc.PeriodEndTime})"); }


				wl("\n  Col changes:");

				foreach (ColChange cc in rc.ColumnChanges)
				{ wl($"    {cc.ColumnName} ({cc.ColumnIndex})  old:[{cc.OldValue}] new:[{cc.NewValue}]"); }
				wl("");
			}

		}


		private static void rk(string prompt = null)
		{
			if (!string.IsNullOrWhiteSpace(prompt))
			{ w(prompt); }

			Console.ReadKey();
		}

		public static void w(string message) => Console.Write(message);
		public static void wl(string message) => Console.WriteLine(message);

		//public static void dwl(string message) => Debug.WriteLine(message);
	}
}
