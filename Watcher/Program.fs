open FSharp.Management
open Newtonsoft.Json
open System
open System.IO

type Local = WmiProvider<"localhost">

type Process = {
    ElapsedTime             : Nullable<uint64>
    IDProcess               : Nullable<uint32>
    IODataOperationsPerSec  : Nullable<uint64>
    IOReadBytesPerSec       : Nullable<uint64>
    IOWriteBytesPerSec      : Nullable<uint64>
    IODataBytesPerSec       : Nullable<uint64>
    IOOtherBytesPerSec      : Nullable<uint64>
    Name                    : string
    PercentUserTime         : Nullable<uint64>
    ThreadCount             : Nullable<uint32>
}

type OSMemory = {
    AvailableBytes  : Nullable<uint64>
    AvailableKBytes : Nullable<uint64>
    AvailableMBytes : Nullable<uint64>
}

[<EntryPoint>]
let main argv =

    let startTime = DateTime.UtcNow.ToString("yyyyMMddHHmmss")
    let fileName = "watcher_" + startTime + ".txt"
    let appendFile (fileName:string) (text:string) = 
        use file = new StreamWriter(fileName, true)
        file.WriteLine(text)
        file.Close()

    let timer = new Timers.Timer(float 5000)
    let event = Async.AwaitEvent (timer.Elapsed) |> Async.Ignore
    timer.Start()

    while true do
        Async.RunSynchronously event
        let processData = Local.GetDataContext().Win32_PerfFormattedData_PerfProc_Process
        let memoryData = Local.GetDataContext().Win32_PerfFormattedData_PerfOS_Memory

        let processes : Process list = [for d in processData -> {ElapsedTime            = d.ElapsedTime
                                                                 IDProcess              = d.IDProcess
                                                                 IODataOperationsPerSec = d.IODataOperationsPersec
                                                                 IOReadBytesPerSec      = d.IOReadBytesPersec
                                                                 IOWriteBytesPerSec     = d.IOWriteBytesPersec
                                                                 IODataBytesPerSec      = d.IODataBytesPersec
                                                                 IOOtherBytesPerSec     = d.IOOtherBytesPersec
                                                                 Name                   = d.Name
                                                                 PercentUserTime        = d.PercentUserTime
                                                                 ThreadCount            = d.ThreadCount}]
        let cardanoNodeProcess = List.filter (fun (proc : Process) -> proc.Name = "cardano-node") processes

        let memory : OSMemory list = [for d in memoryData -> {AvailableBytes  = d.AvailableBytes
                                                              AvailableKBytes = d.AvailableKBytes
                                                              AvailableMBytes = d.AvailableMBytes}]


        appendFile fileName (JsonConvert.SerializeObject cardanoNodeProcess)
        appendFile fileName (JsonConvert.SerializeObject memory)

    0
