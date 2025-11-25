namespace Calculator

module CalculatorEngine =
    open System

    // Basic Calculator Functions
    let calculate operation x y =
        match operation with
        | Add -> Some (x + y)
        | Subtract -> Some (x - y)
        | Multiply -> Some (x * y)
        | Divide when y <> 0.0 -> Some (x / y)
        | Divide -> None // Division by zero
        | Power -> Some (Math.Pow(x, y))
        | SquareRoot -> None // Single operand operation

    let calculateSingleOperand operation x =
        match operation with
        | SquareRoot when x >= 0.0 -> Some (Math.Sqrt(x))
        | SquareRoot -> None // Negative square root
        | _ -> None

    // Unit Conversion Functions
    let convertTemperature fromUnit toUnit value =
        // Convert to Celsius first, then to target unit
        let toCelsius = function
            | Celsius -> value
            | Fahrenheit -> (value - 32.0) * 5.0 / 9.0
            | Kelvin -> value - 273.15

        let fromCelsius celsius = function
            | Celsius -> celsius
            | Fahrenheit -> celsius * 9.0 / 5.0 + 32.0
            | Kelvin -> celsius + 273.15

        let celsius = toCelsius fromUnit
        fromCelsius celsius toUnit

    let convertDistance fromUnit toUnit value =
        // Convert to meters first, then to target unit
        let toMeters = function
            | Meter -> value
            | Kilometer -> value * 1000.0
            | Foot -> value * 0.3048
            | Mile -> value * 1609.344
            | Inch -> value * 0.0254

        let fromMeters meters = function
            | Meter -> meters
            | Kilometer -> meters / 1000.0
            | Foot -> meters / 0.3048
            | Mile -> meters / 1609.344
            | Inch -> meters / 0.0254

        let meters = toMeters fromUnit
        fromMeters meters toUnit

    let convertWeight fromUnit toUnit value =
        // Convert to grams first, then to target unit
        let toGrams = function
            | Gram -> value
            | Kilogram -> value * 1000.0
            | Pound -> value * 453.592
            | Ounce -> value * 28.3495

        let fromGrams grams = function
            | Gram -> grams
            | Kilogram -> grams / 1000.0
            | Pound -> grams / 453.592
            | Ounce -> grams / 28.3495

        let grams = toGrams fromUnit
        fromGrams grams toUnit

    // Main conversion function
    let convert conversionType value =
        match conversionType with
        | Temperature (fromUnit, toUnit) -> convertTemperature fromUnit toUnit value
        | Distance (fromUnit, toUnit) -> convertDistance fromUnit toUnit value
        | Weight (fromUnit, toUnit) -> convertWeight fromUnit toUnit value