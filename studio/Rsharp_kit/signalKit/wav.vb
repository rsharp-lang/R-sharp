
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.Wave
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("wav", Category:=APICategories.UtilityTools)>
Public Module wavToolkit

    Sub New()
        Call Internal.ConsolePrinter.AttachConsoleFormatter(Of WaveFile)(AddressOf wavToString)
    End Sub

    Private Function wavToString(wav As WaveFile) As String
        Dim summary As New StringBuilder

        Call summary.AppendLine($"[{wav.magic}] {wav.fileSize} bytes")
        Call summary.AppendLine($"format: {wav.format}")
        Call summary.AppendLine()
        Call summary.AppendLine($"fmt_data_chunk:")
        Call summary.AppendLine($"     audio_format: {wav.fmt.audioFormat.ToString}")
        Call summary.AppendLine($"         channels: {wav.fmt.channels.ToString}")
        Call summary.AppendLine($"      sample_rate: {wav.fmt.SampleRate}")
        Call summary.AppendLine($"        byte_rate: {wav.fmt.ByteRate}")
        Call summary.AppendLine($"      block_align: {wav.fmt.BlockAlign}")
        Call summary.AppendLine($"  bits_per_sample: {wav.fmt.BitsPerSample}")
        Call summary.AppendLine($"           is_PCM: {wav.fmt.isPCM.ToString.ToUpper}")
        Call summary.AppendLine()
        Call summary.AppendLine($"wav_data:")

        Dim i As i32 = 1

        For Each sample In wav.data.AsEnumerable
            Call summary.AppendLine($"  sample #{++i}: {sample.channels.Take(6).JoinBy(", ")}...")
        Next

        Return summary.ToString
    End Function

    <ExportAPI("read.wav")>
    <RApiReturn(GetType(WaveFile))>
    Public Function readWav(<RRawVectorArgument> file As Object, Optional env As Environment = Nothing) As Object
        If file Is Nothing Then
            Return Internal.debug.stop("the given file object can not be nothing!", env)
        End If

        Dim dataFile As Stream

        If TypeOf file Is String Then
            dataFile = DirectCast(file, String).Open(FileMode.Open, doClear:=False, [readOnly]:=True)
        ElseIf TypeOf file Is String() Then
            dataFile = DirectCast(file, String())(Scan0).Open(FileMode.Open, doClear:=False, [readOnly]:=True)
        ElseIf TypeOf file Is Stream Then
            dataFile = DirectCast(file, Stream)
        ElseIf TypeOf file Is Byte() Then
            dataFile = New MemoryStream(DirectCast(file, Byte()))
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(Stream), file.GetType, env), env)
        End If

        Using reader As New BinaryDataReader(dataFile)
            Return WaveFile.Open(reader)
        End Using
    End Function
End Module
