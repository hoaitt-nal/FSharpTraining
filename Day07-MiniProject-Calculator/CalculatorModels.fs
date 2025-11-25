namespace Calculator

// Models cho Calculator
type Operation =
    | Add | Subtract | Multiply | Divide | Power | SquareRoot

type TemperatureUnit =
    | Celsius | Fahrenheit | Kelvin

type DistanceUnit = 
    | Meter | Kilometer | Foot | Mile | Inch

type WeightUnit =
    | Kilogram | Pound | Gram | Ounce

type ConversionType =
    | Temperature of TemperatureUnit * TemperatureUnit
    | Distance of DistanceUnit * DistanceUnit  
    | Weight of WeightUnit * WeightUnit

type MenuChoice =
    | Calculator
    | UnitConverter
    | Exit