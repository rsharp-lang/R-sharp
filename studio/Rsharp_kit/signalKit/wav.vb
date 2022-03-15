#Region "Microsoft.VisualBasic::80bbacda3188e2e71dbacd6ce0b7af02, R-sharp\studio\Rsharp_kit\signalKit\wav.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


     Code Statistics:

        Total Lines:   83
        Code Lines:    64
        Comment Lines: 7
        Blank Lines:   12
        File Size:     3.49 KB


    ' Module wavToolkit
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: readWav, wavToString
    ' 
    ' /********************************************************************************/

#End Region

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
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("wav", Category:=APICategories.UtilityTools)>
Public Module wavToolkit

    Sub New()
        Call Internal.ConsolePrinter.AttachConsoleFormatter(Of WaveFile)(AddressOf wavToString)
    End Sub

    Private Function wavToString(wavObj As Object) As String
        Dim summary As New StringBuilder
        Dim wav As WaveFile = DirectCast(wavObj, WaveFile)

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

        If TypeOf wav.data Is DataSubChunk Then
            Dim i As i32 = 1

            Call summary.AppendLine($"wav_data:")

            For Each sample As Sample In DirectCast(wav.data, DataSubChunk).AsEnumerable
                Call summary.AppendLine($"  sample #{++i}: {sample.channels.Take(6).JoinBy(", ")}...")
            Next
        End If

        Return summary.ToString
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="lazy"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("read.wav")>
    <RApiReturn(GetType(WaveFile))>
    Public Function readWav(<RRawVectorArgument> file As Object, Optional lazy As Boolean = False, Optional env As Environment = Nothing) As Object
        If file Is Nothing Then
            Return Internal.debug.stop("the given file object can not be nothing!", env)
        ElseIf TypeOf file Is vector Then
            file = DirectCast(file, vector).data
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

        Return WaveFile.Open(New BinaryDataReader(dataFile), lazy:=lazy)
    End Function
End Module
