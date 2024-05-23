Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Development.Components

    Public Class AggregateFunction : Inherits RDefaultFunction

        ReadOnly aggregate As Func(Of IEnumerable(Of Double), Double)

        Default Public ReadOnly Property eval_vec(x As IEnumerable(Of Double)) As Double
            Get
                Return aggregate(x)
            End Get
        End Property

        Sub New(fx As Func(Of IEnumerable(Of Double), Double))
            aggregate = fx
        End Sub

        <RDefaultFunction>
        Public Function eval(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            Dim vec As Double() = CLRVector.asNumeric(x)
            Dim agg As Double = aggregate(vec)

            Return agg
        End Function

    End Class
End Namespace