Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports REnv = SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal

Public Module ApiArgumentHelpers

    Public Function GetDoubleRange(value As Object, env As Environment, Optional default$ = "0,1") As [Variant](Of DoubleRange, Message)
        If value Is Nothing Then
            Return CType([default], DoubleRange)
        End If

        Select Case value.GetType
            Case GetType(vector)
                Return CType(DirectCast(REnv.asVector(Of Double)(DirectCast(value, vector).data), Double()), DoubleRange)
            Case GetType(DoubleRange)
                Return DirectCast(value, DoubleRange)
            Case GetType(String)
                Return CType(DirectCast(value, String), DoubleRange)
            Case Else
                Return debug.stop({
                    "invalid data type for cast to a numeric range!",
                    "required: " & GetType(DoubleRange).FullName,
                    "given: " & value.GetType.FullName
                }, env)
        End Select
    End Function
End Module
