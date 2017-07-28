Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter

    Public Class Interpreter

        ''' <summary>
        ''' 全局环境
        ''' </summary>
        Public ReadOnly Property globalEnvir As New Environment

        Public Function Evaluate(script$) As Object
            Return Codes.TryParse(script).RunProgram(globalEnvir)
        End Function

        Public Function Source(path$, args As IEnumerable(Of NamedValue(Of Object))) As Object

        End Function

        Public Shared ReadOnly Property Rsharp As New Interpreter

        Public Shared Function Evaluate(script$, ParamArray args As NamedValue(Of Object)()) As Object
            SyncLock Rsharp
                With Rsharp
                    If Not args.IsNullOrEmpty Then
                        For Each x In args
                            Call .globalEnvir.Push(x.Name, x.Value, NameOf(TypeCodes.generic))
                        Next
                    End If

                    Return .Evaluate(script)
                End With
            End SyncLock
        End Function
    End Class
End Namespace