namespace Shop.Business

module ErrorHandling =
    open Shop.Models
    
    // Day 10-11: Error handling với Railway Oriented Programming
    let bind f result =
        match result with
        | Ok value -> f value
        | Error err -> Error err
    
    let map f result =
        match result with
        | Ok value -> Ok (f value)
        | Error err -> Error err
    
    let mapError f result =
        match result with
        | Ok value -> Ok value
        | Error err -> Error (f err)
    
    // Validation combinators
    let validateAll (validations: Result<'a, ValidationError> list) =
        let errors = 
            validations 
            |> List.choose (function Error e -> Some e | Ok _ -> None)
        
        if errors.IsEmpty then
            validations 
            |> List.choose (function Ok v -> Some v | Error _ -> None)
            |> Ok
        else
            Error (ValidationErrors errors)
    
    // Error formatting
    let formatValidationError = function
        | InvalidEmail email -> $"Invalid email format: {email}"
        | InvalidPhone phone -> $"Invalid phone number: {phone}"
        | EmptyField field -> $"Field cannot be empty: {field}"
        | InvalidPrice price -> $"Price must be positive: {price}"
        | InsufficientStock (requested, available) -> 
            $"Insufficient stock. Requested: {requested}, Available: {available}"
    
    let formatShopError = function
        | ValidationErrors errors ->
            errors 
            |> List.map formatValidationError
            |> String.concat "; "
        | ProductNotFound (ProductId id) -> $"Product not found: {id}"
        | CustomerNotFound (CustomerId id) -> $"Customer not found: {id}"
        | OrderNotFound (OrderId id) -> $"Order not found: {id}"
        | FileError msg -> $"File error: {msg}"
        | JsonError msg -> $"JSON error: {msg}"