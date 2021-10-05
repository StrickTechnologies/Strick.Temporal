using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strick.Temporal.Test
{
	public class Employee
	{
		public Employee(int id, string name, string jobTitle, decimal salary, DateTime hire) => (ID, Name, JobTitle, Salary, HireDate) = (id, name, jobTitle, salary, hire);


		public const string jobTitleHTech = "Helpdesk Tech";

		public const string jobTitleHSuper = "Helpdesk Supervisor";


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
}
