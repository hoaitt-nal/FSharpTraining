// 📊 F# Script: Export Products from JSON to Excel
// Usage: dotnet fsi export_product.fsx

#r "nuget: ClosedXML"
#r "nuget: Newtonsoft.Json"

open System
open System.IO
open ClosedXML.Excel
open Newtonsoft.Json

// 📝 Product Data Model
type Product = {
    Id: string
    Name: string
    Description: string
    Price: decimal
    Category: string
    Stock: int
    Tags: string[]
    CreatedAt: DateTime
}

// 🔧 Helper Functions
let readJsonFile<'T> (filePath: string) = 
    try
        let json = File.ReadAllText(filePath)
        JsonConvert.DeserializeObject<'T>(json) |> Ok
    with
    | ex -> Error $"Lỗi đọc file JSON: {ex.Message}"

let ensureDirectoryExists (filePath: string) =
    let directory = Path.GetDirectoryName(filePath)
    if not (String.IsNullOrEmpty(directory)) && not (Directory.Exists(directory)) then
        Directory.CreateDirectory(directory) |> ignore

// 📊 Excel Export Functions
let createProductExcel (products: Product[]) (outputPath: string) =
    try
        ensureDirectoryExists outputPath
        
        use workbook = new XLWorkbook()
        let worksheet = workbook.Worksheets.Add("Products")
        
        // 📋 Headers
        let headers = [
            "ID"; "Tên sản phẩm"; "Mô tả"; "Giá (USD)"; 
            "Danh mục"; "Tồn kho"; "Tags"; "Ngày tạo"
        ]
        
        // Set headers
        headers |> List.iteri (fun i header -> 
            let cell = worksheet.Cell(1, i + 1)
            cell.Value <- XLCellValue.FromObject(header)
            cell.Style.Font.Bold <- true
            cell.Style.Fill.BackgroundColor <- XLColor.LightBlue
        )
        
        // 📊 Data rows
        products |> Array.iteri (fun i product ->
            let row = i + 2
            worksheet.Cell(row, 1).Value <- XLCellValue.FromObject(product.Id)
            worksheet.Cell(row, 2).Value <- XLCellValue.FromObject(product.Name)
            worksheet.Cell(row, 3).Value <- XLCellValue.FromObject(product.Description)
            worksheet.Cell(row, 4).Value <- XLCellValue.FromObject(product.Price)
            worksheet.Cell(row, 5).Value <- XLCellValue.FromObject(product.Category)
            worksheet.Cell(row, 6).Value <- XLCellValue.FromObject(product.Stock)
            worksheet.Cell(row, 7).Value <- XLCellValue.FromObject(String.Join(", ", product.Tags))
            worksheet.Cell(row, 8).Value <- XLCellValue.FromObject(product.CreatedAt.ToString("dd/MM/yyyy HH:mm"))
        )
        
        // 🎨 Format columns
        worksheet.Column(4).Style.NumberFormat.Format <- "$#,##0.00"  // Price formatting
        worksheet.Column(6).Style.NumberFormat.Format <- "#,##0"      // Stock formatting  
        worksheet.Column(8).Style.NumberFormat.Format <- "dd/mm/yyyy" // Date formatting
        
        // Auto-fit columns
        worksheet.ColumnsUsed().AdjustToContents() |> ignore
        
        // Save file
        workbook.SaveAs(outputPath)
        Ok $"✅ Đã xuất {products.Length} sản phẩm vào file: {outputPath}"
        
    with
    | ex -> Error $"❌ Lỗi tạo file Excel: {ex.Message}"

