# 🚀 Day 1-2: Getting Started with F#

## 📋 Learning Objectives
- [ ] Install .NET SDK + VS Code / Visual Studio
- [ ] Write your first "Hello World" program
- [ ] Understand basic data types (int, string, bool)
- [ ] Learn about immutable data concepts
- [ ] Set up development environment

## 🛠️ Setup Instructions

### 1. Install .NET SDK
```bash
# Download from https://dotnet.microsoft.com/download
# Verify installation
dotnet --version
```

### 2. Create Your First F# Project
```bash
# Create new F# console app
dotnet new console -lang F# -n HelloFSharp
cd HelloFSharp

# Run the program
dotnet run
```

## 📝 Exercises

### Exercise 1: Hello World
Create `HelloWorld.fs`:
```fsharp
// Basic Hello World
printfn "Hello, F# World!"

// Interactive greeting
printf "What's your name? "
let name = System.Console.ReadLine()
printfn "Hello, %s! Welcome to F#!" name
```

### Exercise 2: Basic Types
Create `BasicTypes.fs`:
```fsharp
// Basic data types
let myInteger = 42
let myFloat = 3.14
let myString = "F# is awesome!"
let myBoolean = true
let myCharacter = 'A'

// Print all values
printfn "Integer: %d" myInteger
printfn "Float: %.2f" myFloat  
printfn "String: %s" myString
printfn "Boolean: %b" myBoolean
printfn "Character: %c" myCharacter
```

### Exercise 3: Immutability Demo
Create `ImmutableData.fs`:
```fsharp
// Immutable by default
let originalValue = 100
printfn "Original: %d" originalValue

// This creates a NEW binding, doesn't modify original
let originalValue = originalValue + 50  
printfn "New binding: %d" originalValue

// Mutable when explicitly declared
let mutable mutableValue = 100
printfn "Mutable original: %d" mutableValue

mutableValue <- mutableValue + 50
printfn "Mutable modified: %d" mutableValue
```

### Exercise 4: Simple Functions
Create `SimpleFunctions.fs`:
```fsharp
// Simple function definition
let greet name = 
    sprintf "Hello, %s!" name

// Function with multiple parameters
let add x y = x + y

// Function using the function
let calculateArea length width = 
    let area = length * width
    sprintf "Area of %d x %d = %d" length width area

// Test the functions
printfn "%s" (greet "Alice")
printfn "5 + 3 = %d" (add 5 3)
printfn "%s" (calculateArea 10 5)
```

## 🏃‍♂️ Practice Tasks

### Task 1: Personal Info Program
Create a program that:
1. Asks for user's name, age, and favorite color
2. Stores them in immutable values
3. Prints a personalized message

### Task 2: Simple Calculator Functions
Create functions for:
1. Addition, subtraction, multiplication, division
2. Test each function with different inputs
3. Handle division by zero with a message

### Task 3: Temperature Converter
Create functions to convert:
1. Celsius to Fahrenheit
2. Fahrenheit to Celsius  
3. Test with various temperatures

## ✅ Completion Checklist
- [ ] .NET SDK installed and working
- [ ] VS Code/Visual Studio configured for F#
- [ ] Completed all 4 exercises
- [ ] Finished all 3 practice tasks
- [ ] Can create and run F# console applications
- [ ] Understand immutability vs mutability
- [ ] Comfortable with basic F# syntax

## 📚 Additional Resources
- [F# for Fun and Profit - Why use F#?](https://fsharpforfunandprofit.com/why-use-fsharp/)
- [Microsoft F# Guide](https://docs.microsoft.com/en-us/dotnet/fsharp/)
- [F# Interactive (FSI) Guide](https://docs.microsoft.com/en-us/dotnet/fsharp/tools/fsharp-interactive/)

## 🎯 Next Steps
Once completed, proceed to **Day 3-4: Functional Thinking** to learn about functions, recursion, and pattern matching!