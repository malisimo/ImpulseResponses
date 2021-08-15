// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.IO

let srcDir = @"C:\Users\matto\Downloads\"
let outDir = @"C:\Users\matto\Samples\ImpulseResponses"

let replaceFileExtension (fromExt:string) toExt (fileName:string) =
    if fileName.EndsWith fromExt then
        fileName.Substring(0, fileName.Length - fromExt.Length) + toExt
    else
        fileName

let convertFile inFile =
    let outFile =
        Path.Combine(outDir, Path.GetRelativePath(srcDir, inFile))
        |> replaceFileExtension "wir" "wav"

    let destDir = Path.GetDirectoryName(outFile)
    if Directory.Exists destDir |> not then
        Directory.CreateDirectory(destDir) |> ignore
    
    match Wir.TryLoad inFile with
    | None ->
        Error(inFile, "Problem loading file")
    | Some(wir) ->
        if wir.Samples.Length = 0 then        
            Error(inFile, "No samples found")
        else
            if wir.Samples.[0].Length = 0 || wir.Samples.[0].Length <> wir.Header.channels then
                Error(inFile, "Incorrect or inconsistent number of channels")
            else
                let wirNorm = { wir with Samples = wir.Samples |> Dsp.toStereo |> Dsp.normalise }

                Wav.Save outFile wirNorm.Header.fs wirNorm.Samples

                Ok(inFile, sprintf "Sample rate: %i, Channels: %i" wirNorm.Header.fs wirNorm.Header.channels)

let printResult (r:Result<string*string,string*string>) =
    match r with
    | Ok(f,res) ->
        Console.ForegroundColor <- ConsoleColor.Green
        printfn "Written %s (%s)" f res
    | Error(f,res) ->
        Console.ForegroundColor <- ConsoleColor.DarkYellow
        printfn "Skipped %s: %s" f res
    
    r

let splitResult (r:Result<'a,'b>) =
    match r with
    | Ok(_) -> true
    | Error(_) -> false

[<EntryPoint>]
let main _ =
    let wirFiles =
        Directory.EnumerateFiles(srcDir, "*.wir", SearchOption.AllDirectories)
        |> Seq.toArray
    
    let succeeded,failed =
        wirFiles
        |> Array.map (convertFile >> printResult)
        |> Array.partition splitResult

    printfn "Converted %i files successfully" succeeded.Length
    printfn "Skipped %i files" failed.Length

    0 // return an integer exit code