﻿#Region "Microsoft.VisualBasic::fab91a051627d24226dede07ab8c0dfb, studio\Rsharp_kit\signalKit\wav.vb"

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


    ' Code Statistics:

    '   Total Lines: 94
    '    Code Lines: 65 (69.15%)
    ' Comment Lines: 17 (18.09%)
    '    - Xml Docs: 88.24%
    ' 
    '   Blank Lines: 12 (12.77%)
    '     File Size: 4.27 KB


    ' Module wavToolkit
    ' 
    '     Function: readWav, wavToString
    ' 
    '     Sub: Main
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
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

''' <summary>
''' Waveform Audio File Format (WAVE, or WAV due to its filename extension; pronounced /wæv/ or /weɪv/) 
''' is an audio file format standard, developed by IBM and Microsoft, for storing an audio bitstream on
''' personal computers. It is the main format used on Microsoft Windows systems for uncompressed audio. 
''' The usual bitstream encoding is the linear pulse-code modulation (LPCM) format.
'''
''' WAV Is an application Of the Resource Interchange File Format (RIFF) bitstream format method For 
''' storing data In chunks, And thus Is similar To the 8SVX And the Audio Interchange File Format (AIFF) 
''' format used On Amiga And Macintosh computers, respectively.
''' </summary>
<Package("wav", Category:=APICategories.UtilityTools)>
Public Module wavToolkit

    Friend Sub Main()
        Call RInternal.ConsolePrinter.AttachConsoleFormatter(Of WaveFile)(AddressOf wavToString)
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
            Return RInternal.debug.stop("the given file object can not be nothing!", env)
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
            Return RInternal.debug.stop(Message.InCompatibleType(GetType(Stream), file.GetType, env), env)
        End If

        Return WaveFile.Open(New BinaryDataReader(dataFile), lazy:=lazy)
    End Function
End Module
