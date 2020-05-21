Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports REnv = SMRUCC.Rsharp.Runtime

Public Module ApiArgumentHelpers

    Public Function GetDoubleRange(value As Object, env As Environment,
                                   Optional default$ = "0,1",
                                   <CallerMemberName>
                                   Optional api$ = Nothing) As [Variant](Of DoubleRange, Message)
        If value Is Nothing Then
            Return CType([default], DoubleRange)
        End If

        Select Case value.GetType
            Case GetType(vector)
                Dim v As vector = value
                Dim vec As Double()

                Select Case v.elementType.mode
                    Case TypeCodes.double
                        vec = REnv.asVector(Of Double)(v.data)
                    Case TypeCodes.integer
                        vec = REnv.asVector(Of Double)(v.data)
                    Case TypeCodes.string

                        If v.length = 1 Then
                            Return CType(DirectCast(v.data.GetValue(Scan0), String), DoubleRange)
                        Else
                            vec = v.data _
                                .AsObjectEnumerator(Of String) _
                                .Select(AddressOf Val) _
                                .ToArray
                        End If

                    Case Else
                        Return debug.stop({
                            "invalid vector data type!",
                            "mode: " & v.elementType.mode,
                            "raw: " & v.elementType.fullName
                        }, env)
                End Select

                If vec.Length < 2 Then
                    Return debug.stop({
                        "a numeric range required two boundary value at least!",
                        "api: " & api
                    }, env)
                Else
                    Return New DoubleRange(vec)
                End If
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
