Imports System.Runtime.CompilerServices
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Object.Linq

    Module RCType

        <Extension>
        Public Function aslogical(vec As vector) As Boolean()
            Return REnv.asVector(Of Boolean)(vec.data)
        End Function

        <Extension>
        Public Function ascharacter(vec As vector) As String()
            Return REnv.asVector(Of String)(vec.data)
        End Function

        <Extension>
        Public Function asnumeric(vec As vector) As Double()
            Return REnv.asVector(Of Double)(vec.data)
        End Function

        <Extension>
        Public Function asinteger(vec As vector) As Long()
            Return REnv.asVector(Of Long)(vec.data)
        End Function
    End Module
End Namespace