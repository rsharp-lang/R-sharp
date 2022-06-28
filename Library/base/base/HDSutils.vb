
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

    <ExportAPI("openStream")>
    <RApiReturn(GetType(StreamPack))>
    Public Function openStream(<RRawVectorArgument> file As Object, Optional env As Environment = Nothing) As Object
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
                Return New StreamPack(DirectCast(file, String))
            Else
                Return Internal.debug.stop("the given file is not found on your filesystem!", env)
            End If
        ElseIf TypeOf file Is Stream Then
            Return New StreamPack(DirectCast(file, Stream))
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(String), file.GetType, env), env)
        End If
    End Function

    <ExportAPI("files")>
    Public Function listFiles(pack As StreamPack) As Object
        Return pack.ListFiles.ToArray
    End Function

    <ExportAPI("tree")>
    Public Function Tree(pack As StreamPack) As String
        Dim sb As New StringBuilder
        Dim device As New StringWriter(sb)

        Call pack.superBlock.Tree(device,)
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

End Module
