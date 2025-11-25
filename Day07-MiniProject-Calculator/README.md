# 🧮 Day 7: Mini Project - Calculator

## 🎯 Project Objectives
- [ ] Combine all concepts from Days 1-6
- [ ] Create interactive CLI calculator application
- [ ] Implement error handling with Option/Result types
- [ ] Use pattern matching extensively  
- [ ] Practice function composition and pipelines
- [ ] Build a complete, working application

## 📋 Project Requirements

### Basic Calculator Features:
1. **Basic Operations**: +, -, *, /, ^(power)
2. **Advanced Functions**: sqrt, sin, cos, tan, log
3. **Memory Functions**: store, recall, clear
4. **History**: Keep track of calculations
5. **Error Handling**: Division by zero, invalid input
6. **Interactive CLI**: Menu-driven interface

### Technical Requirements:
1. Use discriminated unions for operations
2. Implement with Option/Result types for error handling
3. Use records for calculator state
4. Apply function composition and pipelines
5. Include unit conversion features

## 📝 Implementation Guide

### Step 1: Core Types
Create `CalculatorTypes.fs`:
```fsharp
// Operation types
type Operation =
    | Add of float * float
    | Subtract of float * float  
    | Multiply of float * float
    | Divide of float * float
    | Power of float * float
    | SquareRoot of float
    | Sin of float
    | Cos of float
    | Tan of float
    | Log of float

// Calculator state
type CalculatorMemory = {
    StoredValue: float option
    History: (string * float) list
}

// Result types
type CalculationResult =
    | Success of float
    | Error of string

type CalculatorState = {
    CurrentValue: float
    Memory: CalculatorMemory
    LastOperation: string option
}
```

### Step 2: Core Calculator Engine
Create `CalculatorEngine.fs`:
```fsharp
open CalculatorTypes

// Execute basic operations
let executeOperation operation =
    match operation with
    | Add (x, y) -> Success (x + y)
    | Subtract (x, y) -> Success (x - y)
    | Multiply (x, y) -> Success (x * y)
    | Divide (x, y) -> 
        if y <> 0.0 then Success (x / y)
        else Error "Division by zero"
    | Power (x, y) -> Success (x ** y)
    | SquareRoot x -> 
        if x >= 0.0 then Success (sqrt x)
        else Error "Cannot take square root of negative number"
    | Sin x -> Success (sin x)
    | Cos x -> Success (cos x)
    | Tan x -> Success (tan x)
    | Log x -> 
        if x > 0.0 then Success (log x)
        else Error "Logarithm undefined for non-positive numbers"

// Parse input to operation
let parseOperation input currentValue =
    match input with
    | ["+"; y] -> 
        match System.Double.TryParse(y) with
        | (true, num) -> Some (Add (currentValue, num))
        | _ -> None
    | ["-"; y] -> 
        match System.Double.TryParse(y) with
        | (true, num) -> Some (Subtract (currentValue, num))
        | _ -> None
    | ["*"; y] -> 
        match System.Double.TryParse(y) with
        | (true, num) -> Some (Multiply (currentValue, num))
        | _ -> None
    | ["/"; y] -> 
        match System.Double.TryParse(y) with
        | (true, num) -> Some (Divide (currentValue, num))
        | _ -> None
    | ["^"; y] -> 
        match System.Double.TryParse(y) with
        | (true, num) -> Some (Power (currentValue, num))
        | _ -> None
    | ["sqrt"] -> Some (SquareRoot currentValue)
    | ["sin"] -> Some (Sin currentValue)
    | ["cos"] -> Some (Cos currentValue)
    | ["tan"] -> Some (Tan currentValue)
    | ["log"] -> Some (Log currentValue)
    | _ -> None

// Update calculator state
let updateState state operation result operationStr =
    match result with
    | Success value ->
        let newHistory = (operationStr, value) :: state.Memory.History
        { state with 
            CurrentValue = value
            Memory = { state.Memory with History = newHistory }
            LastOperation = Some operationStr }
    | Error _ -> state
```

### Step 3: Memory Operations  
Create `CalculatorMemory.fs`:
```fsharp
open CalculatorTypes

// Memory operations
let storeValue value state =
    { state with Memory = { state.Memory with StoredValue = Some value } }

let recallValue state =
    match state.Memory.StoredValue with
    | Some value -> Success value
    | None -> Error "No value stored in memory"

let clearMemory state =
    { state with Memory = { StoredValue = None; History = state.Memory.History } }

let clearHistory state =
    { state with Memory = { state.Memory with History = [] } }

let showHistory state =
    state.Memory.History
    |> List.rev
    |> List.mapi (fun i (op, result) -> sprintf "%d. %s = %.2f" (i + 1) op result)
```

### Step 4: Unit Conversion
Create `UnitConversion.fs`:
```fsharp
type LengthUnit = Meter | Foot | Inch | Centimeter
type TemperatureUnit = Celsius | Fahrenheit | Kelvin
type WeightUnit = Kilogram | Pound | Gram

// Length conversions (to meters)
let convertToMeters value unit =
    match unit with
    | Meter -> value
    | Foot -> value * 0.3048
    | Inch -> value * 0.0254
    | Centimeter -> value * 0.01

// Convert from meters
let convertFromMeters value unit =
    match unit with
    | Meter -> value
    | Foot -> value / 0.3048
    | Inch -> value / 0.0254
    | Centimeter -> value / 0.01

// Temperature conversions
let convertTemperature value fromUnit toUnit =
    // Convert to Celsius first
    let celsius = 
        match fromUnit with
        | Celsius -> value
        | Fahrenheit -> (value - 32.0) * 5.0 / 9.0
        | Kelvin -> value - 273.15
    
    // Convert from Celsius to target
    match toUnit with
    | Celsius -> celsius
    | Fahrenheit -> celsius * 9.0 / 5.0 + 32.0
    | Kelvin -> celsius + 273.15
```

