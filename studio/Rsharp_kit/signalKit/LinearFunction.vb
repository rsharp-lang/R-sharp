Imports Microsoft.VisualBasic.Math.SignalProcessing
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

Public Class LinearFunction : Inherits RDefaultFunction

    Friend linear As Resampler

    <RDefaultFunction>
    Public Function GetSignal(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
        Dim x_vec As Double() = CLRVector.asNumeric(x)
        Dim y_vec As Double() = linear(x_vec)

        Return y_vec
    End Function
End Class
