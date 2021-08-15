module Wir

open System.IO
open System

module Header =
    type WirHeader = {
        magic:string // "wvIR" (4 bytes)
        fileSizeLE:int // filesize-8
        version:string // version "ver1fmt " (8 bytes)
        headerSizeLE:int
        i3:int // 0x3
        channels:int
        fs:int // impulse Fs
        fs2:int
        i4:int // MONO 0x4 STEREO 0x8 4CH 0x10
        i5:int // 0x17
        data:string // "data" (4 bytes)
    } with
        static member FromStream (s:Stream) =
            let br = new BinaryReader(s)
            {
                magic = String(br.ReadChars 4)
                fileSizeLE = br.ReadInt32()
                version = String(br.ReadChars 8)
                headerSizeLE = br.ReadInt32()
                i3 = br.ReadInt16() |> int
                channels = br.ReadInt16() |> int
                fs = br.ReadInt32()
                fs2 = br.ReadInt32()
                i4 = br.ReadInt16() |> int
                i5 = br.ReadInt16() |> int
                data = String(br.ReadChars 4)
            }

type WirFile = {
    Header:Header.WirHeader
    Samples:float [][]
}

let TryLoad (fileName:string) =
    let rec readSamps arr nChannels (br:BinaryReader) =
        let bytesToRead = ((nChannels |> int64) * 4L)
        let nextPos = br.BaseStream.Position + bytesToRead
        
        if nextPos < br.BaseStream.Length then
            readSamps ([| for _ in 0..(nChannels-1) -> br.ReadSingle() |> float |] :: arr) nChannels br
        else
            arr
            |> List.toArray
            |> Array.rev

    if File.Exists fileName |> not then
        None
    else
        use fs = new FileStream(fileName, FileMode.Open, FileAccess.Read)

        if fs.Length < 41L then
            None
        else
            let header = Header.WirHeader.FromStream fs
            
            {
                Header = header
                // rest of the data is FLOAT_LE (32bit float)
                Samples = 
                    use br = new BinaryReader(fs)
                    readSamps [] header.channels br
            }
            |> Some
