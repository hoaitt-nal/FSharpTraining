// Day 8-9: Data Handling - Practice Tasks Implementation
open System
open System.IO
open System.Text.Json
open System.Security.Cryptography
open System.Text.RegularExpressions

printfn "============ Day 8-9: Data Handling Practice Tasks ============"

printfn "\n============ Task 1: Text File Analyzer ============"

// Task 1: Text File Analyzer Types
type WordFrequency = { Word: string; Count: int }

type TextStatistics = {
    FileName: string
    WordCount: int
    CharacterCount: int
    LineCount: int
    WordFrequencies: WordFrequency list
    MostCommonWord: string
    FileSize: int64
}

// Text analysis functions
let countWords (text: string) =
    text.Split([| ' '; '\t'; '\n'; '\r'; '.'; ','; ';'; '!'; '?' |], StringSplitOptions.RemoveEmptyEntries)
    |> Array.length

let countLines (text: string) =
    text.Split([| '\n'; '\r' |], StringSplitOptions.RemoveEmptyEntries) |> Array.length

let calculateWordFrequency (text: string) =
    text.ToLower().Split([| ' '; '\t'; '\n'; '\r'; '.'; ','; ';'; '!'; '?'; '('; ')'; '"' |], StringSplitOptions.RemoveEmptyEntries)
    |> Array.groupBy id
    |> Array.map (fun (word, occurrences) -> { Word = word; Count = Array.length occurrences })
    |> Array.sortByDescending (fun wf -> wf.Count)
    |> Array.toList

let analyzeTextFile (filePath: string) =
    try
        let content = File.ReadAllText(filePath)
        let fileInfo = FileInfo(filePath)
        let frequencies = calculateWordFrequency content
        let mostCommon = match frequencies with | head :: _ -> head.Word | [] -> ""
        
        Ok {
            FileName = Path.GetFileName(filePath)
            WordCount = countWords content
            CharacterCount = content.Length
            LineCount = countLines content
            WordFrequencies = frequencies |> List.take (min 5 frequencies.Length) // Top 5 words
            MostCommonWord = mostCommon
            FileSize = fileInfo.Length
        }
    with ex ->
        Error (sprintf "Error analyzing file %s: %s" filePath ex.Message)

// JSON serialization
let saveStatisticsToJson (stats: TextStatistics list) =
    try
        let json = JsonSerializer.Serialize(stats, JsonSerializerOptions(WriteIndented = true))
        let fileName = sprintf "text_analysis_%s.json" (DateTime.Now.ToString("yyyyMMdd_HHmmss"))
        File.WriteAllText(fileName, json)
        printfn "📄 Statistics saved to: %s" fileName
        Ok fileName
    with ex ->
        Error (sprintf "Error saving JSON: %s" ex.Message)

// Test Task 1
let testTextAnalyzer () =
    printfn "=== Text File Analyzer Test ==="
    
    // Create sample text files
    let sampleTexts = [
        ("sample1.txt", "The quick brown fox jumps over the lazy dog. The fox is quick and brown.")
        ("sample2.txt", "F# programming language is functional and powerful. F# makes programming fun and efficient.")
        ("sample3.txt", "Data analysis requires careful handling of files and text processing in modern applications.")
    ]
    
    sampleTexts |> List.iter (fun (name, content) -> File.WriteAllText(name, content))
    
    let results = 
        sampleTexts 
        |> List.map (fun (name, _) -> analyzeTextFile name)
        |> List.choose (function | Ok stat -> Some stat | Error msg -> printfn "❌ %s" msg; None)
    
    results |> List.iter (fun stat ->
        printfn "📄 File: %s" stat.FileName
        printfn "   Words: %d, Characters: %d, Lines: %d" stat.WordCount stat.CharacterCount stat.LineCount
        printfn "   Most common word: %s" stat.MostCommonWord
        printfn "   Top words: %s" (stat.WordFrequencies |> List.map (fun wf -> sprintf "%s(%d)" wf.Word wf.Count) |> String.concat ", ")
    )
    
    match saveStatisticsToJson results with
    | Ok fileName -> printfn "✅ Analysis complete, saved to %s" fileName
    | Error msg -> printfn "❌ %s" msg
    
    // Cleanup
    sampleTexts |> List.iter (fun (name, _) -> if File.Exists(name) then File.Delete(name))
    () // Add unit return

printfn "\n============ Task 2: Log File Processor ============"

// Task 2: Log File Processor Types
type LogLevel = | Info | Warning | Error | Debug

type LogEntry = {
    Timestamp: DateTime
    Level: LogLevel
    Message: string
    Source: string
}

