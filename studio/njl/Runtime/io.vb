
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("io")>
Public Module io

    ''' <summary>
    ''' Apply the function f to the result of open(args...; kwargs...) 
    ''' and close the resulting file descriptor upon completion.
    ''' </summary>
    ''' <param name="f"></param>
    ''' <param name="filename"></param>
    ''' <param name="mode"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("open")>
    Public Function open(f As Object,
                         Optional filename As String = Nothing,
                         Optional mode As String = "w",
                         Optional env As Environment = Nothing) As Object

    End Function

    ''' <summary>
    ''' open for writing
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="x"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("write")>
    Public Function write(file As Stream, <RRawVectorArgument> x As Object,
                          Optional env As Environment = Nothing)

    End Function

    ''' <summary>
    ''' open for reading
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="type"></param>
    ''' <returns></returns>
    <ExportAPI("read")>
    Public Function read(file As Stream, Optional type As String = "String") As Object

    End Function
End Module
