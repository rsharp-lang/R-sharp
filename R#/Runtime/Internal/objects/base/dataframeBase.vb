
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Object

    Module dataframeBase

        <ExportAPI("colSums")>
        Public Function colSums(x As dataframe, Optional env As Environment = Nothing) As vector
            Dim names As String() = x.colnames
            Dim vec As Double() = names _
                .Select(Function(v)
                            Return DirectCast(REnv.asVector(Of Double)(x.columns(v)), Double()).Sum
                        End Function) _
                .ToArray

            Return New vector(names, vec, env)
        End Function
    End Module
End Namespace