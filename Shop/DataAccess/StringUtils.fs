namespace Shop.DataAccess

module StringUtils =
    open System
    open System.Text.RegularExpressions
    open Shop.Models

    // Day 8: String processing functions
    let cleanText (text: string) =
        text.Trim().ToLower()

    let normalizeWhitespace (text: string) =
        Regex.Replace(text.Trim(), @"\s+", " ")

    let extractWords (text: string) =
        text.Split([|' '; '\t'; '\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
        |> Array.map cleanText
        |> Array.toList

    let wordCount (text: string) =
        extractWords text |> List.length

    let charCount (text: string) =
        text.Length

    let containsKeyword (keywords: string list) (text: string) =
        let cleanedText = cleanText text
        keywords 
        |> List.exists (fun keyword -> cleanedText.Contains(cleanText keyword))

    let highlightKeywords (keywords: string list) (text: string) =
        keywords
        |> List.fold (fun (acc: string) keyword ->
            acc.Replace(keyword, sprintf "**%s**" keyword, StringComparison.OrdinalIgnoreCase)
        ) text

    // Search functionality
    let fuzzyMatch (query: string) (target: string) =
        let queryWords = extractWords query      // Tách query thành words
        let targetWords = extractWords target    // Tách target thành words
        
        let matchCount = 
            queryWords
            |> List.sumBy (fun qWord ->              // Với mỗi word trong query
                targetWords
                |> List.filter (fun tWord -> tWord.Contains(qWord))  // Tìm target words chứa qWord
                |> List.length                       // Đếm số lượng matches
            )
        
        float matchCount / float queryWords.Length   // Tỷ lệ match (0.0 - 1.0)

    // Note: searchProducts has been moved to ProductRepository module