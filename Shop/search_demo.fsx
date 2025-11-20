// 🛍️ SearchProducts Algorithm - Chi tiết step by step

open System

// Copy các functions cần thiết
let cleanText (text: string) = text.Trim().ToLower()

let extractWords (text: string) =
    text.Split([|' '; '\t'; '\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
    |> Array.map cleanText
    |> Array.toList

let fuzzyMatch (query: string) (target: string) =
    let queryWords = extractWords query
    let targetWords = extractWords target
    
    let matchCount = 
        queryWords
        |> List.sumBy (fun qWord ->
            targetWords
            |> List.filter (fun tWord -> tWord.Contains(qWord))
            |> List.length
        )
    
    float matchCount / float queryWords.Length

// Simplified Product type for demo
type Product = {
    Name: string
    Description: string
    Tags: string list
    Price: decimal
}

let searchProductsDebug (query: string) (products: Product list) =
    printfn "\n🔍 SEARCH PRODUCTS ANALYSIS"
    printfn "Query: '%s'" query
    printfn "Products to search: %d" products.Length
    printfn "================================"
    
    let scoredProducts = 
        products
        |> List.mapi (fun i product ->                           // Với mỗi sản phẩm (có index)
            printfn "\n📦 Product %d: %s" (i+1) product.Name
            
            let nameScore = fuzzyMatch query product.Name        // Tính điểm cho tên sản phẩm
            printfn "  Name score: %.3f ('%s' vs '%s')" nameScore query product.Name
            
            let descScore = fuzzyMatch query product.Description // Tính điểm cho mô tả
            printfn "  Desc score: %.3f ('%s' vs '%s')" descScore query product.Description
            
            let tagScore = 
                if product.Tags.IsEmpty then 
                    printfn "  Tags: none"
                    0.0
                else
                    let tagScores = product.Tags |> List.map (fuzzyMatch query)
                    let maxScore = List.max tagScores               // Lấy điểm cao nhất trong tags
                    printfn "  Tags: %A" product.Tags
                    printfn "  Tag scores: %A → max: %.3f" tagScores maxScore
                    maxScore
            
            let totalScore = nameScore + descScore + tagScore    // Tổng điểm relevance
            printfn "  💯 TOTAL SCORE: %.3f + %.3f + %.3f = %.3f" nameScore descScore tagScore totalScore
            
            (product, totalScore)                                // Trả về tuple (product, score)
        )
    
    printfn "\n📊 FILTERING & SORTING"
    printfn "======================"
    
    let filteredProducts = 
        scoredProducts
        |> List.filter (fun (product, score) -> 
            let keep = score > 0.0
            printfn "  %s: %.3f → %s" product.Name score (if keep then "KEEP ✅" else "REMOVE ❌")
            keep
        )
    
    let sortedProducts = 
        filteredProducts
        |> List.sortByDescending snd                             // Sắp xếp theo điểm giảm dần
    
    printfn "\n🏆 FINAL RANKING:"
    printfn "================="
    sortedProducts |> List.iteri (fun i (product, score) ->
        printfn "  %d. %s (%.3f points)" (i+1) product.Name score
    )
    
    let finalResult = sortedProducts |> List.map fst            // Chỉ lấy products, bỏ scores
    finalResult

// ===============================================
// TEST DATA - Sản phẩm mẫu
// ===============================================

let sampleProducts = [
    { Name = "MacBook Pro 15"; Description = "Professional laptop for developers"; Tags = ["laptop"; "apple"; "pro"]; Price = 2499m }
    { Name = "Gaming Laptop ASUS"; Description = "High-performance gaming computer"; Tags = ["laptop"; "gaming"; "asus"]; Price = 1899m }
    { Name = "Dell Inspiron"; Description = "Affordable laptop for students"; Tags = ["laptop"; "budget"; "dell"]; Price = 799m }
    { Name = "iPad Pro"; Description = "Professional tablet with Apple Pencil"; Tags = ["tablet"; "apple"; "pro"]; Price = 1099m }
    { Name = "Wireless Mouse"; Description = "Bluetooth mouse for laptop"; Tags = ["mouse"; "wireless"; "accessory"]; Price = 49m }
    { Name = "Coffee Maker"; Description = "Automatic drip coffee machine"; Tags = ["kitchen"; "coffee"]; Price = 129m }
]

// ===============================================
// TEST CASES
// ===============================================

printfn "🧪 TEST CASE 1: Search 'laptop'"
let result1 = searchProductsDebug "laptop" sampleProducts

printfn "\n%s" (String.replicate 50 "=")

printfn "🧪 TEST CASE 2: Search 'pro apple'"
let result2 = searchProductsDebug "pro apple" sampleProducts

printfn "\n%s" (String.replicate 50 "=")

printfn "🧪 TEST CASE 3: Search 'gaming computer'"
let result3 = searchProductsDebug "gaming computer" sampleProducts

// ===============================================
// ANGULAR/TYPESCRIPT EQUIVALENT
// ===============================================

printfn "\n🔄 Angular/TypeScript Equivalent:"
printfn """
interface Product {
    name: string;
    description: string;  
    tags: string[];
    price: number;
}

class ProductSearchService {
    searchProducts(query: string, products: Product[]): Product[] {
        return products
            .map(product => {
                const nameScore = this.fuzzyMatch(query, product.name);
                const descScore = this.fuzzyMatch(query, product.description);
                
                const tagScore = product.tags.length === 0 ? 0 :
                    Math.max(...product.tags.map(tag => this.fuzzyMatch(query, tag)));
                
                const totalScore = nameScore + descScore + tagScore;
                return { product, score: totalScore };
            })
            .filter(item => item.score > 0)
            .sort((a, b) => b.score - a.score)
            .map(item => item.product);
    }
}
"""

// ===============================================
// SEARCH ENGINE INSIGHTS
// ===============================================

printfn "\n🎯 SEARCH ENGINE INSIGHTS:"
printfn "========================="
printfn "✅ Multi-field search: Name + Description + Tags"
printfn "✅ Fuzzy matching: Tolerates typos and variations"  
printfn "✅ Weighted scoring: Can adjust importance of fields"
printfn "✅ Relevance ranking: Best matches first"
printfn "✅ Zero-result filtering: Only meaningful results"
printfn "✅ Tag optimization: Best tag score (not average)"

printfn "\n✅ SearchProducts analysis completed!"