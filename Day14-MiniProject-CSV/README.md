# 📊 Day 14: Mini Project - CSV Processor

## 🎯 Project Objectives
- [ ] Build a comprehensive CSV processing application
- [ ] Implement data validation and cleaning
- [ ] Create statistical analysis features
- [ ] Generate reports in multiple formats
- [ ] Apply all F# concepts learned so far
- [ ] Create a production-ready CLI tool

## 📋 Project Requirements

### Core Features:
1. **CSV Reading**: Parse various CSV formats and encodings
2. **Data Validation**: Type checking and constraint validation
3. **Data Cleaning**: Handle missing values, duplicates, outliers
4. **Analysis**: Statistical calculations and aggregations
5. **Reporting**: Export to JSON, HTML, XML formats
6. **CLI Interface**: User-friendly command-line interface

### Technical Requirements:
1. Modular architecture with proper separation
2. Comprehensive error handling
3. Async processing for large files
4. Memory-efficient streaming
5. Extensible plugin architecture
6. Full test coverage

## 📝 Implementation Guide

### Step 1: Domain Models
Create `Models/Domain.fs`:
```fsharp
namespace CsvProcessor.Models

// Core data types
type CellValue =
    | Text of string
    | Number of decimal
    | Date of System.DateTime
    | Boolean of bool
    | Empty

type DataRow = {
    RowNumber: int
    Values: CellValue list
}

type Column = {
    Name: string
    Index: int
    DataType: System.Type
    IsRequired: bool
}

type CsvData = {
    Headers: Column list
    Rows: DataRow list
    FileName: string
    ProcessedAt: System.DateTime
}

// Validation types
type ValidationRule =
    | Required
    | MinLength of int
    | MaxLength of int
    | Range of decimal * decimal
    | Pattern of string
    | Custom of (CellValue -> bool)

type ValidationError = {
    RowNumber: int
    ColumnName: string
    Value: CellValue
    Rule: ValidationRule
    Message: string
}

type ValidationResult = {
    IsValid: bool
    Errors: ValidationError list
    ValidRows: DataRow list
    InvalidRows: DataRow list
}

// Analysis types
type StatisticType =
    | Count
    | Sum
    | Average
    | Min
    | Max
    | Median
    | StandardDeviation

type ColumnStatistics = {
    ColumnName: string
    Statistics: Map<StatisticType, decimal>
    UniqueValues: int
    NullCount: int
}

type DataSummary = {
    TotalRows: int
    ValidRows: int
    InvalidRows: int
    ColumnStatistics: ColumnStatistics list
    ProcessingTime: System.TimeSpan
}
```