// 📈 Advanced Excel with Summary Sheet
let createDetailedProductExcel (products: Product[]) (outputPath: string) =
    try
        ensureDirectoryExists outputPath
        
        use workbook = new XLWorkbook()
        
        // 📊 Main Products Sheet
        let productSheet = workbook.Worksheets.Add("Products")
        
        // Headers
        let headers = [
            "ID"; "Tên sản phẩm"; "Mô tả"; "Giá (USD)"; 
            "Danh mục"; "Tồn kho"; "Giá trị kho"; "Tags"; "Ngày tạo"
        ]
        
        headers |> List.iteri (fun i header -> 
            let cell = productSheet.Cell(1, i + 1)
            cell.Value <- XLCellValue.FromObject(header)
            cell.Style.Font.Bold <- true
            cell.Style.Fill.BackgroundColor <- XLColor.LightBlue
        )
        
        // Product data with calculated inventory value
        products |> Array.iteri (fun i product ->
            let row = i + 2
            let inventoryValue = product.Price * decimal product.Stock
            
            productSheet.Cell(row, 1).Value <- XLCellValue.FromObject(product.Id)
            productSheet.Cell(row, 2).Value <- XLCellValue.FromObject(product.Name)
            productSheet.Cell(row, 3).Value <- XLCellValue.FromObject(product.Description)
            productSheet.Cell(row, 4).Value <- XLCellValue.FromObject(product.Price)
            productSheet.Cell(row, 5).Value <- XLCellValue.FromObject(product.Category)
            productSheet.Cell(row, 6).Value <- XLCellValue.FromObject(product.Stock)
            productSheet.Cell(row, 7).Value <- XLCellValue.FromObject(inventoryValue)
            productSheet.Cell(row, 8).Value <- XLCellValue.FromObject(String.Join(", ", product.Tags))
            productSheet.Cell(row, 9).Value <- XLCellValue.FromObject(product.CreatedAt)
        )
        
        // Format product sheet
        productSheet.Column(4).Style.NumberFormat.Format <- "$#,##0.00"
        productSheet.Column(6).Style.NumberFormat.Format <- "#,##0"
        productSheet.Column(7).Style.NumberFormat.Format <- "$#,##0.00"
        productSheet.Column(9).Style.NumberFormat.Format <- "dd/mm/yyyy"
        productSheet.ColumnsUsed().AdjustToContents() |> ignore
        
        // 📈 Summary Sheet
        let summarySheet = workbook.Worksheets.Add("Summary")
        
        // Calculate statistics
        let totalProducts = products.Length
        let totalInventoryValue = products |> Array.sumBy (fun p -> p.Price * decimal p.Stock)
        let avgPrice = products |> Array.averageBy (fun p -> float p.Price)
        let totalStock = products |> Array.sumBy (fun p -> p.Stock)
        
        // Category analysis
        let categoryStats = 
            products 
            |> Array.groupBy (fun p -> p.Category)
            |> Array.map (fun (category, categoryProducts) ->
                let count = categoryProducts.Length
                let totalValue = categoryProducts |> Array.sumBy (fun p -> p.Price * decimal p.Stock)
                let avgPrice = categoryProducts |> Array.averageBy (fun p -> float p.Price)
                (category, count, totalValue, avgPrice))
            |> Array.sortByDescending (fun (_, _, totalValue, _) -> totalValue)
        
        // Summary headers and data
        summarySheet.Cell(1, 1).Value <- XLCellValue.FromObject("📊 THỐNG KÊ SẢN PHẨM")
        summarySheet.Cell(1, 1).Style.Font.Bold <- true
        summarySheet.Cell(1, 1).Style.Font.FontSize <- 16
        
        summarySheet.Cell(3, 1).Value <- XLCellValue.FromObject("Tổng số sản phẩm:")
        summarySheet.Cell(3, 2).Value <- XLCellValue.FromObject(totalProducts)
        
        summarySheet.Cell(4, 1).Value <- XLCellValue.FromObject("Tổng giá trị kho:")
        summarySheet.Cell(4, 2).Value <- XLCellValue.FromObject(totalInventoryValue)
        summarySheet.Cell(4, 2).Style.NumberFormat.Format <- "$#,##0.00"
        
        summarySheet.Cell(5, 1).Value <- XLCellValue.FromObject("Giá trung bình:")
        summarySheet.Cell(5, 2).Value <- XLCellValue.FromObject(avgPrice)
        summarySheet.Cell(5, 2).Style.NumberFormat.Format <- "$#,##0.00"
        
        summarySheet.Cell(6, 1).Value <- XLCellValue.FromObject("Tổng tồn kho:")
        summarySheet.Cell(6, 2).Value <- XLCellValue.FromObject(totalStock)
        
        // Category breakdown
        summarySheet.Cell(8, 1).Value <- XLCellValue.FromObject("📋 THỐNG KÊ THEO DANH MỤC")
        summarySheet.Cell(8, 1).Style.Font.Bold <- true
        
        let categoryHeaders = ["Danh mục"; "Số lượng"; "Giá trị"; "Giá TB"]
        categoryHeaders |> List.iteri (fun i header ->
            summarySheet.Cell(9, i + 1).Value <- XLCellValue.FromObject(header)
            summarySheet.Cell(9, i + 1).Style.Font.Bold <- true
            summarySheet.Cell(9, i + 1).Style.Fill.BackgroundColor <- XLColor.LightGray
        )
        
        categoryStats |> Array.iteri (fun i (category, count, totalValue, avgPrice) ->
            let row = i + 10
            summarySheet.Cell(row, 1).Value <- XLCellValue.FromObject(category)
            summarySheet.Cell(row, 2).Value <- XLCellValue.FromObject(count)
            summarySheet.Cell(row, 3).Value <- XLCellValue.FromObject(totalValue)
            summarySheet.Cell(row, 4).Value <- XLCellValue.FromObject(avgPrice)
        )
        
        // Format summary sheet
        summarySheet.Column(3).Style.NumberFormat.Format <- "$#,##0.00"
        summarySheet.Column(4).Style.NumberFormat.Format <- "$#,##0.00"
        summarySheet.ColumnsUsed().AdjustToContents() |> ignore
        
        workbook.SaveAs(outputPath)
        Ok $"✅ Đã xuất {products.Length} sản phẩm với thống kê vào file: {outputPath}"
        
    with
    | ex -> Error $"❌ Lỗi tạo file Excel chi tiết: {ex.Message}"

