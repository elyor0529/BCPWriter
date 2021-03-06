# C# library to manipulate Microsoft SQL Server bcp binary format

## What is bcp (Bulk Copy Program)?

From [Microsoft bcp documentation](http://msdn.microsoft.com/en-us/library/ms162802.aspx):

> The bcp utility bulk copies data between an instance of
> Microsoft SQL Server and a data file in a user-specified format.
> The bcp utility can be used to import large numbers of new rows into
> SQL Server tables or to export data out of tables into data files.

## Why use bcp?

Because bcp is the *fastest* way for inserting data into MS SQL Server.

## Example

```C#
BCPWriter writer = new BCPWriter();

// FirstName
writer.AddColumn(new SQLNVarChar(SQLNVarChar.MAX));

// LastName
writer.AddColumn(new SQLNVarChar(SQLNVarChar.MAX));

// Birth
writer.AddColumn(new SQLInt());

// Death
writer.AddColumn(new SQLInt());

List<object> rows = new List<object>();
rows.Add("Frederic Francois");
rows.Add("Chopin");
rows.Add(1810);
rows.Add(1849);

rows.Add("Franz");
rows.Add("Liszt");
rows.Add(1811);
rows.Add(1886);

rows.Add("George");
rows.Add("Sand");
rows.Add(1804);
rows.Add(1876);

BinaryWriter stream = new BinaryWriter(new FileStream("data.bcp", FileMode.Create));
writer.WriteRows(stream, rows);
stream.Close();
```

## Implementation

To achieve this I have reverse-engineered bcp binary format, see documentation inside
BCPWriter.cs file (use [Doxygen](http://www.doxygen.org/) to generate it).

The source code is clean, documented, heavily tested and should be stable.
A large amount of unit tests come with the source code.
