// 🚀 F# Simple Product Export Script
// Usage: dotnet fsi simple_export.fsx [output_filename]

#r "nuget: ClosedXML"
#r "nuget: Newtonsoft.Json"
#load "../Models/Domain.fs"
#load "../DataAccess/StringUtils.fs"
#load "../DataAccess/JsonHandler.fs"
#load "../DataAccess/ProductRepository.fs"

open System
open System.IO
open ClosedXML.Excel
open Shop.DataAccess
open Shop.Models


let exportProduct = async {
    let exportsDir = Path.Combine(__SOURCE_DIRECTORY__, "..", "Exports")
    if not (Directory.Exists exportsDir) then Directory.CreateDirectory exportsDir |> ignore
    let outputPath = Path.Combine(exportsDir, $"products_export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx")
    
    // Set correct working directory for ProductRepository to find Data/products.json
    let originalDir = Directory.GetCurrentDirectory()
    Directory.SetCurrentDirectory(Path.Combine(__SOURCE_DIRECTORY__, ".."))
    
    try
        // JsonHandler from DataAccess/JsonHandler.fs
        let! loadResult = ProductRepository.loadProductsAsync()
        
        match loadResult with
        | Ok (products: Product list) ->
            // Create a new Excel workbook
            use workbook = new XLWorkbook()
            let worksheet = workbook.Worksheets.Add("Products")     
            let header = ["Id"; "Name"; "Description"; "Price"; "Category"]

            // Add header row
            for colIndex, headerText in List.indexed header do
                worksheet.Cell(1, colIndex + 1).Value <- headerText
            // Add product rows
            for rowIndex, (product: Product) in List.indexed products do
                let (ProductId id) = product.Id
                worksheet.Cell(rowIndex + 2, 1).Value <- id
                worksheet.Cell(rowIndex + 2, 2).Value <- product.Name
                worksheet.Cell(rowIndex + 2, 3).Value <- product.Description
                worksheet.Cell(rowIndex + 2, 4).Value <- product.Price
                worksheet.Cell(rowIndex + 2, 5).Value <- product.Category
            // Save the workbook
            workbook.SaveAs(outputPath)
            printfn "✅ Exported %d products to %s" products.Length outputPath
        | Error err ->
            printfn "❌ Error loading products: %A" err
    finally
        Directory.SetCurrentDirectory(originalDir)
}

// Run the export
exportProduct |> Async.RunSynchronously
