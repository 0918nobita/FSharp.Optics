module Performance

open System.Diagnostics

[<Measure>] type ms
[<Measure>] type ns

let private convertMsToNs (ms : float<ms>) : float<ns> =
    ms * 1000000.0<ns/ms>

let measureExecutionTime (numOfTimes : int) (op : unit -> 'a) : float<ns> =
    let stopWatch = Stopwatch.StartNew()
    for _ in 1..numOfTimes do
        ignore <| op ()
    stopWatch.Stop()
    let elapsed = 1.0<ms> * float stopWatch.ElapsedMilliseconds
    elapsed / float numOfTimes
    |> convertMsToNs