type LogStatistics = {
    TotalEntries: int
    InfoCount: int
    WarningCount: int
    ErrorCount: int
    DebugCount: int
    DateRange: DateTime * DateTime
}

// Log parsing functions
let parseLogLevel (levelStr: string) =
    match levelStr.ToUpper() with
    | "INFO" -> Info
    | "WARNING" | "WARN" -> Warning  
    | "ERROR" -> Error
    | "DEBUG" -> Debug
    | _ -> Info

let parseLogEntry (line: string) =
    try
        // Pattern: "2024-11-27 10:30:45 [INFO] Application: Starting application"
        let pattern = @"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) \[(\w+)\] (\w+): (.+)"
        let match' = Regex.Match(line, pattern)
        
        if match'.Success then
            let timestamp = DateTime.Parse(match'.Groups.[1].Value)
            let level = parseLogLevel match'.Groups.[2].Value
            let source = match'.Groups.[3].Value
            let message = match'.Groups.[4].Value
            
            Some { Timestamp = timestamp; Level = level; Message = message; Source = source }
        else
            None
    with _ -> None

let parseLogFile (filePath: string) =
    try
        let lines = File.ReadAllLines(filePath)
        let entries = lines |> Array.choose parseLogEntry |> Array.toList
        Ok entries
    with ex ->
        Result.Error (sprintf "Error parsing log file %s: %s" filePath ex.Message)

let filterLogsByDateRange (startDate: DateTime) (endDate: DateTime) (entries: LogEntry list) =
    entries |> List.filter (fun entry -> entry.Timestamp >= startDate && entry.Timestamp <= endDate)

let filterLogsByLevel (level: LogLevel) (entries: LogEntry list) =
    entries |> List.filter (fun entry -> entry.Level = level)

let calculateLogStatistics (entries: LogEntry list) =
    if List.isEmpty entries then
        { TotalEntries = 0; InfoCount = 0; WarningCount = 0; ErrorCount = 0; DebugCount = 0; 
          DateRange = (DateTime.MinValue, DateTime.MinValue) }
    else
        let timestamps = entries |> List.map (fun e -> e.Timestamp)
        {
            TotalEntries = List.length entries
            InfoCount = entries |> List.filter (fun e -> e.Level = Info) |> List.length
            WarningCount = entries |> List.filter (fun e -> e.Level = Warning) |> List.length
            ErrorCount = entries |> List.filter (fun e -> e.Level = Error) |> List.length
            DebugCount = entries |> List.filter (fun e -> e.Level = Debug) |> List.length
            DateRange = (List.min timestamps, List.max timestamps)
        }

// Test Task 2
let testLogProcessor () =
    printfn "=== Log File Processor Test ==="
    
    let sampleLogContent = 
        "2024-11-27 10:30:45 [INFO] Application: Starting application\n" +
        "2024-11-27 10:30:46 [DEBUG] Database: Connecting to database\n" +
        "2024-11-27 10:30:47 [INFO] Database: Connected successfully\n" +
        "2024-11-27 10:35:15 [WARNING] Authentication: Failed login attempt for user 'admin'\n" +
        "2024-11-27 10:40:22 [ERROR] FileSystem: Unable to access file '/tmp/data.txt'\n" +
        "2024-11-27 10:45:33 [INFO] Application: Processing completed"
    
    let logFileName = "sample.log"
    File.WriteAllText(logFileName, sampleLogContent)
    
    match parseLogFile logFileName with
    | Ok entries ->
        printfn "📊 Parsed %d log entries" (List.length entries)
        
        let stats = calculateLogStatistics entries
        printfn "   Total: %d, Info: %d, Warning: %d, Error: %d, Debug: %d" 
            stats.TotalEntries stats.InfoCount stats.WarningCount stats.ErrorCount stats.DebugCount
            
        let (startDate, endDate) = stats.DateRange
        printfn "   Date range: %s to %s" (startDate.ToString("yyyy-MM-dd HH:mm:ss")) (endDate.ToString("yyyy-MM-dd HH:mm:ss"))
        
        // Filter examples
        let errorLogs = filterLogsByLevel LogLevel.Error entries
        printfn "   Error logs: %d" (List.length errorLogs)
        
        errorLogs |> List.iter (fun log ->
            printfn "     [%s] %s: %s" (log.Timestamp.ToString("HH:mm:ss")) log.Source log.Message)
            
    | Result.Error msg ->
        printfn "❌ %s" msg
    
    // Cleanup
    if File.Exists(logFileName) then File.Delete(logFileName)
    () // Add unit return

printfn "\n============ Task 3: Configuration Manager ============"

// Task 3: Configuration Manager Types
type DatabaseConfig = {
    ConnectionString: string
    MaxConnections: int
    Timeout: int
}

type LoggingConfig = {
    Level: string
    FilePath: string
    MaxFileSize: int64
}

type AppConfig = {
    AppName: string
    Version: string
    Database: DatabaseConfig
    Logging: LoggingConfig
    DebugMode: bool
}

// Configuration functions
let loadConfigFromJson (filePath: string) =
    try
        let json = File.ReadAllText(filePath)
        let config = JsonSerializer.Deserialize<AppConfig>(json)
        Ok config
    with ex ->
        Result.Error (sprintf "Error loading config: %s" ex.Message)

let validateConfig (config: AppConfig) =
    let errors = [
        if String.IsNullOrWhiteSpace(config.AppName) then "AppName is required"
        if String.IsNullOrWhiteSpace(config.Database.ConnectionString) then "Database ConnectionString is required"
        if config.Database.MaxConnections <= 0 then "Database MaxConnections must be positive"
        if config.Database.Timeout <= 0 then "Database Timeout must be positive"
        if String.IsNullOrWhiteSpace(config.Logging.FilePath) then "Logging FilePath is required"
        if config.Logging.MaxFileSize <= 0L then "Logging MaxFileSize must be positive"
    ]
    
    if List.isEmpty errors then Ok config
    else Result.Error ("Configuration validation failed: " + String.concat "; " errors)

let saveConfigToJson (config: AppConfig) (filePath: string) =
    try
        let json = JsonSerializer.Serialize(config, JsonSerializerOptions(WriteIndented = true))
        File.WriteAllText(filePath, json)
        Ok filePath
    with ex ->
        Result.Error (sprintf "Error saving config: %s" ex.Message)

// Test Task 3
let testConfigurationManager () =
    printfn "=== Configuration Manager Test ==="
    
    let sampleConfig = {
        AppName = "MyFSharpApp"
        Version = "1.0.0"
        Database = {
            ConnectionString = "Server=localhost;Database=mydb;Trusted_Connection=true"
            MaxConnections = 100
            Timeout = 30
        }
        Logging = {
            Level = "Info"
            FilePath = "/var/log/app.log"
            MaxFileSize = 10485760L // 10MB
        }
        DebugMode = true
    }
    
    let configFileName = "appsettings.json"
    
    match saveConfigToJson sampleConfig configFileName with
    | Ok _ ->
        printfn "✅ Configuration saved to %s" configFileName
        
        match loadConfigFromJson configFileName with
        | Ok loadedConfig ->
            printfn "✅ Configuration loaded successfully"
            printfn "   App: %s v%s" loadedConfig.AppName loadedConfig.Version
            printfn "   Database: MaxConnections=%d, Timeout=%d" loadedConfig.Database.MaxConnections loadedConfig.Database.Timeout
            printfn "   Logging: Level=%s, MaxFileSize=%d MB" loadedConfig.Logging.Level (loadedConfig.Logging.MaxFileSize / 1024L / 1024L)
            
            match validateConfig loadedConfig with
            | Ok _ -> printfn "✅ Configuration validation passed"
            | Result.Error msg -> printfn "❌ Validation failed: %s" msg
            
        | Result.Error msg ->
            printfn "❌ Error loading config: %s" msg
            
    | Result.Error msg ->
        printfn "❌ Error saving config: %s" msg
    
    // Cleanup
    if File.Exists(configFileName) then File.Delete(configFileName)
    () // Add unit return

printfn "\n============ Task 4: File Backup System ============"

// Task 4: File Backup System Types
type FileChecksum = {
    FilePath: string
    Hash: string
    Size: int64
    LastModified: DateTime
}

type BackupOperation = {
    SourceFile: string
    DestinationFile: string
    Operation: string // "Copy", "Skip", "Update"
    Timestamp: DateTime
}

// File backup functions
let calculateFileChecksum (filePath: string) =
    try
        use md5 = MD5.Create()
        use stream = File.OpenRead(filePath)
        let hashBytes = md5.ComputeHash(stream)
        let hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower()
        let fileInfo = FileInfo(filePath)
        
        Ok {
            FilePath = filePath
            Hash = hash
            Size = fileInfo.Length
            LastModified = fileInfo.LastWriteTime
        }
    with ex ->
        Result.Error (sprintf "Error calculating checksum for %s: %s" filePath ex.Message)

let scanDirectory (directoryPath: string) (pattern: string) =
    try
        let files = Directory.GetFiles(directoryPath, pattern, SearchOption.AllDirectories)
        Ok (Array.toList files)
    with ex ->
        Result.Error (sprintf "Error scanning directory: %s" ex.Message)

let compareChecksums (sourceChecksum: FileChecksum) (destChecksumOpt: FileChecksum option) =
    match destChecksumOpt with
    | None -> "Copy" // File doesn't exist in destination
    | Some destChecksum ->
        if sourceChecksum.Hash = destChecksum.Hash then "Skip" // Files are identical
        else "Update" // Files are different

let performBackupOperation (sourceDir: string) (destDir: string) (relativePath: string) (operation: string) =
    async {
        try
            let sourceFile = Path.Combine(sourceDir, relativePath)
            let destFile = Path.Combine(destDir, relativePath)
            
            match operation with
            | "Copy" | "Update" ->
                let destDirectory = Path.GetDirectoryName(destFile)
                if not (Directory.Exists(destDirectory)) then
                    Directory.CreateDirectory(destDirectory) |> ignore
                
                let! bytes = File.ReadAllBytesAsync(sourceFile) |> Async.AwaitTask
                do! File.WriteAllBytesAsync(destFile, bytes) |> Async.AwaitTask
                
                return Ok {
                    SourceFile = sourceFile
                    DestinationFile = destFile
                    Operation = operation
                    Timestamp = DateTime.Now
                }
            | "Skip" ->
                return Ok {
                    SourceFile = sourceFile
                    DestinationFile = destFile
                    Operation = "Skip"
                    Timestamp = DateTime.Now
                }
            | _ ->
                return Result.Error (sprintf "Unknown operation: %s" operation)
        with ex ->
            return Result.Error (sprintf "Backup operation failed for %s: %s" relativePath ex.Message)
    }

// Test Task 4
let testFileBackupSystem () = async {
    printfn "=== File Backup System Test ==="
    
    let sourceDir = "source_backup_test"
    let destDir = "dest_backup_test"
    
    // Create test directories and files
    Directory.CreateDirectory(sourceDir) |> ignore
    Directory.CreateDirectory(destDir) |> ignore
    
    let testFiles = [
        ("file1.txt", "This is the content of file 1.")
        ("subdir/file2.txt", "This is the content of file 2 in a subdirectory.")
        ("file3.txt", "Some other content for testing purposes.")
    ]
    
    testFiles |> List.iter (fun (relativePath, content) ->
        let fullPath = Path.Combine(sourceDir, relativePath)
        let dir = Path.GetDirectoryName(fullPath)
        if not (Directory.Exists(dir)) then Directory.CreateDirectory(dir) |> ignore
        File.WriteAllText(fullPath, content)
    )
    
    printfn "📁 Created test files in %s" sourceDir
    
    // Perform backup operations
    match scanDirectory sourceDir "*.*" with
    | Ok files ->
        printfn "📊 Found %d files to backup" (List.length files)
        
        let! results = 
            files
            |> List.map (fun file ->
                let relativePath = Path.GetRelativePath(sourceDir, file)
                performBackupOperation sourceDir destDir relativePath "Copy")
            |> Async.Parallel
        
        let (successful, failed) = 
            results 
            |> Array.partition (function | Ok _ -> true | Result.Error _ -> false)
        
        printfn "✅ Backup completed: %d successful, %d failed" (Array.length successful) (Array.length failed)
        
        successful |> Array.iter (function
            | Ok operation -> printfn "   %s: %s -> %s" operation.Operation (Path.GetFileName(operation.SourceFile)) (Path.GetFileName(operation.DestinationFile))
            | Result.Error _ -> ())
            
        failed |> Array.iter (function
            | Result.Error msg -> printfn "   ❌ %s" msg
            | Ok _ -> ())
            
    | Result.Error msg ->
        printfn "❌ %s" msg
    
    // Cleanup
    if Directory.Exists(sourceDir) then Directory.Delete(sourceDir, true)
    if Directory.Exists(destDir) then Directory.Delete(destDir, true)
}

// ============ Main Program ============

[<EntryPoint>]
let main argv =
    async {
        try
            printfn "\n🚀 Running all Practice Tasks...\n"
            
            // Run Task 1: Text File Analyzer
            testTextAnalyzer ()
            
            printfn ""
            
            // Run Task 2: Log File Processor  
            testLogProcessor ()
            
            printfn ""
            
            // Run Task 3: Configuration Manager
            testConfigurationManager ()
            
            printfn ""
            
            // Run Task 4: File Backup System
            do! testFileBackupSystem ()
            
            printfn "\n🎉 All Practice Tasks completed successfully!"
            return 0
            
        with ex ->
            printfn "💥 Error: %s" ex.Message
            return 1
    } |> Async.RunSynchronously
