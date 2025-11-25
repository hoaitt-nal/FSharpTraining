namespace Calculator

module UserInterface =
    open System
    open CalculatorEngine

    // Display Functions
    let printHeader () =
        printfn "╔═══════════════════════════════════════╗"
        printfn "║         F# Calculator & Converter      ║"
        printfn "║              Version 1.0               ║"
        printfn "╚═══════════════════════════════════════╝"
        printfn ""

    let printMainMenu () =
        printfn "┌─────────────────────────────────────┐"
        printfn "│              MAIN MENU              │"
        printfn "├─────────────────────────────────────┤"
        printfn "│ 1. Calculator                       │"
        printfn "│ 2. Unit Converter                   │"
        printfn "│ 3. Exit                             │"
        printfn "└─────────────────────────────────────┘"
        printf "Select option (1-3): "

    let printCalculatorMenu () =
        printfn "┌─────────────────────────────────────┐"
        printfn "│            CALCULATOR               │"
        printfn "├─────────────────────────────────────┤"
        printfn "│ Operations:                         │"
        printfn "│ + Addition                          │"
        printfn "│ - Subtraction                       │"  
        printfn "│ * Multiplication                    │"
        printfn "│ / Division                          │"
        printfn "│ ^ Power                             │"
        printfn "│ sqrt Square Root                    │"
        printfn "│ q Back to main menu                 │"
        printfn "└─────────────────────────────────────┘"

    let printConverterMenu () =
        printfn "┌─────────────────────────────────────┐"
        printfn "│           UNIT CONVERTER            │"
        printfn "├─────────────────────────────────────┤"
        printfn "│ 1. Temperature (°C, °F, K)          │"
        printfn "│ 2. Distance (m, km, ft, mi, in)     │"
        printfn "│ 3. Weight (kg, lb, g, oz)           │"
        printfn "│ 4. Back to main menu                │"
        printfn "└─────────────────────────────────────┘"
        printf "Select conversion type (1-4): "

    // Input Helper Functions
    let tryParseDouble (input: string) =
        match Double.TryParse(input) with
        | true, value -> Some value
        | false, _ -> None

    let getNumberInput prompt =
        let rec loop () =
            printf "%s" prompt
            let input = Console.ReadLine()
            match tryParseDouble input with
            | Some value -> value
            | None -> 
                printfn "Invalid number. Please try again."
                loop ()
        loop ()

    let getOperationInput () =
        let rec loop () =
            printf "Enter operation (+, -, *, /, ^, sqrt, q): "
            let input = Console.ReadLine().Trim().ToLower()
            match input with
            | "+" -> Some Add
            | "-" -> Some Subtract
            | "*" -> Some Multiply
            | "/" -> Some Divide
            | "^" -> Some Power
            | "sqrt" -> Some SquareRoot
            | "q" -> None
            | _ -> 
                printfn "Invalid operation. Please try again."
                loop ()
        loop ()

    // Calculator Interface
    let runCalculator () =
        printCalculatorMenu ()
        
        let rec calculatorLoop () =
            match getOperationInput () with
            | None -> () // Return to main menu
            | Some SquareRoot ->
                let x = getNumberInput "Enter number: "
                match calculateSingleOperand SquareRoot x with
                | Some result -> 
                    printfn "√%.2f = %.6f" x result
                    printfn ""
                | None -> 
                    printfn "Error: Cannot calculate square root of negative number."
                    printfn ""
                calculatorLoop ()
            | Some operation ->
                let x = getNumberInput "Enter first number: "
                let y = getNumberInput "Enter second number: "
                match calculate operation x y with
                | Some result ->
                    let operatorSymbol = 
                        match operation with
                        | Add -> "+"
                        | Subtract -> "-"
                        | Multiply -> "*"
                        | Divide -> "/"
                        | Power -> "^"
                        | _ -> "?"
                    printfn "%.2f %s %.2f = %.6f" x operatorSymbol y result
                    printfn ""
                | None ->
                    printfn "Error: Division by zero or invalid operation."
                    printfn ""
                calculatorLoop ()
        
        calculatorLoop ()

    // Temperature Converter
    let runTemperatureConverter () =
        printfn "Temperature Converter"
        printfn "Units: 1=Celsius, 2=Fahrenheit, 3=Kelvin"
        
        let getTemperatureUnit prompt =
            let rec loop () =
                printf "%s" prompt
                match Console.ReadLine() with
                | "1" -> Celsius
                | "2" -> Fahrenheit  
                | "3" -> Kelvin
                | _ -> 
                    printfn "Invalid choice. Please enter 1, 2, or 3."
                    loop ()
            loop ()

        let fromUnit = getTemperatureUnit "From unit (1-3): "
        let toUnit = getTemperatureUnit "To unit (1-3): "
        let value = getNumberInput "Enter temperature value: "
        
        let result = convert (Temperature (fromUnit, toUnit)) value
        let unitName = function
            | Celsius -> "°C"
            | Fahrenheit -> "°F"
            | Kelvin -> "K"
            
        printfn "%.2f%s = %.2f%s" value (unitName fromUnit) result (unitName toUnit)
        printfn ""

    // Distance Converter
    let runDistanceConverter () =
        printfn "Distance Converter"
        printfn "Units: 1=Meter, 2=Kilometer, 3=Foot, 4=Mile, 5=Inch"
        
        let getDistanceUnit prompt =
            let rec loop () =
                printf "%s" prompt
                match Console.ReadLine() with
                | "1" -> Meter
                | "2" -> Kilometer
                | "3" -> Foot
                | "4" -> Mile
                | "5" -> Inch
                | _ -> 
                    printfn "Invalid choice. Please enter 1-5."
                    loop ()
            loop ()

        let fromUnit = getDistanceUnit "From unit (1-5): "
        let toUnit = getDistanceUnit "To unit (1-5): "
        let value = getNumberInput "Enter distance value: "
        
        let result = convert (Distance (fromUnit, toUnit)) value
        let unitName = function
            | Meter -> "m"
            | Kilometer -> "km"
            | Foot -> "ft"
            | Mile -> "mi"
            | Inch -> "in"
            
        printfn "%.2f%s = %.6f%s" value (unitName fromUnit) result (unitName toUnit)
        printfn ""

    // Weight Converter
    let runWeightConverter () =
        printfn "Weight Converter"
        printfn "Units: 1=Kilogram, 2=Pound, 3=Gram, 4=Ounce"
        
        let getWeightUnit prompt =
            let rec loop () =
                printf "%s" prompt
                match Console.ReadLine() with
                | "1" -> Kilogram
                | "2" -> Pound
                | "3" -> Gram
                | "4" -> Ounce
                | _ -> 
                    printfn "Invalid choice. Please enter 1-4."
                    loop ()
            loop ()

        let fromUnit = getWeightUnit "From unit (1-4): "
        let toUnit = getWeightUnit "To unit (1-4): "
        let value = getNumberInput "Enter weight value: "
        
        let result = convert (Weight (fromUnit, toUnit)) value
        let unitName = function
            | Kilogram -> "kg"
            | Pound -> "lb"
            | Gram -> "g"
            | Ounce -> "oz"
            
        printfn "%.2f%s = %.6f%s" value (unitName fromUnit) result (unitName toUnit)
        printfn ""

    // Unit Converter Interface
    let runUnitConverter () =
        let rec converterLoop () =
            printConverterMenu ()
            match Console.ReadLine() with
            | "1" -> 
                runTemperatureConverter ()
                converterLoop ()
            | "2" -> 
                runDistanceConverter ()
                converterLoop ()
            | "3" -> 
                runWeightConverter ()
                converterLoop ()
            | "4" -> () // Return to main menu
            | _ -> 
                printfn "Invalid choice. Please try again."
                converterLoop ()
        
        converterLoop ()

    // Main Application Loop
    let runApplication () =
        printHeader ()
        
        let rec mainLoop () =
            printMainMenu ()
            match Console.ReadLine() with
            | "1" -> 
                runCalculator ()
                mainLoop ()
            | "2" -> 
                runUnitConverter ()
                mainLoop ()
            | "3" -> 
                printfn "Thank you for using F# Calculator & Converter!"
                ()
            | _ -> 
                printfn "Invalid choice. Please try again."
                mainLoop ()
        
        mainLoop ()