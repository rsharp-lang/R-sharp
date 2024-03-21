Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    Module Polyglot

        <ExportAPI("register")>
        Public Function register(<RRawVectorArgument> file_types As Object, assembly As Assembly, Optional env As Environment = Nothing) As Object

        End Function

        <ExportAPI("assembly")>
        <RApiReturn(GetType(Assembly))>
        Public Function assembly(filename As String, Optional env As Environment = Nothing) As Object

        End Function
    End Module
End Namespace