# Strick.Temporal
Utilities for working with temporal data in .Net

# TemporalComparer
## DataTable requirements
* This will work on any DataTable that contains temporal data – it doesn’t have to come specifically from a system versioned (temporal) table.

* One DateTime column is required.  For a system versioned table, this would normally be the SysStartTime column.  It will look for a column named SysStartTime by default, but you can specify a different column in case your DataTable doesn’t have the standard column.

* For the results to be useful, the DataTable should contain at least one additional column (i.e. the data you actually want to scan for changes).

* It assumes the rows in the DataTable are sorted newest->oldest.  If your DataTable is not sorted this way, you may get no or incorrect results (e.g. the “old” and “new” comparison would be backward if your DataTable is sorted oldest->newest).

* If the rows in the DataTable are NOT all related, it assumes the rows are sorted first by the Key column, and newest->oldest within that.  More on Key in the usage section below…

## Usage
* The class to use is Strick.Temporal.TemporalComparer.  You’ll have to pass your DataTable to the constructor.

* Optionally, you can also pass the period start time column to the constructor.  If you don’t pass the column in, it will look for a DateTime type column named SysStartTime.  If the column isn’t found, an exception will be thrown.

* Optional.  You can use the UserIDColumn to indicate which column contains an ID for the user who made the updates on that row.  If you set this, the value from the specified column will be put into the UserID property of each RowChg object.  If you don’t set this, the UserID property in each RowChg object will be null.

* Optional.  You can specify the period end time column using the EndTimeColumn property. .  If you set this, the value from the specified column will be put into the PeriodEndTime property of each RowChg object.  If you don’t set this, the PeriodEndTime property in each RowChg object will be null.

* Optional.  You can specify the “key” column using the KeyColumn property.  Use this if your DataTable contains rows that are not all for the same entity (e.g. your DataTable contains rows for multiple People or multiple Companies).  If you set this, the comparer will only compare (and return results) for rows whose KeyColumn value matches.  And, the value from the specified column will be put into the Key property of each RowChg object.  If you don’t set this, the Key property in each RowChg object will be null and the comparer assumes ALL rows in the DataTable are related.

	* Note: Only single-column keys are supported right now.  I figure it will be a future enhancement to support compound keys if it comes up.

* Optional.  If for some reason you want to include any of the above special columns in the comparisons, you can use the IncludeStartTimeColumn, IncludeEndTimeColumn, IncludeUserIDColumn, IncludeKeyColumn properties.  These all default to false (because they should most always be different between rows, and are already included in the RowChg objects when appropriate).  This is probably primarily a debugging and testing tool, but maybe there are some legitimate uses…

* Optional.  By default, it will compare ALL columns in the DataTable (aside from those designated by the StartTimeColumn, EndTimeColumn, UserIDColumn and KeyColumn properties).  You can override that behavior and compare only a subset of the columns in the DataTable by using either the IncludedColumns or ExcludedColumns properties.  Add columns to the IncludedColumns collection, and only those columns will be compared and included in the results.  Add columns to the ExcludedColumns Collection, and it will compare ALL columns except those (and the “special” columns noted above).  If both IncludedColumns and ExcludedColumns have columns in them, IncludedColumns will take precedent, and ExcludedColumns will be ignored.

	* You can also accomplish this by by including only the columns you want to compare in the SQL query when you create your DataTable.

* The Changes property will return an IEnumerable<RowChg> containing the differences it finds.  Note that Changes runs the comparison each time it’s called.  The results are NOT cached in the TemporalComparer object.  This could take significant time on a large DataTable.  If your code needs to call Changes multiple times, you should store the results from the first call (e.g. List<RowChg> MyChanges = comparer.Changes.ToList() ) and then use that variable to subsequently access the results.
Results

* Each RowChg object is guaranteed to have at least one ColChg object in its ColumnChanges property.

	* If two rows in your DataTable don’t have any changes (this can certainly happen if you’re only looking at a subset of the columns), no RowChg object is returned for those two rows.

* The results are sorted oldest->newest.
 
Here’s the Q&D I’ve been using to test with:
```
TemporalComparer tc = new TemporalComparer(MyDataTable);
       tc.UserIDColumn = MyDataTable.Columns["ModifiedUserId"];
       tc.KeyColumn = MyDataTable.Columns["Id"];
       ShowRC(tc.Changes);
private static void ShowRC(IEnumerable<RowChg> Changes)
       {
              foreach (RowChg rc in Changes)
              {
                     Console.WriteLine($"* Row Change: Key:{rc.Key} At:{rc.ChangeTime} by User ID:{rc.UserID} (row end time: {rc.PeriodEndTime})\n  Col changes:");
                     foreach (ColChg cc in rc.ColumnChanges)
                     { Console.WriteLine($"    {cc.ColumnName} old:[{cc.OldValue}] new:[{cc.NewValue}]"); }
                     Console.WriteLine("");
              }
       }
```
