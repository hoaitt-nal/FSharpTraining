# F# Shop Application - Training Days 8-16

## 🏪 Shop Application Demo

This is a comprehensive F# training application demonstrating functional programming concepts from Days 8-16.

### 🚀 How to Run

```bash
# Navigate to Shop directory
cd Shop

# Build the application
dotnet build

# Run the demo
dotnet run
```

### 📁 Project Structure

```
Shop/
├── Models/
│   └── Domain.fs           # Core domain models and types
├── DataAccess/
│   ├── StringUtils.fs      # String processing and search
│   ├── FileOperations.fs   # File I/O operations
│   ├── JsonHandler.fs      # JSON serialization
│   └── AsyncWorkflows.fs   # Async computation patterns
├── Business/
│   ├── ErrorHandling.fs    # Railway Oriented Programming
│   └── Pipelines.fs        # Functional composition
├── Program.fs              # Main entry point with demos
└── Shop.fsproj            # Project configuration
```

### 🎯 Features Demonstrated

- **Day 8-9**: String Processing & Search Algorithms
- **Day 10-11**: File I/O Operations with Async
- **Day 12-13**: Functional Pipelines & Composition  
- **Day 14**: Async Workflows & Parallel Processing
- **Day 15-16**: Error Handling & Module Organization

### 📊 Output Files

After running, check the `Data/` folder for generated files:
- `config.json` - Configuration settings
- `shop.log` - Application logs
- Various demo output files

### 🛠️ Technology Stack

- F# .NET 9.0
- System.Text.Json
- Functional Programming Patterns
- Async/Await
- Result Types for Error Handling

### 💡 Key Concepts Demonstrated

1. **Functional Programming**: Immutable data, pure functions, composition
2. **Type Safety**: Union types, option types, pattern matching
3. **Error Handling**: Railway Oriented Programming with Result types
4. **Async Programming**: Async workflows and parallel processing
5. **Modular Design**: Clean separation of concerns