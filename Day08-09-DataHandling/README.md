# 📁 Day 8-9: Data Handling

## 📋 Learning Objectives
- [ ] Master string manipulation and processing
- [ ] Implement file I/O operations (read/write files)
- [ ] Work with JSON serialization/deserialization
- [ ] Understand and use async workflows
- [ ] Handle different data formats (CSV, XML, JSON)
- [ ] Error handling in I/O operations

### 🔍 **Chi tiết Learning Objectives:**

#### 1. **Master string manipulation and processing**
```fsharp
// String processing basics
let countWords (text: string) =
    text.Split([|' '; '\t'; '\n'|], StringSplitOptions.RemoveEmptyEntries)
    |> Array.length

// String transformations
let processText text =
    text
    |> fun s -> s.Trim()
    |> fun s -> s.ToLower()
    |> fun s -> s.Replace("  ", " ")
    
// Pipeline approach
let analyzeText text =
    text
    |> countWords
    |> fun wordCount -> sprintf "Text has %d words" wordCount
```
**Mục tiêu**: Xử lý text data hiệu quả với functional pipelines

#### 2. **Implement file I/O operations**
```fsharp
// Safe file operations với Result type
let readTextFile (filePath: string) =
    try
        let content = File.ReadAllText(filePath)
        Ok content
    with ex ->
        Error(sprintf "Error reading file: %s" ex.Message)

// Async file operations
let readTextFileAsync (filePath: string) = async {
    try
        let! content = File.ReadAllTextAsync(filePath) |> Async.AwaitTask
        return Ok content
    with ex ->
        return Error(sprintf "Error reading file: %s" ex.Message)
}
```
**Mục tiêu**: Safe file I/O với proper error handling

#### 3. **Work with JSON serialization/deserialization**
```fsharp
// JSON với System.Text.Json
type Person = { Name: string; Age: int; City: string }

let serializeToJson person =
    JsonSerializer.Serialize(person)

let deserializeFromJson json =
    try
        JsonSerializer.Deserialize<Person>(json) |> Ok
    with ex ->
        Error(sprintf "JSON parsing error: %s" ex.Message)
```
**Mục tiêu**: Handle JSON data safely với error handling

#### 4. **Understand and use async workflows**
```fsharp
// Async data processing
let processFileAsync filePath = async {
    let! content = readTextFileAsync filePath
    match content with
    | Ok text -> 
        let wordCount = countWords text
        return Ok wordCount
    | Error msg -> 
        return Error msg
}

// Parallel processing
let processMultipleFiles files = async {
    let! results = 
        files 
        |> List.map processFileAsync
        |> Async.Parallel
    return results
}
```
**Mục tiêu**: Non-blocking I/O operations và parallel processing

#### 5. **Handle different data formats (CSV, XML, JSON)**
```fsharp
// CSV processing
let parseCsv (content: string) =
    content.Split('\n')
    |> Array.skip 1  // Skip header
    |> Array.map (fun line -> line.Split(','))
    |> Array.filter (fun fields -> fields.Length > 0)

// XML processing với pattern matching
let processXmlElement element =
    match element with
    | "person" -> "Processing person data"
    | "address" -> "Processing address data"
    | _ -> "Unknown element"
```
**Mục tiêu**: Flexible data format handling

#### 6. **Error handling in I/O operations**
```fsharp
// Comprehensive error handling
type FileError =
    | FileNotFound of string
    | AccessDenied of string
    | InvalidFormat of string
    | NetworkError of string

let safeFileOperation filePath =
    try
        let content = File.ReadAllText(filePath)
        Ok content
    with 
    | :? FileNotFoundException -> Error (FileNotFound filePath)
    | :? UnauthorizedAccessException -> Error (AccessDenied filePath)
    | ex -> Error (NetworkError ex.Message)
```
**Mục tiêu**: Robust error handling strategies cho I/O operations

### 🎯 **Tại sao những concept này quan trọng?**
- **Real-world Applications**: Most apps need to handle external data
- **Robustness**: Proper error handling prevents crashes
- **Performance**: Async operations keep UI responsive
- **Data Integration**: Handle multiple data sources và formats
- **Scalability**: Process large datasets efficiently
- **Maintainability**: Clean error handling và data pipelines

