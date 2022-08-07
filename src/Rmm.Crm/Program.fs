// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"

open Rmm

let test = DB "Server=KBNTSSQL38\\TEST02; Database=RMM_MSCRM; Integrated Security=SSPI; Connect Timeout=3; TrustServerCertificate=True"

let env = test

let endResult = async {
    let! result = Data.Logs.latest env
    result
    |> Array.iter (fun l -> printfn $"%A{l}")
    return 0
}


printfn $"End of Program: %i{Async.RunSynchronously endResult}"
