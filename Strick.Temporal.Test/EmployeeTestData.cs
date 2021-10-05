using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strick.Temporal.Test
{
	public static class EmployeeTestData
	{
		public static DataTable getDataTable()
		{
			//Build a simple Employee DataTable
			DataTable t = new DataTable("Employees");
			t.Columns.Add("ID", typeof(int));
			t.Columns.Add("Name", typeof(string));
			t.Columns.Add("JobTitle", typeof(string));
			t.Columns.Add("Salary", typeof(decimal));
			t.Columns.Add("HireDate", typeof(DateTime));
			t.Columns.Add("TerminationDate", typeof(DateTime));
			t.Columns.Add("TerminationReason", typeof(string));
			t.Columns.Add("SysStartTime", typeof(DateTime));
			t.Columns.Add("SysEndTime", typeof(DateTime));
			t.Columns.Add("ChangedBy", typeof(string));

			return t;
		}

		public static void EEData(DataTable tbl)
		{
			EEDataMary(tbl);
			EEDataJoe(tbl);
		}

		public static void EEDataMary(DataTable tbl)
		{
			DateTime start;
			DateTime end;

			//Mary is hired
			start = new DateTime(2015, 3, 1, 8, 1, 23);
			var Mary = new Employee(1, "Mary Martin", Employee.jobTitleHTech, (decimal)22.12, start.Date);
			end = new DateTime(2016, 2, 28, 14, 15, 47);
			AddEERow(tbl, Mary, start, end, "Jack");

			//Mary gets a raise at her 1-year anniversary
			Mary.Raise(5);
			start = end;
			end = new DateTime(2016, 10, 31, 10, 34, 56);
			AddEERow(tbl, Mary, start, end, "Jack");

			//Mary gets a promotion and raise
			Mary.Promote(Employee.jobTitleHSuper, 10);
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

		public static void EEDataJoe(DataTable tbl)
		{
			DateTime start;
			DateTime end;

			//Joe is hired
			start = new DateTime(2015, 4, 1, 8, 42, 16);
			var Joe = new Employee(2, "Joe Jones", Employee.jobTitleHTech, (decimal)21.64, start.Date);
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
			Joe.Promote(Employee.jobTitleHSuper, 10);
			start = end;
			end = new DateTime(9999, 12, 31, 23, 59, 59, 999);
			AddEERow(tbl, Joe, start, end, "Jack");
		}


		private static void AddEERow(DataTable tbl, Employee emp, DateTime SysStartTime, DateTime SysEndTime, string changedBy) => AddEERow(tbl, emp.ID, emp.Name, emp.JobTitle, emp.Salary, emp.HireDate, emp.TerinationDate, emp.TerminationReason, SysStartTime, SysEndTime, changedBy);

		private static void AddEERow(DataTable tbl, int ID, string Name, string JobTitle, decimal Salary, DateTime Hire, DateTime? Termination, string TermReason, DateTime SysStartTime, DateTime SysEndTime, string changedBy)
		{ tbl.Rows.Add(ID, Name, JobTitle, Salary, Hire, Termination, TermReason, SysStartTime, SysEndTime, changedBy); }

	}
}
