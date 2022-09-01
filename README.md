# Strick.Temporal
Utilities for working with temporal data in .Net

# Overview
These are the primary classes you will work with.

* [`TemporalComparer`](#temporalcomparer-class)
* [`RowChange`](#rowchange-class)
* [`ColChange`](#colchange-class)
* [`TemporalComparerColumnList`](#temporalcomparercolumnlist-class)

# `TemporalComparer` class
The `TemporalComparer` class will find differences in temporal data. It works with a DataTable object containing temporal data.

## About DataTables
While `TemporalComparer` was designed with system-versioned temporal tables in mind, it will work on most any DataTable that contains temporal data – it doesn’t have to come specifically from a system-versioned temporal table. Keep these things in mind:

* One DateTime column is required. This column designates the period start time for each row. For a system-versioned temporal table, this would typically be the `SysStartTime` column. If a column named `SysStartTime` exists in the DataTable, `TemporalComparer` will find it automatically. Otherwise you can specify a different column in the constructor call.

* The DataTable should be sorted descending (newest->oldest) on the `StartTimeColumn`. If your DataTable is not sorted this way, you may get no or incorrect results (e.g. the “old” and “new” comparison would be backward if your DataTable is sorted oldest->newest).

* If the rows in the DataTable are NOT all related, the rows should be sorted first by the Key column(s), and newest->oldest within that.

* To get any useful results from the comparison, the DataTable should contain at least one additional column (i.e. the data you actually want to scan for changes).

## Usage

* Pass your DataTable to the constructor.

* If the DataTable does **NOT** contain a DateTime column named `SysStartTime`, you will need to pass the period start time column to the constructor. **If a suitable column isn’t passed to the constructor or found in the DataTable, an exception will be thrown.**

* If your DataTable contains rows that are not all for the same entity (e.g. your DataTable contains rows for multiple People or multiple Companies), specify the column(s) that make up the "key" for each row using the `KeyColumns` property. If `KeyColumns` contains one or more columns, `TemporalComparer` will only compare (and return results) for rows whose key value matches. The value(s) from the specified column(s) will be put into the `Key` property of each `RowChange` object. Otherwise, the `Key` property in each `RowChange` object will be null and the comparer assumes ALL rows in the DataTable are related.

* Use the `UserIDColumn` property to indicate which column contains an ID for the user who made the updates on that row. If you set this, the value from the specified column will be put into the `UserID` property of each `RowChange` object. Otherwise, the `UserID` property in each `RowChange` object will be `null`.

* You can specify the period end time column using the `EndTimeColumn` property. If you set this, the value from the specified column will be put into the `PeriodEndTime` property of each `RowChange` object. Otherwise, the `PeriodEndTime` property in each `RowChange` object will be `null`.

* You can use the `ChangesSortDirection` property to control the order of the results returned by `TemporalComparer`. [Additional details](#sorting)

* By default, `TemporalComparer` will compare ALL columns in the DataTable (aside from those designated by the `StartTimeColumn`, `EndTimeColumn`, and `UserIDColumn` properties). You can override that behavior and compare only a subset of the columns in the DataTable by using either the `IncludedColumns` or `ExcludedColumns` properties. Add columns to the `IncludedColumns` collection, and only those columns will be compared and included in the results. Add columns to the `ExcludedColumns` Collection, and `TemporalComparer` will compare ALL columns except those (and the "special" columns noted above). **If both the `IncludedColumns` and `ExcludedColumns` collections have columns in them, `IncludedColumns` will take precedent, and `ExcludedColumns` will be ignored.**

	* **!** You can also accomplish this by by including only the columns you want to compare in the SQL query when you create your DataTable. Obviously, in that case no other data will be in the DataTable. If there are other fields you might wish to display or you cannot change the columns in your DataTable, use one of the methods outlined above.

* You can do different comparisons on the same data simply by changing either the `IncludedColumns` or `ExcludedColumns` properties and accessing the `Changes` property again. For example:

	```c#
	TemporalComparer tc = new(myTable);
	tc.IncludedColumns.Add(myTable.Columns["someColName"]);
	var someColChanges = tc.Changes; //changes only for column "someColName"
	//do stuff with someColChanges
	tc.IncludedColumns.Clear();
	tc.IncludedColumns.Add(myTable.Columns["otherColName"]);
	var otherColChanges = tc.Changes; //changes only for column "otherColName"
	//do stuff with otherColChanges
	```
* You can set the `Caption` property of any column in your DataTable. The `Caption` property is included in any `ColChange` objects for the column. If not specifically set, the `DataColumn.Caption` property returns the `DataColumn.ColumnName` value (i.e. it will have a value whether you specifcally set it or not).

## Results

Use the `Changes` property to retrieve an `IEnumerable<RowChange>` sequence containing the differences found in the DataTable. Accessing the `Changes` property invokes the `TemporalComparer` object's scan of the DataTable.

**Note that the `Changes` property runs the scan each time it is accessed. The results are *NOT* cached in the `TemporalComparer` object.**  
This could take significant time on a large DataTable. If your code needs to access `Changes` multiple times, you should store the results, then use that variable to subsequently access the results. e.g.:

```c#
List<RowChange> MyChanges = comparer.Changes.ToList();
//do stuff with MyChanges
```

### Other Details...
* Each `RowChange` object is guaranteed to have at least one `ColChange` object in its `ColumnChanges` property.
	* If two related rows in your DataTable don’t have any differences (this can happen if you’re only looking at a subset of the columns), no `RowChange` object is returned for those two rows.

<a name="sorting"></a>
* The results are ordered by each of the key fields in descending order, then by the `RowChange.ChangeTime` property in ascending (oldest->newest) order within the key. This ordering occurs naturally based on the order of the source DataTable and the comparer's traversal of the rows (no additional sorting is performed). The default ordering can be overridden using the `ChangesSortDirection` property.
	* The default value for `ChangesSortDirection` is `ListSortDirection.Ascending`. This results in the ordering described above.
	* Changing the `ChangesSortDirection` to `ListSortDirection.Descending` will produce results ordered as follows: the results will first be sorted by each of the Key fields in ascending order, then by the `RowChange.ChangeTime` property in descending (newest->oldest) order within the key.

## Example
This is a simple example showing how to use `TemporalComparer`.

```c#
TemporalComparer tc = new TemporalComparer(MyDataTable);
tc.UserIDColumn = MyDataTable.Columns["ChangedBy"];
tc.KeyColumn = MyDataTable.Columns["ID"];
ShowRC(tc);

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

public static void w(string message) => Console.Write(message);
public static void wl(string message) => Console.WriteLine(message);
```

When run against the DataTable created in the unit tests for the project, the above sample code returns results that look like this:

```
* Row Change: Index:7 At:3/31/2016 4:35:17 PM Key:2  by User ID:Jack (row end time: 3/31/2017 1:49:36 PM) (row end time: 3/31/2017 1:49:36 PM)
  Col changes:
    Salary (3)  old:[21.64] new:[22.72]

* Row Change: Index:6 At:3/31/2017 1:49:36 PM Key:2  by User ID:Jill (row end time: 6/3/2017 9:01:06 AM) (row end time: 6/3/2017 9:01:06 AM)
  Col changes:
    Salary (3)  old:[22.72] new:[23.86]

* Row Change: Index:5 At:6/3/2017 9:01:06 AM Key:2  by User ID:Jack (row end time: 12/31/9999 11:59:59 PM) (row end time: 12/31/9999 11:59:59 PM)
  Col changes:
    JobTitle (2)  old:[Helpdesk Tech] new:[Helpdesk Supervisor]
    Salary (3)  old:[23.86] new:[26.25]

* Row Change: Index:3 At:2/28/2016 2:15:47 PM Key:1  by User ID:Jack (row end time: 10/31/2016 10:34:56 AM) (row end time: 10/31/2016 10:34:56 AM)
  Col changes:
    Salary (3)  old:[22.12] new:[23.23]

* Row Change: Index:2 At:10/31/2016 10:34:56 AM Key:1  by User ID:Jack (row end time: 2/28/2017 1:04:47 PM) (row end time: 2/28/2017 1:04:47 PM)
  Col changes:
    JobTitle (2)  old:[Helpdesk Tech] new:[Helpdesk Supervisor]
    Salary (3)  old:[23.23] new:[25.55]

* Row Change: Index:1 At:2/28/2017 1:04:47 PM Key:1  by User ID:Jill (row end time: 5/31/2017 5:32:24 PM) (row end time: 5/31/2017 5:32:24 PM)
  Col changes:
    Salary (3)  old:[25.55] new:[26.83]

* Row Change: Index:0 At:5/31/2017 5:32:24 PM Key:1  by User ID:Jack (row end time: 12/31/9999 11:59:59 PM) (row end time: 12/31/9999 11:59:59 PM)
  Col changes:
    TerminationDate (5)  old:[] new:[5/31/2017 12:00:00 AM]
    TerminationReason (6)  old:[] new:[Left for another job]
```

# `RowChange` class
Represents changes between two related rows in your DataTable. The `ColumnChanges` property contains the changes to the individual columns. 

# `ColChange` class
Represents a change in a column between two related rows in your DataTable.

# `TemporalComparerColumnList` class
Represents a collection of `DataColumn` objects belonging to a specific DataTable. Implements the `ICollection<DataColumn>` interface, so the usual collection manipulation methods (e.g. Add, Remove, Clear) and properties (e.g. Count) can be used.

# Other

## Semantic versioning

We follow [semantic versioning](https://semver.org/).