## 🛠️ Setup Requirements
```bash
# Add required packages
dotnet add package System.Text.Json
dotnet add package FSharp.Data
dotnet add package Newtonsoft.Json
```

## 📝 Code Examples & Exercises

### Exercise 1: String Processing
Create `StringProcessing.fs`:
```fsharp
// String manipulation functions
let countWords (text: string) =
    text.Split([|' '; '\t'; '\n'|], System.StringSplitOptions.RemoveEmptyEntries)
    |> Array.length

let countCharacters text =
    text |> String.length

let countVowels text =
    text.ToLower()
    |> Seq.filter (fun c -> "aeiou".Contains(c))
    |> Seq.length

// Advanced string processing
let removeExtraSpaces (text: string) =
    System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim()

let capitalizeWords (text: string) =
    text.Split(' ')
    |> Array.map (fun word -> 
        if word.Length > 0 then
            word.[0].ToString().ToUpper() + word.[1..].ToLower()
        else word)
    |> String.concat " "

// Text analysis pipeline
let analyzeText text =
    let words = countWords text
    let chars = countCharacters text
    let vowels = countVowels text
    let sentences = text.Split([|'.'; '!'; '?'|], System.StringSplitOptions.RemoveEmptyEntries).Length
    
    {
        Text = text
        WordCount = words
        CharCount = chars
        VowelCount = vowels
        SentenceCount = sentences
        AvgWordsPerSentence = if sentences > 0 then float words / float sentences else 0.0
    }
```

### Exercise 2: File Operations
Create `FileOperations.fs`:
```fsharp
open System.IO

// Basic file operations
let readTextFile filePath =
    try
        let content = File.ReadAllText(filePath)
        Ok content
    with
    | ex -> Error (sprintf "Failed to read file: %s" ex.Message)

let writeTextFile filePath content =
    try
        File.WriteAllText(filePath, content)
        Ok ()
    with
    | ex -> Error (sprintf "Failed to write file: %s" ex.Message)

let appendToFile filePath content =
    try
        File.AppendAllText(filePath, content)
        Ok ()
    with
    | ex -> Error (sprintf "Failed to append to file: %s" ex.Message)

// Log file management
type LogLevel = Info | Warning | Error

let writeLog logLevel message =
    let timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
    let levelStr = logLevel.ToString().ToUpper()
    let logEntry = sprintf "[%s] %s: %s\n" timestamp levelStr message
    
    let logFile = "application.log"
    appendToFile logFile logEntry

// CSV file processing
let readCsvLines filePath =
    try
        let lines = File.ReadAllLines(filePath)
        lines
        |> Array.map (fun line -> line.Split(','))
        |> Ok
    with
    | ex -> Error (sprintf "Failed to read CSV: %s" ex.Message)

let writeCsvLines filePath data =
    try
        let csvContent = 
            data
            |> Array.map (fun row -> String.concat "," row)
            |> String.concat "\n"
        File.WriteAllText(filePath, csvContent)
        Ok ()
    with
    | ex -> Error (sprintf "Failed to write CSV: %s" ex.Message)
```

### Exercise 3: JSON Handling
Create `JsonHandling.fs`:
```fsharp
open System.Text.Json
open System.Text.Json.Serialization

// Data types for JSON
type Person = {
    [<JsonPropertyName("id")>]
    Id: int
    [<JsonPropertyName("name")>]
    Name: string
    [<JsonPropertyName("email")>]
    Email: string
    [<JsonPropertyName("age")>]
    Age: int
}

type Configuration = {
    DatabaseUrl: string
    MaxConnections: int
    EnableLogging: bool
    Features: string list
}

// JSON serialization
let serializeToJson<'T> (obj: 'T) =
    try
        let json = JsonSerializer.Serialize(obj, JsonSerializerOptions(WriteIndented = true))
        Ok json
    with
    | ex -> Error (sprintf "Serialization failed: %s" ex.Message)

let deserializeFromJson<'T> (json: string) =
    try
        let obj = JsonSerializer.Deserialize<'T>(json)
        Ok obj
    with
    | ex -> Error (sprintf "Deserialization failed: %s" ex.Message)

// Configuration management
let loadConfiguration filePath =
    match readTextFile filePath with
    | Ok json -> deserializeFromJson<Configuration> json
    | Error msg -> Error msg

let saveConfiguration filePath config =
    match serializeToJson config with
    | Ok json -> writeTextFile filePath json
    | Error msg -> Error msg

// Working with JSON arrays
let loadPeopleFromJson filePath =
    match readTextFile filePath with
    | Ok json -> 
        match deserializeFromJson<Person[]> json with
        | Ok people -> Ok (Array.toList people)
        | Error msg -> Error msg
    | Error msg -> Error msg

let savePeopleToJson filePath people =
    let peopleArray = List.toArray people
    match serializeToJson peopleArray with
    | Ok json -> writeTextFile filePath json
    | Error msg -> Error msg
```

