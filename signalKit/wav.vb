
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.Wave
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("wav", Category:=APICategories.UtilityTools)>
Public Module wavToolkit

    <ExportAPI("read.wav")>
    <RApiReturn(GetType(WaveFile))>
    Public Function readWav(<RRawVectorArgument> file As Object, Optional env As Environment = Nothing) As Object
        If file Is Nothing Then
            Return Internal.debug.stop("the given file object can not be nothing!", env)
        End If

        Dim dataFile As Stream

        If TypeOf file Is String Then
            dataFile = DirectCast(file, String).Open(FileMode.Open, doClear:=False, [readOnly]:=True)
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
