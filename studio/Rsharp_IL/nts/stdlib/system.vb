Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace jsstd

    Public Module system

        <ExportAPI("Date")>
        Public Function [Date]() As Object
            Return Now
        End Function

        <ExportAPI("parseInt")>
        Public Function parseInt(<RRawVectorArgument> x As Object) As Object
            Return CLRVector.asLong(x)
        End Function

        <ExportAPI("parseFloat")>
        Public Function parseFloat(<RRawVectorArgument> x As Object) As Object
            Return CLRVector.asNumeric(x)
        End Function
    End Module
End Namespace