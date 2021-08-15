module Dsp

let normalise (samps:float [][]) =
    let maxSamp = 
        samps
        |> Array.map (fun chan ->
            chan
            |> Array.max)
        |> Array.max

    let scale =
        if maxSamp <> 0. then 1. / maxSamp else 1.
    
    samps
    |> Array.map (fun chanSamps ->
        chanSamps
        |> Array.map (fun s -> s * scale))

let toStereo (samps:float [][]) =
    match samps.[0].Length with
    | i when i < 3 -> samps
    | _ ->
        samps
        |> Array.map (fun chanSamps -> Array.take 2 chanSamps)