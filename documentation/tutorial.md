# Kusto-X Tutorial

We can conceptualize Kusto-X as a language and a runtime.

The Kusto-X language is a super set of the Kusto language.  Here is the most minimalistic Kusto-X script:

```kusto
@control-flow{
}
```

The '@' announces a Kusto-X script, like a '.' announces a Kusto command.

A *control flow* is a long running operation.  It allows us to add control flow to Kusto commands.

## Trivial Control Flow

Having a single Kusto ingestion command in a control flow results in a *trivial* control flow equilvalent to simply running the command:

```kusto
@control-flow{
    .append sampleTable <|
        datatable(name:string) ["Alice", "Bob"]
}
```

Here we used the [Kusto datatable operator](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/datatableoperator?pivots=azuredataexplorer) with the [set command](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/management/data-ingestion/ingest-from-query).

The only difference between running this control flow and running the command directly is that it would benificiate from the Kusto-X runtime:  it would be queued until an ingestion slot becomes available and it would be retried if transient failures occur.

## Snapshot

*Snapshot* allows us to take a snapshot of a query or command and store it in a named *variable*.

Kusto-X has a different syntax for those than the [Kusto let](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/letstatement).  This is because they are different concepts:

* *Let* is part of a query and is evaluated "within the same transaction" (with the same list of extents) than the query.
* A snapshot is taken at one point in time and *stored* in the control flow.  If the control flow runs for a while, the snapshot might not be in sync with the queries / commands it is applied to as it ran in a "different transaction".

Snapshots are used extensively in Kusto-X.  Here is a simple example:

```kusto
@control-flow{
    @snapshot(names){
        datatable(name:string) ["Alice", "Bob"]
    }

    .append sampleTable <|
        names
}
```

Snapshots are special instructions.  They need to be at the beginning of a scope.  They are executed first, in order and sequentially (regardless of the concurrency level).

A powerful feature of snapshots is they can be used in commands where literals (i.e. constants) are expected.  For instance:

```kusto
@control-flow{
    @snapshot(names){
        datatable(name:string) ["Alice", "Bob"]
    }

    @snapshot(date) with (scalar=true){
        print ago(3d)
    }

    .append sampleTable with (creationTime=date) <|
        names
}
```

This makes it very useful to use dynamic [ingestion properties](https://docs.microsoft.com/en-us/azure/data-explorer/ingestion-properties).

We noticed the `scalar=true` for the second snapshot.  This is required when we want to capture scalar as the content of the snapshot must be a valid Kusto query which only return tables.

A snapshot can capture a Kusto query or a Kusto command:

```kusto
@control-flow{
    @snapshot(names){
        .show tables 
        | project name=TableName
    }

    .append sampleTable <|
        names
}
```

## Grouping

A grouping allows to run more than one command:

```kusto
@control-flow{
    @grouping{
        .append sampleTable <|
            datatable(name:string) ["Alice", "Bob"]

        .append sampleTable <|
            datatable(name:string) ["Carl"]
    }
}
```

This control flow would first run the first command than the second one, sequentially.

### Concurrency

By default grouping runs commands sequentially.  They can also run commands in parallel:

```kusto
@control-flow{
    @grouping with (concurrency=2){
        .append sampleTable <|
            datatable(name:string) ["Alice", "Bob"]

        .append sampleTable <|
            datatable(name:string) ["Carl"]
    }
}
```

The level of concurrency can be any integer.  Zero (0) means no concurrency, which is equivalent to one (1) and is the default.  The effective concurrency level will obviously be subject to the ingestion slots available.

## If

*If* allows to branch according to data:

```kusto
@control-flow{
    @snapshot(tableNotExist) with (scalar=true){
        .show tables 
        | where TableName == "nyc_taxi"
        | count
        | project Count == 0
    }

    if(tableNotExist){
        .set sampleTable <|
            datatable(name:string) ["Alice", "Bob"]
    }
    else{
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
    @snapshot(names){
        datatable(name:string) ["Alice", "Bob"]
    }

    @foreach name in names
    {
        .append sampleTable <|
            print name=name

    }
}
```

*Foreach* expects a table and takes the first column to enumerate on.

## Until semantic

Currently, there is no "until" semantic in Kusto-X.

## Mixing instructions