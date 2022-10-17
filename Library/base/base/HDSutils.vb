#Region "Microsoft.VisualBasic::cf2525c35660834d0eb9cbed40aca73a, R-sharp\Library\base\base\HDSutils.vb"

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

    '   Total Lines: 112
    '    Code Lines: 93
    ' Comment Lines: 0
    '   Blank Lines: 19
    '     File Size: 3.83 KB


    ' Module HDSutils
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: createStream, listFiles, openStream, readText, saveFile
    '               Tree
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataStorage.HDSPack
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("HDS")>
Module HDSutils

    Sub New()
        Call Internal.ConsolePrinter.AttachConsoleFormatter(Of StreamGroup)(Function(o) o.ToString)
        Call Internal.ConsolePrinter.AttachConsoleFormatter(Of StreamBlock)(Function(o) o.ToString)
    End Sub

    <ExportAPI("createStream")>
    Public Function createStream(file As String) As Object
        Dim pack As New StreamPack(file)
        Call pack.Clear()
        Return pack
    End Function

    <ExportAPI("openStream")>
    <RApiReturn(GetType(StreamPack))>
    Public Function openStream(<RRawVectorArgument> file As Object,
                               Optional [readonly] As Boolean = False,
                               Optional env As Environment = Nothing) As Object

        If file Is Nothing Then
            Return Internal.debug.stop("file for open can not be nothing!", env)
        ElseIf TypeOf file Is vector Then
            Dim v As Array = DirectCast(file, vector).data

            If v.Length = 0 Then
                Return Internal.debug.stop("file for open can not be nothing!", env)
            ElseIf v.Length > 1 Then
                file = v.GetValue(Scan0)
                env.AddMessage($"the input file vector contains more than one element, use the first element as file path!")
            Else
                file = v.GetValue(Scan0)
            End If
        End If

        If TypeOf file Is String Then
            If DirectCast(file, String).FileExists Then
                Return New StreamPack(DirectCast(file, String), [readonly]:=[readonly])
            Else
                Return Internal.debug.stop("the given file is not found on your filesystem!", env)
            End If
        ElseIf TypeOf file Is Stream Then
            Return New StreamPack(DirectCast(file, Stream), [readonly]:=[readonly])
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(String), file.GetType, env), env)
        End If
    End Function

    <ExportAPI("files")>
    Public Function listFiles(pack As StreamPack) As Object
        Return pack.ListFiles.ToArray
    End Function

    <ExportAPI("tree")>
    Public Function Tree(pack As StreamPack, Optional showReadme As Boolean = True) As String
        Dim sb As New StringBuilder
        Dim device As New StringWriter(sb)

        Call pack.superBlock.Tree(
            text:=device,
            pack:=pack,
            showReadme:=showReadme
        )
        Call device.Flush()

        Return sb.ToString
    End Function

    <ExportAPI("getText")>
    Public Function readText(pack As StreamPack, fileName As String) As String
        Dim file As StreamBlock = pack.GetObject(fileName)

        If file Is Nothing Then
            Return Nothing
        Else
            Using buffer As Stream = pack.OpenBlock(file),
                read As New StreamReader(buffer)

                Return read.ReadToEnd
            End Using
        End If
    End Function

    <ExportAPI("saveFile")>
    Public Function saveFile(hds As StreamPack, fileName As String, data As Object)
        Using buf As Stream = hds.OpenBlock(fileName)
            Dim write As Byte() = {}

            If TypeOf data Is String Then
                write = Encoding.UTF8.GetBytes(CStr(data))
            End If

            Call buf.Write(write, Scan0, write.Length)
            Call buf.Flush()
        End Using

        Return True
    End Function

End Module
