using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Strick.Temporal.Test.Program;


namespace Strick.Temporal.Test
{
	internal static class Par
	{
		public static ParPerson promptForPerson()
		{
			string pid = rl("Enter a Person Id (0 or <enter> to exit): ");

			int id = 0;
			if (!string.IsNullOrWhiteSpace(pid) && int.TryParse(pid, out id) && id > 0)
			{ return ParPersonRepository.byID(id); }

			return null;
		}

		public static void showPerson(ParPerson person)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			wl($"*** Person: {person.PersonID} {person.FirstName} {person.LastName}");
			Console.ResetColor();

			showPersonHistory(person);
			wl("");
		}

		public static void showPersonHistory(ParPerson person)
		{
			if (person.ChangeHistory != null && person.ChangeHistory.Count > 0)
			{
				foreach (RowChange rc in person.ChangeHistory)
				{
					Console.ForegroundColor = ConsoleColor.Green;
					wl($"\t* Changes made by {rc.UserID} at:{rc.ChangeTime}");
					Console.ResetColor();

					foreach (ColChange cc in rc.ColumnChanges)
					{ wl($"\t\t{cc.Caption}  old:[{cc.OldValue}] new:[{cc.NewValue}]"); }
					wl("");
				}
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Green;
				wl("\tThis person has no changes.");
				Console.ResetColor();
			}
		}
	}


	internal static class ParDB
	{
		public static SqlConnection GetDBConnection() => new SqlConnection("Server=(localdb)\\Temporal; Integrated Security=True; Database=ParDb");

		public static DataRow GetCurrentRow(string tblName, string rowFilter)
		{
			string where = !string.IsNullOrWhiteSpace(rowFilter) ? $"where {rowFilter}" : "";


			using SqlConnection conn = GetDBConnection();
			using SqlCommand cmd = new($"SELECT * FROM {tblName} {where}", conn);
			using SqlDataAdapter da = new(cmd);

			//using DataTable tbl = new();
			//da.Fill(tbl);
			using DataTable tbl =  GetDT($"SELECT * FROM {tblName} {where}");

			if (tbl != null && tbl.Rows.Count > 0)
			{ return tbl.Rows[0]; }

			return null;
		}


		public static DataTable GetTemporalHistory(string tblName, IEnumerable<string> keyColumns, string rowFilter)
		{
			string where = !string.IsNullOrWhiteSpace(rowFilter) ? $"where {rowFilter}" : "";
			string order;
			if (keyColumns != null && keyColumns.Count() > 0)
			{ order = $"order by {string.Join(",", keyColumns)},SysEndTime desc"; }
			else
			{ order = $"order by SysEndTime desc"; }
			
			return GetDT($"SELECT *, SysStartTime, SysEndTime FROM {tblName} for system_time all {where} {order}");
		}

		public static DataTable GetDT(string sql)
		{
			using SqlConnection conn = GetDBConnection();
			using SqlCommand cmd = new(sql, conn);
			using SqlDataAdapter da = new(cmd);

			DataTable tbl = new();
			da.Fill(tbl);

			return tbl;
		}
	}


	internal class ParPerson
	{
		public ParPerson(int personID) => (PersonID) = (personID);


		public int PersonID { get; set; }

		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Initials { get; set; }


		private List<RowChange> ch;
		public IReadOnlyList<RowChange> ChangeHistory
		{
			get
			{
				if (ch == null)
				{
					ch = ParPersonRepository.getChangeHistory(this).ToList();
				}

				return ch;
			}
		}
	}


	internal static class ParPersonRepository
	{
		public static ParPerson byID(int PersonID) => read(ParDB.GetCurrentRow("Person", $"id={PersonID}"));

		private static ParPerson read(DataRow row)
		{
			if (row == null)
			{ return null; }

			return new ParPerson((int)row["Id"])
			{
				FirstName = row["FirstName"].ToString(),
				LastName = row["LastName"].ToString(),
				Initials = row["Initials"].ToString(),
			};
		}


		public static IEnumerable<RowChange> getChangeHistory(ParPerson person)
		{
			if (person == null)
			{ throw new ArgumentNullException(); }

			string where = $"where Id={person.PersonID}";
			string flds = "Id, FirstName, LastName, Initials, UserId, JobTitleId, DepartmentId, TesterNumber, TerritoryId, IdStatusId, EndUser, Referral, AlsoManages, MailToAddressId, Notes, IsActive, DeletedDateTime, DeletedUserId, CreatedDateTime, CreatedUserId, ModifiedDateTime, ModifiedUserId, SysStartTime, SysEndTime, PrivateNotes, CompanyId, ContactId";
			string sql = $"select {flds} from Person {where} union (select {flds} from Person_History {where}) order by SysEndTime desc";
			using DataTable dt = ParDB.GetDT(sql);

			if (dt == null || dt.Rows.Count < 2)
			{
				//nothing to compare, just return an empty set
				return new List<RowChange>();
			}

			//just for fun...
			dt.Columns["FirstName"].Caption = "First Name";
			dt.Columns["LastName"].Caption = "Last Name";
			dt.Columns["MailToAddressId"].Caption = "Mail To Address ID";
			dt.Columns["PrivateNotes"].Caption = "Private Notes";

			TemporalComparer tc = new(dt);
			tc.UserIDColumn = dt.Columns["ModifiedUserId"];
			return tc.Changes;
		}
	}
}
