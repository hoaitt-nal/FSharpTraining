#I "."
#load "packageHelper.fsx"

open System
open System.IO
open System.Linq
open DotNetEnv
open System.Text
open Microsoft.Extensions.Configuration
open System.Text.Json

module CommonHelper =

    type BackupData =
        { RecordType: string
          BackupData: string }

    // let configuration =
    //     printfn "Current directory: %s" (Directory.GetCurrentDirectory())

    //     ConfigurationBuilder()
    //         .SetBasePath(Directory.GetCurrentDirectory())
    //         .AddJsonFile("serilog.json", false, true)
    //         .AddEnvironmentVariables()

    // let config = configuration.Build()
    // Log.Logger <- LoggerConfiguration().ReadFrom.Configuration(config).CreateLogger()

    type OrElseBuilder() =
        member this.ReturnFrom(x) = x

        member this.Combine(a, b) =
            match a with
            | Some _ -> a // a succeeds -- use it
            | None -> b // a fails -- use b instead

        member this.Delay(f) = f ()

    let rec FindAncestorPath file basePath =
        let di = DirectoryInfo(basePath)
        let fi = di.GetFiles(file).FirstOrDefault()

        match fi with
        | null ->
            match Directory.GetDirectoryRoot(basePath) with
            | name when name = di.FullName -> ""
            | _ -> FindAncestorPath file di.Parent.FullName
        | _ -> fi.FullName

    let envFileForDev = FindAncestorPath ".env" (Directory.GetCurrentDirectory())

    match envFileForDev with
    | x when String.IsNullOrWhiteSpace(x) -> ()
    | x -> Env.Load(x) |> ignore

    let toJsonFile (fileName: string) (baseDirectory: string) (data: string) =
        let t = DateTime.Now.ToString("yyyyMMdd-HHmmss")
        let filePath = Path.Combine(baseDirectory, $"{fileName}.{t}.json")
        Directory.CreateDirectory(baseDirectory) |> ignore
        File.WriteAllText(filePath, data, Encoding.UTF8)

    let fromJsonFile (fileName: string) (baseDirectory: string) =
        let filePath = Path.Combine($"{baseDirectory}", $"{fileName}")
        File.ReadAllText(filePath)

    let readJsonFile<'T> (filePath: string) =
        async {
            let! content = File.ReadAllTextAsync(filePath) |> Async.AwaitTask
            return JsonSerializer.Deserialize<'T>(content)
        }
