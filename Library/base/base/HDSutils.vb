#Region "Microsoft.VisualBasic::8a6319e87d30f36834fcfe1e86314d3f, F:/GCModeller/src/R-sharp/Library/base//base/HDSutils.vb"

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

'   Total Lines: 204
'    Code Lines: 157
' Comment Lines: 12
'   Blank Lines: 35
'     File Size: 7.36 KB


' Module HDSutils
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: createStream, DiskDefragmentation, ExtractFiles, getData, listFiles
'               openStream, readText, saveFile, Tree
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataStorage.HDSPack
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

''' <summary>
''' HDS stream pack toolkit
''' </summary>
<Package("HDS")>
Module HDSutils

    Sub New()
        Call Internal.ConsolePrinter.AttachConsoleFormatter(Of StreamGroup)(Function(o) o.ToString)
        Call Internal.ConsolePrinter.AttachConsoleFormatter(Of StreamBlock)(Function(o) o.ToString)
    End Sub

    ''' <summary>
    ''' Create a new data pack stream object, this function will clear up the data that sotre in the target <paramref name="file"/>!
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="meta_size"></param>
    ''' <returns></returns>
    <ExportAPI("createStream")>
    Public Function createStream(file As String, Optional meta_size As Long = 4 * 1024 * 1024) As Object
        Dim pack As New StreamPack(file, meta_size:=meta_size)
        Call pack.Clear(meta_size)
        Return pack
    End Function

    ''' <summary>
    ''' Extract the files inside the HDS pack to a specific filesystem environment
    ''' </summary>
    ''' <param name="pack"></param>
    ''' <param name="fs"></param>
    ''' <returns></returns>
    <ExportAPI("extract_files")>
    Public Function ExtractFiles(pack As StreamPack, fs As IFileSystemEnvironment) As Object
        For Each file As StreamBlock In pack.files
            Dim data = pack.OpenBlock(file)
            Dim newfile = fs.OpenFile(file.referencePath.ToString, FileMode.OpenOrCreate, FileAccess.Write)

            Call data.CopyTo(newfile)
            Call newfile.Flush()
            Call newfile.Dispose()
        Next

        Return True
    End Function

    <ExportAPI("disk_defragment")>
    Public Function DiskDefragmentation(pack As StreamPack) As Object
        Dim buffer As New MemoryStream
        Dim newPack As New StreamPack(buffer, 8 * 1024 * 1024)

        For Each file As StreamBlock In pack.files
            Dim data = pack.OpenBlock(file)
            Dim newfile = newPack.OpenBlock(file.fullName)

            data.CopyTo(newfile)
            newfile.Flush()
            newfile.Dispose()

            For Each attr In file.attributes.attributes

            Next
        Next

        Return newPack
    End Function

    ''' <summary>
    ''' Open a HDS stream pack file, this function will create a new file is the given <paramref name="file"/> is not exists
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="readonly">Indicates that the file content just used for readonly, not allow updates?</param>
    ''' <param name="allowCreate"></param>
    ''' <param name="meta_size"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("openStream")>
    <RApiReturn(GetType(StreamPack))>
    Public Function openStream(<RRawVectorArgument> file As Object,
                               Optional [readonly] As Boolean = False,
                               Optional allowCreate As Boolean = False,
                               Optional meta_size As Long = 8 * 1024 * 1024,
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
                Return New StreamPack(DirectCast(file, String), [readonly]:=[readonly], meta_size:=meta_size)
            ElseIf allowCreate Then
                Return HDSutils.createStream(DirectCast(file, String), meta_size:=meta_size)
            Else
                Return Internal.debug.stop({
                     "the given file is not found on your filesystem!",
                     "You can set the parameter 'allowCreate' = TRUE for enable create a new local file!"
                }, env)
            End If
        ElseIf TypeOf file Is Stream Then
            Return New StreamPack(DirectCast(file, Stream), [readonly]:=[readonly], meta_size:=meta_size)
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(String), file.GetType, env), env)
        End If
    End Function

    ''' <summary>
    ''' Get files in a specific directory
    ''' </summary>
    ''' <param name="pack"></param>
    ''' <param name="dir">the default directory path is root, for get all data files</param>
    ''' <returns></returns>
    <ExportAPI("files")>
    Public Function listFiles(pack As StreamPack,
                              Optional dir As String = "/",
                              Optional excludes_dir As Boolean = False,
                              Optional env As Environment = Nothing) As Object

        Dim objs As StreamObject()

        If dir.StringEmpty OrElse dir = "/" OrElse dir = "\" Then
            objs = pack.ListFiles.ToArray
        Else
            Dim folder As StreamGroup = pack.GetObject(dir & "/")

            If folder Is Nothing Then
                Dim msg As String() = {
                    $"the given required directory path({dir}) is missing from the data pack!",
                    $"dir: {dir}"
                }

                If env.globalEnvironment.options.strict Then
                    Return Internal.debug.stop(msg, env)
                Else
                    env.AddMessage(msg, MSG_TYPES.WRN)
                    Return Nothing
                End If
            Else
                objs = folder.ListFiles.ToArray
            End If
        End If

        If excludes_dir Then
            objs = objs _
                .Where(Function(o) TypeOf o Is StreamBlock) _
                .ToArray
        End If

        Return objs
    End Function

    ''' <summary>
    ''' Get the filesystem tree inside the given HDS pack file
    ''' </summary>
    ''' <param name="pack"></param>
    ''' <param name="showReadme"></param>
    ''' <returns></returns>
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

    ''' <summary>
    ''' get a stream in readonly mode for parse data
    ''' </summary>
    ''' <param name="pack"></param>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <ExportAPI("getData")>
    Public Function getData(pack As StreamPack, file As StreamBlock) As Stream
        Return pack.OpenBlock(file)
    End Function

    ''' <summary>
    ''' Read a specific text data from a HDS stream pack
    ''' </summary>
    ''' <param name="pack"></param>
    ''' <param name="fileName"></param>
    ''' <returns></returns>
    <ExportAPI("getText")>
    Public Function readText(pack As StreamPack, fileName As String,
                             Optional encoding As Object = "utf8",
                             Optional env As Environment = Nothing) As String

        Dim file As StreamBlock = pack.GetObject(fileName)
        Dim encoder = SMRUCC.Rsharp.GetEncoding(encoding)

        If file Is Nothing Then
            Return Nothing
        Else
            Using buffer As Stream = pack.OpenBlock(file),
                read As New StreamReader(buffer, encoding:=encoder)

                Return read.ReadToEnd
            End Using
        End If
    End Function

    <ExportAPI("writeText")>
    Public Function writeText(pack As StreamPack, fileName As String,
                              <RRawVectorArgument>
                              text As Object) As Boolean

        Return pack.WriteText(CLRVector.asCharacter(text), fileName)
    End Function

    ''' <summary>
    ''' write data to a HDS stream pack file
    ''' </summary>
    ''' <param name="hds"></param>
    ''' <param name="fileName"></param>
    ''' <param name="data"></param>
    ''' <returns></returns>
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
