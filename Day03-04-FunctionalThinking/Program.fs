printfn "============Practice Task 1: Math Library============="
// Task 1: Power function (recursive)
let rec power baseNum exp =
    match exp with
    | 0 -> 1
    | 1 -> baseNum
    | n when n > 0 -> baseNum * power baseNum (n - 1)
    | _ -> failwith "Negative exponents not supported"

// Task 1.2: GCD (Greatest Common Divisor) using Euclidean algorithm
let rec gcd a b =
    if b = 0 then a
    else gcd b (a % b)

// Task 1.3: Check if number is prime
let isPrime n =
    if n < 2 then false
    elif n = 2 then true
    elif n % 2 = 0 then false
    else
        let limit = int (sqrt (float n))
        [3..2..limit]
        |> List.forall (fun i -> n % i <> 0)

// Task 1.4: Generate list of primes up to N
let generatePrimes n =
    [2..n]
    |> List.filter isPrime

// Test Math Library
printfn "Power 2^8: %d" (power 2 8) // Should be 256
// Breakpoint here for debugging
// baseNum = 2, exp = 8
// 2 * power 2 7
// 2 * (2 * power 2 6)
// 2 * (2 * (2 * power 2 5))
// ...
printfn "Power 2^0: %d" (power 2 0) // returns 1
printfn "Power 2^1 %d" (power 2 1) // returns 2
printfn "GCD of 48 and 18: %d" (gcd 48 18) // Should be 6
printfn "Is 17 prime? %b" (isPrime 17) // Should be true
printfn "Is 15 prime? %b" (isPrime 15) // Should be false
printfn "Primes up to 20: %A" (generatePrimes 20) // Should be [2; 3; 5; 7; 11; 13; 17; 19]

printfn "============Practice Task 2: List Processing============="
// Task 2.1: Find maximum element using recursion
let rec findMax lst =
    match lst with
    | [] -> failwith "Empty list has no maximum"
    | [ x ] -> x
    | head :: tail ->
        let maxTail = findMax tail
        if head > maxTail then head else maxTail

// Task 2.2: Reverse a list using recursion
let rec reverseList lst =
    match lst with
    | [] -> []
    | head :: tail -> reverseList tail @ [head]

// Task 2.3: Remove duplicates from list
let removeDuplicates lst =
    let rec removeDupsHelper acc remaining =
        match remaining with
        | [] -> List.rev acc
        | head :: tail ->
            if List.contains head acc then
                removeDupsHelper acc tail
            else
                removeDupsHelper (head :: acc) tail
    removeDupsHelper [] lst

// Task 2.4: Implement custom map and filter functions
let rec customMap f lst =
    match lst with
    | [] -> []
    | head :: tail -> f head :: customMap f tail

let rec customFilter predicate lst =
    match lst with
    | [] -> []
    | head :: tail ->
        if predicate head then
            head :: customFilter predicate tail
        else
            customFilter predicate tail

// Test List Processing
let testList = [ 5; 2; 8; 1; 9; 3; 7 ]
printfn "Original list: %A" testList
printfn "Maximum: %d" (findMax testList) // Should be 9
printfn "Reversed list: %A" (reverseList testList) // Should be [7; 3; 9; 1; 8; 2; 5]

let listWithDups = [1; 2; 2; 3; 1; 4; 3; 5]
printfn "List with duplicates: %A" listWithDups
printfn "After removing duplicates: %A" (removeDuplicates listWithDups) // Should be [1; 2; 3; 4; 5]

printfn "Custom map (*2): %A" (customMap (fun x -> x * 2) testList)
printfn "Custom filter (>5): %A" (customFilter (fun x -> x > 5) testList)
// Breakpoint here for debugging
// testList = [5; 2; 8; 1; 9; 3; 7]
// head = 5, tail = [2; 8; 1; 9; 3; 7]
// findMax [2; 8; 1; 9; 3; 7]
// head = 2, tail = [8; 1; 9; 3; 7]
// head = 8, tail = [1; 9; 3; 7]
// head = 1, tail = [9; 3; 7]
// head = 9, tail = [3; 7]
// head = 3, tail = [7]
// head = 7, tail = []
// returns 7
// back to previous call:
// 3 > 7 -> false -> 7
// 9 > 7 -> true -> 9
// 1 > 9 -> false -> 9
// 8 > 9 -> false -> 9
// 2 > 9 -> false -> 9
// 5 > 9 -> false -> 9
// returns 9

printfn "============Practice Task 3: String Processing Pipeline============="
// Task 3: Text processing pipeline
let processText (sentence: string) =
    sentence
    |> (fun (s: string) -> s.Split(' ')) // Split into words
    |> Array.filter (fun (word: string) -> word.Length >= 3) // Filter out short words
    |> Array.map (fun (word: string) -> word.ToUpper()) // Convert to uppercase
    |> String.concat " | " // Join with separator

// Test String Processing Pipeline
let testSentence =
    "F# is a great functional programming language for data processing"

printfn "Original sentence: %s" testSentence
printfn "Processed: %s" (processText testSentence)
// Should filter out words < 3 chars and convert to uppercase with | separator

printfn "============Practice Task 4: Advanced Pattern Matching Calculator============="
// Task 4: Calculator with pattern matching
type Operation =
    | Add of float * float
    | Subtract of float * float
    | Multiply of float * float
    | Divide of float * float
    | Invalid

let parseOperation (input: string) =
    let parts = input.Split(' ')

    if parts.Length = 3 then
        let x = parts.[0]
        let op = parts.[1]
        let y = parts.[2]
        match System.Double.TryParse(x), op, System.Double.TryParse(y) with
        | (true, x), "+", (true, y) -> Add(x, y)
        | (true, x), "-", (true, y) -> Subtract(x, y)
        | (true, x), "*", (true, y) -> Multiply(x, y)
        | (true, x), "/", (true, y) -> Divide(x, y)
        | _ -> Invalid
    else
        Invalid

let calculate operation =
    match operation with
    | Add(x, y) -> Some(x + y)
    | Subtract(x, y) -> Some(x - y)
    | Multiply(x, y) -> Some(x * y)
    | Divide(x, y) -> if y <> 0.0 then Some(x / y) else None // Division by zero
    | Invalid -> None

let processCalculation input = input |> parseOperation |> calculate

// Chain multiple operations
let chainCalculations inputs =
    inputs 
    |> List.map processCalculation 
    |> List.choose (fun x -> x)

// Test Advanced Pattern Matching Calculator
let calcTests =
    [ "10 + 5" // Should be Some 15.0
      "20 - 8" // Should be Some 12.0
      "6 * 7" // Should be Some 42.0
      "15 / 3" // Should be Some 5.0
      "10 / 0" // Should be None (division by zero)
      "10 any 5" ] // Should be None (invalid format)

printfn "Calculator Tests:"

calcTests
|> List.iter (fun test ->
    match processCalculation test with
    | Some result -> printfn "%s = %.2f" test result
    | None -> printfn "%s = Error (invalid operation or division by zero)" test)

let validResults = chainCalculations calcTests
printfn "All valid calculation results: %A" validResults
