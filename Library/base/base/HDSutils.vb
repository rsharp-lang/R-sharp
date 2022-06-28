
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataStorage.HDSPack
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("HDF5.utils")>
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
        ElseIf TypeOf file Is String Then
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

End Module
