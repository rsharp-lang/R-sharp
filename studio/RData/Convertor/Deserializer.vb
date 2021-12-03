Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.RDataSet.Struct.LinkedList

Namespace Convertor

    Public Module Deserializer

        <Extension>
        Public Function ToObject(Of T As Class)(robj As RObject) As T

        End Function

    End Module
End Namespace