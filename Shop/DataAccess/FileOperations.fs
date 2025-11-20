namespace Shop.DataAccess

module FileOperations =
    open System
    open System.IO
    open Shop.Models

    // Day 8-9: File I/O operations
    let ensureDirectoryExists (path: string) =
        let directory = Path.GetDirectoryName(path)
        if not (Directory.Exists(directory)) then
            Directory.CreateDirectory(directory) |> ignore

    let writeTextFile (path: string) (content: string) =
        async {
            try
                ensureDirectoryExists path
                do! File.WriteAllTextAsync(path, content) |> Async.AwaitTask
                return Ok ()
            with
            | ex -> return Error (FileError ex.Message)
        }

    let readTextFile (path: string) =
        async {
            try
                if File.Exists(path) then
                    let! content = File.ReadAllTextAsync(path) |> Async.AwaitTask
                    return Ok content
                else
                    return Error (FileError $"File not found: {path}")
            with
            | ex -> return Error (FileError ex.Message)
        }

    let appendToFile (path: string) (content: string) =
        async {
            try
                ensureDirectoryExists path
                let timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                let logEntry = $"[{timestamp}] {content}\n"
                do! File.AppendAllTextAsync(path, logEntry) |> Async.AwaitTask
                return Ok ()
            with
            | ex -> return Error (FileError ex.Message)
        }

    // Logging functions
    let logInfo (message: string) =
        let logPath = Path.Combine("Data", "shop.log")
        appendToFile logPath $"INFO: {message}"

    let logError (message: string) =
        let logPath = Path.Combine("Data", "shop.log")
        appendToFile logPath $"ERROR: {message}"

    let logWarning (message: string) =
        let logPath = Path.Combine("Data", "shop.log")
        appendToFile logPath $"WARNING: {message}"

    // File listing and management
    let listFiles (directory: string) (pattern: string) =
        try
            if Directory.Exists(directory) then
                Directory.GetFiles(directory, pattern)
                |> Array.toList
                |> Ok
            else
                Ok []
        with
        | ex -> Error (FileError ex.Message)

    let getFileInfo (path: string) =
        try
            if File.Exists(path) then
                let info = FileInfo(path)
                Ok {|
                    FullName = info.FullName
                    Name = info.Name
                    Size = info.Length
                    Created = info.CreationTime
                    Modified = info.LastWriteTime
                |}
            else
                Error (FileError $"File not found: {path}")
        with
        | ex -> Error (FileError ex.Message)

    // Backup functionality
    let backupFile (sourcePath: string) =
        try
            if File.Exists(sourcePath) then
                let timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss")
                let backupPath = $"{sourcePath}.backup_{timestamp}"
                File.Copy(sourcePath, backupPath)
                Ok backupPath
            else
                Error (FileError $"Source file not found: {sourcePath}")
        with
        | ex -> Error (FileError ex.Message)