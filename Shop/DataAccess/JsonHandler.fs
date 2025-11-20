namespace Shop.DataAccess

module JsonHandler =
    open System.Text.Json
    open Shop.Models
    
    // Default JSON serialization options
    let private defaultJsonOptions = 
        let options = JsonSerializerOptions()
        options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        options.WriteIndented <- true
        options
    
    // Generic JSON file operations with async
    let loadFromJsonFileAsync<'T> (filePath: string) (options: JsonSerializerOptions option) : Async<Result<'T, ShopError>> =
        async {
            try
                if not (System.IO.File.Exists filePath) then
                    return Error (FileError $"File not found: {filePath}")
                else
                    let! jsonContent = System.IO.File.ReadAllTextAsync(filePath) |> Async.AwaitTask
                    let opts = options |> Option.defaultValue defaultJsonOptions
                    let result = JsonSerializer.Deserialize<'T>(jsonContent, opts)
                    return Ok result
            with
            | :? JsonException as ex ->
                return Error (JsonError $"JSON parsing error: {ex.Message}")
            | :? System.IO.IOException as ex ->
                return Error (FileError $"File I/O error: {ex.Message}")
            | ex ->
                return Error (FileError $"Unexpected error: {ex.Message}")
        }
    
    let saveToJsonFileAsync<'T> (filePath: string) (data: 'T) (options: JsonSerializerOptions option) : Async<Result<unit, ShopError>> =
        async {
            try
                let opts = options |> Option.defaultValue defaultJsonOptions
                let jsonContent = JsonSerializer.Serialize(data, opts)
                
                // Ensure directory exists
                let directory = System.IO.Path.GetDirectoryName(filePath)
                if not (System.String.IsNullOrEmpty(directory)) && not (System.IO.Directory.Exists(directory)) then
                    System.IO.Directory.CreateDirectory(directory) |> ignore
                
                do! System.IO.File.WriteAllTextAsync(filePath, jsonContent) |> Async.AwaitTask
                return Ok ()
            with
            | :? System.IO.IOException as ex ->
                return Error (FileError $"File I/O error: {ex.Message}")
            | ex ->
                return Error (FileError $"Unexpected error: {ex.Message}")
        }
