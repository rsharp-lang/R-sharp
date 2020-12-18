Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.System.Package.File

<Package("package_utils")>
Module package

    <ExportAPI("read")>
    <RApiReturn(GetType(Expression))>
    Public Function loadExpr(<RRawVectorArgument> raw As Object, Optional env As Environment = Nothing) As Object
        raw = env.castToRawRoutine(raw, Encodings.UTF8WithoutBOM, True)

        If TypeOf raw Is Message Then
            Return raw
        End If

        Using io As New BinaryReader(DirectCast(raw, Stream))
            io.BaseStream.Seek(Scan0, SeekOrigin.Begin)
            Return BlockReader.Read(io).Parse
        End Using
    End Function
End Module