// 🚀 Main Export Function
let exportProducts (inputPath: string) (outputPath: string) (detailed: bool) =
    printfn "🚀 Bắt đầu export sản phẩm..."
    printfn "📂 Input: %s" inputPath
    printfn "📄 Output: %s" outputPath
    
    match readJsonFile<Product[]> inputPath with
    | Error err -> 
        printfn "%s" err
        1
    | Ok products -> 
        printfn "✅ Đã đọc %d sản phẩm từ JSON" products.Length
        
        let exportResult = 
            if detailed then 
                createDetailedProductExcel products outputPath
            else 
                createProductExcel products outputPath
        
        match exportResult with
        | Ok message -> 
            printfn "%s" message
            printfn "🎉 Export hoàn thành!"
            0
        | Error err -> 
            printfn "%s" err
            1

// 📁 File paths
let currentDir = __SOURCE_DIRECTORY__
let inputFile = Path.Combine(currentDir, "..", "Shop", "Data", "products.json")
let outputDir = Path.Combine(currentDir, "..", "exports")
let outputFile = Path.Combine(outputDir, $"products_export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx")
let detailedOutputFile = Path.Combine(outputDir, $"products_detailed_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx")

// 🎯 Execute Export
printfn "🛍️  F# PRODUCT EXPORT TOOL"
printfn "%s" (String.replicate 40 "=")

// Basic export
printfn "\n📊 Xuất file Excel cơ bản..."
let basicResult = exportProducts inputFile outputFile false

// Detailed export with statistics
printfn "\n📈 Xuất file Excel chi tiết với thống kê..."
let detailedResult = exportProducts inputFile detailedOutputFile true

// Summary
printfn "\n📋 KẾT QUẢ:"
if basicResult = 0 then printfn "✅ Export cơ bản: Thành công"
else printfn "❌ Export cơ bản: Thất bại"

if detailedResult = 0 then printfn "✅ Export chi tiết: Thành công"  
else printfn "❌ Export chi tiết: Thất bại"

printfn "\n🎉 Hoàn thành!"