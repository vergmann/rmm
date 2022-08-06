// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"

open Rmm

let endResult = async {
    let! result = Logs.latest
    result
    |> Array.iter (fun l -> printfn $"%A{l}")
    return 0
}

printfn $"End of Program: %i{Async.RunSynchronously endResult}"
