module Wav

open System.IO
open System


module Header =
    let write samplingRate numChannels numBytes
        (writer: BinaryWriter) = 
        let bytesPerSample = 4 // Float

        let chunkID = "RIFF"B
        let chunkSize = 36 + numBytes // * seek and update after numBytes is known
        let format = "WAVE"B
        let subChunk1ID = "fmt "B
        let subChunk1Size = 16
        let audioFormat = 3s // Float
        let nc = int16 numChannels
        let bitsPerSample = int16 (bytesPerSample * 8)
        let blockAlign = int16 (numChannels * bytesPerSample)
        let byteRate = samplingRate * numChannels * bytesPerSample
        let subChunk2Id = "data"B
        let subChunk2Size = numBytes // * seek and update after numBytes is known

        writer.Write(chunkID) // 0
        writer.Write(chunkSize) // 4 (*)
        writer.Write(format) // 8
        writer.Write(subChunk1ID) // 12
        writer.Write(subChunk1Size) // 16
        writer.Write(audioFormat) // 20
        writer.Write(nc) // 22
        writer.Write(samplingRate : int) // 24
        writer.Write(byteRate) // 28
        writer.Write(blockAlign) // 32
        writer.Write(bitsPerSample) // 34
        writer.Write(subChunk2Id) // 36
        writer.Write(subChunk2Size) // 40 (*)

let Save (fileName:string) samplingRate (samples : float [][]) = 
    let writeSamps (bw:BinaryWriter) (samples:float [][]) =
        let flatSamples =
            samples
            |> Array.concat

        flatSamples
        |> Array.map (float32 >> bw.Write)
        |> Array.length
        |> fun n -> n * 4

    use fileStream = new FileStream(fileName, FileMode.Create)
    use bw = new BinaryWriter(fileStream)
    let numChannels = Array.length samples.[0]

    // Write header
    Header.write samplingRate numChannels 0 bw

    // Pack and write the stream
    let bytesWritten = writeSamps bw samples
    
        // Now we should know the number of bytes    
    fileStream.Seek(4L, SeekOrigin.Begin) |> ignore
    bw.Write(36 + bytesWritten)
    fileStream.Seek(32L, SeekOrigin.Current) |> ignore
    bw.Write(bytesWritten)
    
    printfn "Written %s (%A bytes)" fileName bytesWritten