### Step 2: CSV Reader Module
Create `IO/CsvReader.fs`:
```fsharp
namespace CsvProcessor.IO

open CsvProcessor.Models
open System
open System.IO
open System.Text

module CsvReader =
    
    type CsvOptions = {
        Delimiter: char
        HasHeaders: bool
        Encoding: Encoding
        QuoteChar: char
        EscapeChar: char option
        TrimWhitespace: bool
    }
    
    let defaultOptions = {
        Delimiter = ','
        HasHeaders = true
        Encoding = Encoding.UTF8
        QuoteChar = '"'
        EscapeChar = None
        TrimWhitespace = true
    }
    
    // Parse CSV line respecting quotes and escapes
    let parseLine delimiter quoteChar escapeChar line =
        let rec parseField chars inQuotes currentField fields =
            match chars with
            | [] -> 
                let field = String(List.rev currentField |> List.toArray)
                List.rev (field :: fields)
            | c :: rest when c = quoteChar && not inQuotes ->
                parseField rest true currentField fields
            | c :: rest when c = quoteChar && inQuotes ->
                parseField rest false currentField fields
            | c :: rest when c = delimiter && not inQuotes ->
                let field = String(List.rev currentField |> List.toArray)
                parseField rest false [] (field :: fields)
            | c :: rest ->
                parseField rest inQuotes (c :: currentField) fields
        
        line |> Seq.toList |> parseField [] false [] []
    
    // Infer data type from string value
    let inferDataType value =
        if String.IsNullOrWhiteSpace(value) then typeof<string>
        elif Decimal.TryParse(value) |> fst then typeof<decimal>
        elif DateTime.TryParse(value) |> fst then typeof<DateTime>
        elif Boolean.TryParse(value) |> fst then typeof<bool>
        else typeof<string>
    
    // Parse cell value based on inferred type
    let parseCellValue value dataType =
        if String.IsNullOrWhiteSpace(value) then Empty
        elif dataType = typeof<decimal> then
            match Decimal.TryParse(value) with
            | (true, num) -> Number num
            | _ -> Text value
        elif dataType = typeof<DateTime> then
            match DateTime.TryParse(value) with
            | (true, date) -> Date date
            | _ -> Text value
        elif dataType = typeof<bool> then
            match Boolean.TryParse(value) with
            | (true, bool) -> Boolean bool
            | _ -> Text value
        else Text value
    
    // Read CSV file async
    let readCsvAsync filePath options = async {
        try
            let! lines = File.ReadAllLinesAsync(filePath, options.Encoding) |> Async.AwaitTask
            
            if Array.isEmpty lines then
                return Error "File is empty"
            else
                let parsedLines = 
                    lines 
                    |> Array.map (parseLine options.Delimiter options.QuoteChar options.EscapeChar)
                
                let (headers, dataLines) = 
                    if options.HasHeaders then
                        (Array.head parsedLines, Array.tail parsedLines)
                    else
                        let headerCount = Array.head parsedLines |> List.length
                        let generatedHeaders = [1..headerCount] |> List.map (sprintf "Column%d")
                        (generatedHeaders, parsedLines)
                
                // Infer column types from data
                let columnTypes = 
                    if Array.isEmpty dataLines then
                        headers |> List.map (fun _ -> typeof<string>)
                    else
                        let firstDataRow = Array.head dataLines
                        firstDataRow |> List.map (fun value -> 
                            if options.TrimWhitespace then inferDataType (value.Trim())
                            else inferDataType value
                        )
                
                let columns = 
                    List.zip3 headers columnTypes [0..List.length headers - 1]
                    |> List.map (fun (name, dataType, index) -> {
                        Name = name
                        Index = index
                        DataType = dataType
                        IsRequired = false
                    })
                
                let rows = 
                    dataLines
                    |> Array.mapi (fun i line ->
                        let values = 
                            List.zip line columnTypes
                            |> List.map (fun (value, dataType) ->
                                let cleanValue = if options.TrimWhitespace then value.Trim() else value
                                parseCellValue cleanValue dataType
                            )
                        {
                            RowNumber = i + 1
                            Values = values
                        }
                    )
                    |> Array.toList
                
                let csvData = {
                    Headers = columns
                    Rows = rows
                    FileName = Path.GetFileName(filePath)
                    ProcessedAt = DateTime.Now
                }
                
                return Ok csvData
        with
        | ex -> return Error (sprintf "Failed to read CSV: %s" ex.Message)
    }
    
    // Streaming reader for large files
    let readCsvStreamAsync filePath options batchSize processor = async {
        use reader = new StreamReader(filePath, options.Encoding)
        let mutable lineNumber = 0
        let mutable batch = []
        
        try
            // Read headers
            let! headerLine = reader.ReadLineAsync() |> Async.AwaitTask
            if isNull headerLine then
                return Error "File is empty"
            else
                let headers = parseLine options.Delimiter options.QuoteChar options.EscapeChar headerLine
                
                let rec processLines () = async {
                    let! line = reader.ReadLineAsync() |> Async.AwaitTask
                    if isNull line then
                        // Process final batch
                        if not (List.isEmpty batch) then
                            do! processor (List.rev batch)
                        return Ok ()
                    else
                        lineNumber <- lineNumber + 1
                        let parsedLine = parseLine options.Delimiter options.QuoteChar options.EscapeChar line
                        let newBatch = parsedLine :: batch
                        
                        if List.length newBatch >= batchSize then
                            do! processor (List.rev newBatch)
                            batch <- []
                        else
                            batch <- newBatch
                        
                        return! processLines ()
                }
                
                return! processLines ()
        with
        | ex -> return Error (sprintf "Failed to stream CSV: %s" ex.Message)
    }
```

