# DLINQ

DLINQ is a text-based query language which provided by Pomelo. It can be used for accessing data from `IEnumerable`/`IQueryable` interfaces. 

## Query Command
A query command is consisted of `subject` and commands. A query command should start with the subject. Usually the subject is the name of table. 
And each command starts with `|`, command name is placed after `|`. 

Here is a sample for DLINQ(Text-based):

``` ql
Logs // subject
| where time >= Convert.ToDateTime("2021-09-12") // command
| where server == "127.0.0.1" // command
| groupby severity // command
| select new(severity as Sev, Count() as Count) // command
```

The above `DLINQ` will be compiled into `Dynamic LINQ` at runtime as below:

```c#
queryContext.Logs
    .Where("time >= Convert.ToDateTime(\"2021-09-12\")")
    .Where("server == \"127.0.0.1\"")
    .GroupBy("severity")
    .Select("new(severity as Sev, Count() as Count)")
```

Then the `Dynamic LINQ` will be compiled into native `LINQ` at runtime as below:

```c#
queryContext.Logs
    .Where(x => x.Time >= Convert.ToDateTime("2021-09-12"))
    .Where(x => x.Server == "127.0.0.1")
    .GroupBy(x => x.Severity)
    .Select(x => new { Sev = x.Severity, Count = x.Count() });
```

We can use most of the methods which provided by `dynamic-linq` as the command name. And use `,` to split arguments. 
You can refer to [Dynamic LINQ documents](https://dynamic-linq.net/basic-query-operators) to learn the method definitions.

For most of these methods usage, we should follow this pattern to pass arguments.

Syntax:
```ql
| <method> <arg1>, <arg2>, <arg_n>
```

Sample:

```ql
Logs
| skip 10
| take 10
```

But we have some special cases to prettify some methods like `join`. 
We listed the special cases as below:

### Join / GroupJoin

Syntax:

```ql
| join [<table_name>] on inner.<inner_property> == outer.<outer_property> into <new_structure_definition>
```

```ql
| groupjoin [<table_name>] on inner.<inner_property> == outer.<outer_property> into <new_structure_definition>
```

Sample:

```ql
Users
| where Level >= 5
| join [items] on inner.UserId == outer.Name into new(outer.Name as Username, inner.Name as ItemName)
```