### Step 5: User Interface
Create `CalculatorUI.fs`:
```fsharp
open CalculatorTypes
open CalculatorEngine
open CalculatorMemory
open UnitConversion

let printMenu () =
    printfn """
╔══════════════════════════════════════╗
║            F# Calculator             ║
╠══════════════════════════════════════╣
║ Basic Operations:                    ║
║   + <num>  : Add                     ║
║   - <num>  : Subtract                ║
║   * <num>  : Multiply                ║
║   / <num>  : Divide                  ║
║   ^ <num>  : Power                   ║
║                                      ║
║ Functions:                           ║
║   sqrt     : Square Root             ║
║   sin      : Sine                    ║
║   cos      : Cosine                  ║
║   tan      : Tangent                 ║
║   log      : Natural Log             ║
║                                      ║
║ Memory:                              ║
║   ms       : Memory Store            ║
║   mr       : Memory Recall           ║
║   mc       : Memory Clear            ║
║   history  : Show History            ║
║                                      ║
║ Other:                               ║
║   clear    : Clear Current           ║
║   convert  : Unit Conversion         ║
║   exit     : Exit Calculator         ║
╚══════════════════════════════════════╝
"""

let rec calculatorLoop state =
    printfn "\nCurrent Value: %.2f" state.CurrentValue
    printf "Enter operation: "
    
    let input = System.Console.ReadLine().Split(' ') |> Array.toList
    
    match input with
    | ["exit"] -> 
        printfn "Thank you for using F# Calculator!"
    | ["clear"] ->
        let newState = { state with CurrentValue = 0.0 }
        calculatorLoop newState
    | ["ms"] ->
        let newState = storeValue state.CurrentValue state
        printfn "Value %.2f stored in memory" state.CurrentValue
        calculatorLoop newState
    | ["mr"] ->
        match recallValue state with
        | Success value ->
            let newState = { state with CurrentValue = value }
            printfn "Recalled: %.2f" value
            calculatorLoop newState
        | Error msg -> 
            printfn "Error: %s" msg
            calculatorLoop state
    | ["mc"] ->
        let newState = clearMemory state
        printfn "Memory cleared"
        calculatorLoop newState
    | ["history"] ->
        let historyLines = showHistory state
        if List.isEmpty historyLines then
            printfn "No calculations in history"
        else
            printfn "Calculation History:"
            historyLines |> List.iter (printfn "%s")
        calculatorLoop state
    | _ ->
        match parseOperation input state.CurrentValue with
        | Some operation ->
            let result = executeOperation operation
            match result with
            | Success value ->
                let operationStr = sprintf "%.2f %s" state.CurrentValue (String.concat " " input)
                let newState = updateState state operation result operationStr
                printfn "Result: %.2f" value
                calculatorLoop newState
            | Error msg ->
                printfn "Error: %s" msg
                calculatorLoop state
        | None ->
            printfn "Invalid operation. Type 'help' for available commands."
            calculatorLoop state

// Initialize and start calculator
let startCalculator () =
    let initialState = {
        CurrentValue = 0.0
        Memory = { StoredValue = None; History = [] }
        LastOperation = None
    }
    
    printfn "Welcome to F# Calculator!"
    printMenu ()
    calculatorLoop initialState
```

### Step 6: Program Entry Point
Create `Program.fs`:
```fsharp
open CalculatorUI

[<EntryPoint>]
let main argv =
    startCalculator ()
    0
```

## 🏃‍♂️ Practice Enhancements

### Enhancement 1: Scientific Calculator
Add support for:
- Factorial function
- Trigonometric functions in degrees
- Hyperbolic functions (sinh, cosh, tanh)
- Constants (π, e)

### Enhancement 2: Expression Parser
Implement:
- Parse complex expressions like "2 + 3 * 4"
- Handle parentheses for order of operations
- Support variables

### Enhancement 3: Graphing Functions
Add features to:
- Plot simple functions
- Generate data points
- Export to CSV for external graphing

### Enhancement 4: Configuration
Implement:
- Save/load calculator settings
- Precision settings
- Different number bases (binary, hex, octal)

## ✅ Completion Checklist
- [ ] Implemented all basic operations
- [ ] Added advanced mathematical functions
- [ ] Included memory operations
- [ ] Created interactive CLI interface
- [ ] Implemented proper error handling
- [ ] Added calculation history
- [ ] Included unit conversion features
- [ ] Tested all functionality thoroughly

## 🔍 Concepts Applied
- **Discriminated Unions**: Operation types
- **Records**: Calculator state management
- **Pattern Matching**: Command parsing and operation execution
- **Option/Result Types**: Error handling
- **Function Composition**: Chaining operations
- **Immutable State**: State transitions

## 🚀 Project Structure
```
Day7-MiniProject-Calculator/
├── CalculatorTypes.fs      # Type definitions
├── CalculatorEngine.fs     # Core calculation logic
├── CalculatorMemory.fs     # Memory operations
├── UnitConversion.fs       # Unit conversion functions
├── CalculatorUI.fs         # User interface
├── Program.fs              # Entry point
└── Calculator.fsproj       # Project file
```

## 🎯 Next Steps
Congratulations! You've built a complete F# application. Ready for **Day 8-9: Data Handling** to learn about file I/O, JSON, and async operations!