### Step 3: Data Validation Module
Create `Processing/Validation.fs`:
```fsharp
namespace CsvProcessor.Processing

open CsvProcessor.Models
open System.Text.RegularExpressions

module Validation =
    
    let validateCell rule columnName rowNumber value =
        let createError message = {
            RowNumber = rowNumber
            ColumnName = columnName
            Value = value
            Rule = rule
            Message = message
        }
        
        match rule, value with
        | Required, Empty -> Some (createError "Value is required")
        | MinLength minLen, Text text when text.Length < minLen -> 
            Some (createError (sprintf "Minimum length is %d" minLen))
        | MaxLength maxLen, Text text when text.Length > maxLen -> 
            Some (createError (sprintf "Maximum length is %d" maxLen))
        | Range (min, max), Number num when num < min || num > max -> 
            Some (createError (sprintf "Value must be between %M and %M" min max))
        | Pattern pattern, Text text when not (Regex.IsMatch(text, pattern)) -> 
            Some (createError (sprintf "Value must match pattern: %s" pattern))
        | Custom validator, value when not (validator value) -> 
            Some (createError "Custom validation failed")
        | _ -> None
    
    let validateRow (rules: Map<string, ValidationRule list>) (columns: Column list) (row: DataRow) =
        let columnMap = columns |> List.map (fun c -> (c.Index, c.Name)) |> Map.ofList
        
        row.Values
        |> List.mapi (fun index value ->
            match Map.tryFind index columnMap with
            | Some columnName ->
                match Map.tryFind columnName rules with
                | Some columnRules ->
                    columnRules
                    |> List.choose (fun rule -> validateCell rule columnName row.RowNumber value)
                | None -> []
            | None -> []
        )
        |> List.concat
    
    let validateData rules csvData =
        let startTime = System.DateTime.Now
        
        let allErrors = 
            csvData.Rows
            |> List.collect (validateRow rules csvData.Headers)
        
        let errorsByRow = 
            allErrors 
            |> List.groupBy (fun e -> e.RowNumber)
            |> Map.ofList
        
        let validRows = 
            csvData.Rows
            |> List.filter (fun row -> not (Map.containsKey row.RowNumber errorsByRow))
        
        let invalidRows = 
            csvData.Rows
            |> List.filter (fun row -> Map.containsKey row.RowNumber errorsByRow)
        
        let endTime = System.DateTime.Now
        
        {
            IsValid = List.isEmpty allErrors
            Errors = allErrors
            ValidRows = validRows
            InvalidRows = invalidRows
        }
```

### Step 4: Statistics and Analysis
Create `Analysis/Statistics.fs`:
```fsharp
namespace CsvProcessor.Analysis

open CsvProcessor.Models

module Statistics =
    
    let calculateColumnStatistics columnIndex columnName (rows: DataRow list) =
        let values = 
            rows
            |> List.choose (fun row ->
                if columnIndex < List.length row.Values then
                    match List.item columnIndex row.Values with
                    | Number n -> Some n
                    | _ -> None
                else None
            )
        
        let sortedValues = List.sort values
        let count = List.length values
        let nullCount = rows.Length - count
        
        let statistics = 
            if List.isEmpty values then Map.empty
            else
                let sum = List.sum values
                let average = sum / decimal count
                let min = List.min values
                let max = List.max values
                
                let median = 
                    if count % 2 = 0 then
                        let mid1 = sortedValues.[count / 2 - 1]
                        let mid2 = sortedValues.[count / 2]
                        (mid1 + mid2) / 2m
                    else
                        sortedValues.[count / 2]
                
                let variance = 
                    values
                    |> List.map (fun x -> (x - average) * (x - average))
                    |> List.average
                
                let stdDev = sqrt (float variance) |> decimal
                
                [
                    (Count, decimal count)
                    (Sum, sum)
                    (Average, average)
                    (Min, min)
                    (Max, max)
                    (Median, median)
                    (StandardDeviation, stdDev)
                ] |> Map.ofList
        
        let uniqueValues = 
            rows
            |> List.map (fun row -> List.item columnIndex row.Values)
            |> List.distinct
            |> List.length
        
        {
            ColumnName = columnName
            Statistics = statistics
            UniqueValues = uniqueValues
            NullCount = nullCount
        }
    
    let generateDataSummary (csvData: CsvData) =
        let startTime = System.DateTime.Now
        
        let columnStats = 
            csvData.Headers
            |> List.map (fun col -> calculateColumnStatistics col.Index col.Name csvData.Rows)
        
        let endTime = System.DateTime.Now
        
        {
            TotalRows = List.length csvData.Rows
            ValidRows = List.length csvData.Rows // Assuming all rows are valid for now
            InvalidRows = 0
            ColumnStatistics = columnStats
            ProcessingTime = endTime - startTime
        }
```

## 🏃‍♂️ Practice Enhancements

### Enhancement 1: Advanced Analysis
Add features for:
1. Correlation analysis between columns
2. Outlier detection algorithms
3. Data quality scoring
4. Trend analysis for time series

### Enhancement 2: Export Formats
Implement exporters for:
1. Excel files (XLSX)
2. Database formats (SQL inserts)
3. Parquet files
4. Charts and visualizations

### Enhancement 3: Performance Optimization
Optimize for:
1. Memory usage with streaming
2. Parallel processing
3. Caching strategies
4. Progress reporting

### Enhancement 4: Web Interface
Build a web UI with:
1. File upload functionality
2. Interactive data preview
3. Real-time processing status
4. Downloadable reports

## ✅ Completion Checklist
- [ ] Implemented CSV parsing with various options
- [ ] Added comprehensive data validation
- [ ] Created statistical analysis features
- [ ] Built report generation in multiple formats
- [ ] Included proper error handling
- [ ] Added async processing for large files
- [ ] Created user-friendly CLI interface
- [ ] Written comprehensive tests

## 🎯 Next Steps
Ready for **Day 15-16: Web with Giraffe** to build web applications with F#!