# F# Training Plan - Days 8-16

## 📅 Ngày 8-9: Làm việc với dữ liệu

### Mục tiêu học tập:
- [ ] String manipulation và processing
- [ ] File I/O operations (đọc/ghi file)  
- [ ] JSON serialization/deserialization
- [ ] Async workflows cơ bản

### Bài tập thực hành:
1. **String Processing**: Tạo text analyzer đếm từ, ký tự
2. **File Operations**: Log file reader/writer
3. **JSON Handling**: Configuration file manager
4. **Async Workflows**: Parallel file processing

---

## 📅 Ngày 10-11: Functional Patterns

### Mục tiêu học tập:
- [ ] Pipeline operators (`|>`, `<|`, `>>`, `<<`)
- [ ] Function composition
- [ ] Option type và pattern matching
- [ ] Result type cho error handling
- [ ] Railway-oriented programming

### Bài tập thực hành:
1. **Data Pipeline**: Transform và validate user input
2. **Error Handling**: Safe division calculator
3. **Composition**: Chained data transformations
4. **Validation Pipeline**: User registration system

---

## 📅 Ngày 12-13: Module & Tổ chức mã

### Mục tiêu học tập:
- [ ] Tạo và organize modules
- [ ] Namespace hierarchy
- [ ] Internal modules
- [ ] Unit testing với xUnit
- [ ] Test-driven development

### Bài tập thực hành:
1. **Library Design**: Math utilities module
2. **Testing**: Unit tests cho business logic
3. **Module Organization**: Multi-layer application
4. **Documentation**: XML docs và examples

---

## 📅 Ngày 14: Mini Project - CSV Processor

### Project Requirements:
- [ ] Đọc file CSV với different formats
- [ ] Data validation và cleaning
- [ ] Statistical analysis (sum, average, grouping)
- [ ] Generate reports (HTML/JSON output)
- [ ] Error handling và logging

### Features:
1. **CSV Reader**: Parse multiple CSV formats
2. **Data Analysis**: Sales/Financial report generator
3. **Export Options**: Multiple output formats
4. **CLI Interface**: Command-line tool

---

## 📅 Ngày 15-16: F# và Web với Giraffe

### Mục tiêu học tập:
- [ ] Setup Giraffe web framework
- [ ] Routing và middleware
- [ ] JSON serialization cho API
- [ ] Request/Response handling
- [ ] Basic authentication

### Bài tập thực hành:
1. **REST API**: CRUD operations
2. **JSON Endpoints**: Data exchange
3. **File Upload**: CSV processing API
4. **Web Interface**: Simple frontend integration

---

## 🚀 Khuyến nghị học tập:

### Ngày 8-9 (Hôm nay):
**Start with**: String & File Operations
1. Tạo text file analyzer
2. Implement async file operations  
3. JSON configuration system
4. Build foundation cho các ngày sau

### Tools cần cài đặt:
```bash
# JSON handling
dotnet add package System.Text.Json

# Testing framework  
dotnet add package xunit
dotnet add package xunit.runner.visualstudio

# Giraffe web framework (cho ngày 15-16)
dotnet add package Giraffe
```

### Project Structure đề xuất:
```
FSharpTraining/
├── Day8-9-DataHandling/
├── Day10-11-FunctionalPatterns/  
├── Day12-13-ModulesAndTesting/
├── Day14-MiniProject-CSV/
└── Day15-16-WebWithGiraffe/
```

Bạn muốn bắt đầu với phần nào trước? Tôi khuyên nên start với **String & File Operations** để build foundation tốt!