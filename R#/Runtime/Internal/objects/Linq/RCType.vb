Imports System.Runtime.CompilerServices
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Object.Linq

    Module RCType

        <Extension>
        Public Function aslogical(vec As vector) As Boolean()
            Return REnv.asVector(Of Boolean)(vec.data)
        End Function
    End Module
End Namespace