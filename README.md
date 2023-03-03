# Kusto-X

![Shell Dev Deploy](https://github.com/vplauzon/kusto-x/workflows/Shell%20Dev%20Deploy/badge.svg)
![API continuous Build / Test](https://github.com/vplauzon/kusto-x/workflows/API%20continuous%20Build%20/%20Test/badge.svg)
![Web Portal continuous Build / Test](https://github.com/vplauzon/kusto-x/workflows/Web%20Portal%20continuous%20Build%20/%20Test/badge.svg)

Kusto-X is an extension to the Kusto Query Language.  The original aim is to POC to add control flows / long running operations to Kusto.

# Kusto-X Specs

We can conceptualize Kusto-X as a language and a runtime.

The Kusto-X language is a super set of the Kusto language.  Here is the most minimalistic Kusto-X script:

```kusto
@control-flow{
}
```

The '@' announces a Kusto-X specific command, like a '.' announces a Kusto command.

That control flow script is empty and doesn't do anything.

## Trivial Control Flow

Having a single Kusto ingestion command in a control flow results in a *trivial* control flow equilvalent to simply running the command:

```kusto
@control-flow{
    .create table T(Id:string)
}
```

The only difference between running this control flow and running the command directly is retry upon.

## Sequence

A sequence allows to run more than one command:

```kusto
@control-flow{
    .create table T1(Id:string)

    .create table T2(Id:string)
}
```

This control flow would first run the first command than the second one, sequentially.

We notice an empty line between commands:  **this is mandatory in Kusto-X to separate the ingestion commands and / or instructions within a sequence**.

Sequences are present at the root of a control flow, in an if-else statement and foreach-loops.

## Kusto-X commands

Here are a couple of commands unique to Kusto-X.

### .execute command-text

### .get blob list

### .queue ingest

### .queue export

### .await ingest

## Captures

`Capture` allows us to capture the value of a constant, a query, a Kusto/Kusto-X commands or query and store it in a named *captured value*.  Those can then be used in queries, commands and Kusto-X commands.  The same constrains apply for native queries & commands, i.e. captured values can't be used where litterals are expected (e.g. properties).

`Capture` has a different syntax than [Kusto let](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/letstatement).  This is because they are different concepts:

* *Let* is part of a query and is evaluated "within the same transaction" (with the same list of extents) than the query.
* A capture is taken at one point in time and *stored* in the control flow.  If the control flow runs for a while, the capture might not be in sync with the queries / commands it is applied to as it ran in a "different transaction".

Captures are used extensively in Kusto-X.  Here are different examples:

```kusto
@control-flow{
    @capture-scalar myConstant = print 2

    @capture-scalar myNumbers = print dynamic([1,2,3])

    @capture nameTable = datatable(name:string) ["Alice", "Bob"]

    @capture errorTable = T
        | where Level == "Error"
    
    @capture nodeTable = .show cluster

    @capture nonAdminNodeTable = .show cluster
        | where not(IsAdmin)

    @capture blobsTable = @get blob list

    @capture bigBlobsTable = @get blob list
        | where BlobSize > 1000000
}
```

Captures can't be declared in a grouping of concurrency higher than 1 since that would bring uncertainty between a capture happening and its value being referenced.

Capture values can then be used in other statements.  For instance:

```kusto
@control-flow{
    @capture names = datatable(name:string) ["Alice", "Bob"]

    .append sampleTable <|
        names
}
```

A capture value is strong type.  It can either be a Kusto table or a Kusto scalar.

## If

*If* allows to branch according to data:

```kusto
@control-flow{
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

A `if` can be by itself (i.e. without `else`) or `else if` can also be used to add branches.

## For each

*Foreach* allows to enumerate a table and run commands for each row:

```kusto
@control-flow{
    @capture names = datatable(name:string) ["Alice", "Bob"]

    @foreach name in names
    {
        .append sampleTable <|
            print name=name
    }
}
```

*Foreach* can enumerate on:

* A table, which case it enumerates on the first column
* A scalar of dynamic type representing an array

## Until semantic

Currently, there is no "until" semantic in Kusto-X.

## Mixing instructions