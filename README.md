# Kusto-X

[![Continuous Build / Unit Test](https://github.com/vplauzon/kusto-x/actions/workflows/continuous-build.yaml/badge.svg)](https://github.com/vplauzon/kusto-x/actions/workflows/continuous-build.yaml)
[![Continuous Integration](https://github.com/vplauzon/kusto-x/actions/workflows/continuous-int.yaml/badge.svg)](https://github.com/vplauzon/kusto-x/actions/workflows/continuous-int.yaml)
[![Integration tests](https://github.com/vplauzon/kusto-x/actions/workflows/int_tests.yaml/badge.svg)](https://github.com/vplauzon/kusto-x/actions/workflows/int_tests.yaml)


Kusto-X is an extension to the Kusto Query Language.  The original aim is to POC "procedures" in Kusto, i.e. long running operations with control flows.

The main scenarios it targets are about data engineering:

* Loading multiple blobs into Kusto (potentially partitioning it in certain ways, with `creationTime` for instance)
* Loading data in meta-format not supported by Kusto (e.g. Apache Iceberg)
* Materializing results in ways not supported by Update Policy / Materialized view (e.g. materializing a hierarchy)
* Orchestrating data movement
    * E.g. moving data between Kusto databases / clusters via partitioned exports
    * E.g. initial export (prior to continuous export) of a large table

It could also be used for orchestrating a long running query but it isn't a primary scenario

We can conceptualize Kusto-X as a language and a runtime.

The Kusto-X language is a super set of the Kusto language adding control flow statements (starting with a '@').

Queries and commands are identical in Kusto and Kusto-X (except Kusto-X queries and commands can use captured values).  Some commands are unique to Kusto-X.

Control flow statements are unique to Kusto-X.

# Control flow statements

Here is the most minimalistic way to run a Kusto-X procedure:

```kusto
.run-procedure <| {
}
```

This commands returns immediatly with a job-id (a string of character) identifying the instance of the job executing that procedure.

That procedure script is empty and doesn't do anything.

We can also persist a procedure (**TODO:  reference**).

Any control flow operations (starting with a `'@'`) must be inside a procedure.

## Trivial procedure

Having a single Kusto-X statement in a procedure results in a *trivial* procedure, equivalent to simply running the statement itself:

```kusto
.run-procedure <| {
    .create table T(Id:string)
}
```

The only difference between running this control flow and running the command directly is retry upon.  Also the procedure is run asynchronously (as a job).

## Sequence

A sequence allows to run more than one statement:

```kusto
.run-procedure <| {
    .create table T1(Id:string)

    .create table T2(Id:string)
}
```

This procedure would first run the first command than the second one, sequentially.

We notice an empty line between statements:  **this is mandatory in Kusto-X to separate the ingestion commands and / or instructions within a sequence**.  This is due to limitation in the Kusto-X parser.

## Captures

`Capture` allows us to capture the value of a constant, a query, a Kusto commands or control flow statement and store it in a named *captured value*.

Queries can use capture values unless litterals are expected.

Commands cannot use capture values unless the command is a Kusto-X command.

Control flow statements can use capture values (and typically can only use those).

All control flow statements return values (even `if` & `for`!).

`Capture` has a different syntax than [Kusto let](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/letstatement).  This is because they are different concepts:

* *Let* is part of a query and is evaluated "within the same transaction" (with the same list of extents) than the query.
* A capture is taken at one point in time and *stored* in the control flow.  If the control flow runs for a while, the capture might not be in sync with the queries / commands it is applied to as it ran in a "different transaction context".

Captures are used extensively in Kusto-X.  Here are different examples:

```kusto
.run-procedure <| {
    @capture-scalar myConstant = print 2

    @capture-scalar myNumbers = print dynamic([1,2,3])

    @capture nameTable = datatable(name:string) ["Alice", "Bob"]

    @capture errorTable = T
        | where Level == "Error"
    
    @capture nodeTable = .show cluster

    @capture nonAdminNodeTable = .show cluster
        | where not(IsAdmin)

    @capture blobsTable = .get blob list

    @capture bigBlobsTable = .get blob list
        | where BlobSize > 1000000
}
```

We noticed two forms of capture, one with scalar, the other without.  `capture-scalar` captures the returned value of a statement as a scalar.  This is useful to differentiate 1x1 tables and scalar.

Captured values can then be used in other statements.  For instance:

```kusto
.run-procedure <| {
    @capture names = datatable(name:string) ["Alice", "Bob"]

    .append sampleTable <|
        names
}
```

A captured value has a schema (or a type if it's a scalar).

## If

*If* allows to branch according to data:

```kusto
.run-procedure <| {
    @capture-scalar tableNotExist = .show tables 
        | where TableName == "nyc_taxi"
        | count
        | project Count == 0

    @if tableNotExist {
        .set sampleTable <|
            datatable(name:string) ["Alice", "Bob"]
    }
    @else{
        .append sampleTable <|
            datatable(name:string) ["Alice", "Bob"]
    }
}
```

A `if` can be by itself (i.e. without `else`), with `else`.  `else if` can also be used to add branches.

## For each

*Foreach* allows to enumerate a table and run commands for each row:

```kusto
.run-procedure <| {
    @capture names = datatable(name:string) ["Alice", "Bob"]

    @foreach(name in names) with(concurrency=2)
    {
        .append sampleTable <|
            print name=name
    }
}
```

*Foreach* can enumerate on:

* A table, in which case it enumerates on the first column
* A scalar of dynamic type representing an array

*Foreach*'s concurrency is optional.  It must be positive (i.e. greater than zero).  Default is 1.  A captured value can be used.

## Until semantic

Currently, there is no "until" semantic in Kusto-X.

## Kusto-X commands

Here are a couple of commands unique to Kusto-X.

### .run procedure

```
.run procedure <| {
    //  Procedure body
}
```

Alternatively, `.run proc` is also accepted.

### .show procedure runs

Here is a familly of commands allowing to probe into the runs of procedure, the steps of the runs and the results of the runs or individual steps.

Each command can be followed by a query to filter / transform the result.

For each command `procedure` can be replaced by `proc`.

Command|Description
-|-
`.show procedure runs`|List all the runs still in retention
`.show procedure runs <jobId>`|List only the run corresponding to the job
`.show procedure runs <jobId> result`|Show the result of the last step of the job
`.show procedure runs <jobId> history`|List the different states a run went through
`.show procedure runs <jobId> steps`|List all the steps of a given job
`.show procedure runs <jobId> steps <breadcrumb>`|List only the specified step
`.show procedure runs <jobId> steps <breadcrumb> children`|List the specified step and its immediate children
`.show procedure runs <jobId> steps <breadcrumb> result`|Show the result of a specified step
`.show procedure runs <jobId> steps <breadcrumb> history`|List all different states a step went through

Parameter `jobId` should be in quotes.

Parameter `breadcrumb` is a coma-separated list of integer, e.g. `[0, 1, 12]`.

### .get blobs

```
.get blobs 'https://myaccount.blob.core.windows.net/mycontainer/myfolder/'
```

### .queue ingest

### .queue export

### .queue export to

### @await ingest

## Examples

### Backfill

Here is an example for a backfill ingestion:

```kusto
.run-procedure <| {
    //  Fetch all blobs we want to ingest
    //  Infer the creation-time of each blob given their position in folder hierarchy
    @capture blobs = .get blobs 'https://myaccount.blob.core.windows.net/mycontainer/myfolder/'
        | parse Url with * '/myfolder/' year '/' month '/' day '/' *
        | extend DateTime = todatetime(strcat(year, '/', month, '/', day))
        | project bag_pack_columns(Url, DateTime)
    
    //  Queue each blob for ingestion and capture the continuation tokens
    @capture tokens = @foreach(blob in blobs)
        {
            .queue-ingest tostring(blob.Url) with(CreationTime=todatetime(blob.DateTime))
        }
    
    //  Wait for all ingestions to have proceeded
    @await tokens
}
```