### Exercise 4: Async Workflows
Create `AsyncWorkflows.fs`:
```fsharp
open System.IO
open System.Net.Http

// Basic async file operations
let readFileAsync filePath = async {
    try
        let! content = File.ReadAllTextAsync(filePath) |> Async.AwaitTask
        return Ok content
    with
    | ex -> return Error (sprintf "Failed to read file: %s" ex.Message)
}

let writeFileAsync filePath content = async {
    try
        do! File.WriteAllTextAsync(filePath, content) |> Async.AwaitTask
        return Ok ()
    with
    | ex -> return Error (sprintf "Failed to write file: %s" ex.Message)
}

// Parallel file processing
let processFilesInParallel filePaths processor = async {
    let tasks = 
        filePaths
        |> List.map (fun path -> async {
            let! content = readFileAsync path
            match content with
            | Ok text -> return Some (path, processor text)
            | Error _ -> return None
        })
    
    let! results = Async.Parallel tasks
    return results |> Array.choose id |> Array.toList
}

// HTTP operations with async
let fetchUrlAsync url = async {
    use client = new HttpClient()
    try
        let! response = client.GetStringAsync(url) |> Async.AwaitTask
        return Ok response
    with
    | ex -> return Error (sprintf "HTTP request failed: %s" ex.Message)
}

// Download and save file
let downloadFileAsync url filePath = async {
    match! fetchUrlAsync url with
    | Ok content -> 
        match! writeFileAsync filePath content with
        | Ok () -> return Ok (sprintf "Downloaded %s to %s" url filePath)
        | Error msg -> return Error msg
    | Error msg -> return Error msg
}

// Batch processing with async
let processBatchAsync items processor batchSize = async {
    let batches = 
        items
        |> List.chunkBySize batchSize
    
    let results = ResizeArray()
    
    for batch in batches do
        let! batchResults = 
            batch
            |> List.map processor
            |> Async.Parallel
        
        results.AddRange(batchResults)
        
        // Small delay between batches
        do! Async.Sleep(100)
    
    return results |> Seq.toList
}
```

## 🏃‍♂️ Practice Tasks

### Task 1: Text File Analyzer
Build a program that:
1. Reads text files from a directory
2. Analyzes word frequency
3. Generates statistics report
4. Saves results to JSON

### Task 2: Log File Processor
Create a log analyzer that:
1. Parses different log formats
2. Filters by date range and log level
3. Aggregates statistics
4. Exports filtered logs

### Task 3: Configuration Manager
Implement a system that:
1. Loads app settings from JSON
2. Validates configuration
3. Supports environment-specific configs
4. Hot-reloads configuration changes

### Task 4: File Backup System
Build a backup utility that:
1. Scans directories for files
2. Compares file checksums
3. Copies changed files asynchronously
4. Maintains backup logs

## ✅ Completion Checklist
- [ ] Master string processing and manipulation
- [ ] Comfortable with file I/O operations
- [ ] Can serialize/deserialize JSON data
- [ ] Understand async workflows and parallel processing
- [ ] Implemented error handling for I/O operations
- [ ] Completed all 4 exercises
- [ ] Finished all 4 practice tasks

## 🎯 Next Steps
Ready for **Day 10-11: Functional Patterns** to learn advanced composition and error handling patterns!