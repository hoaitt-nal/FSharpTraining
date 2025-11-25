namespace Calculator

module CalculatorProgram =
    open UserInterface

    [<EntryPoint>]
    let main argv =
        try
            runApplication ()
        with
        | ex -> 
            printfn "An error occurred: %s" ex.Message
            
        0 // Exit code