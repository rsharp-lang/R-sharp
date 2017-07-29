Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter

    ''' <summary>
    ''' The R# language interpreter
    ''' </summary>
    Public Class Interpreter

        ''' <summary>
        ''' Global runtime environment.(全局环境)
        ''' </summary>
        Public ReadOnly Property globalEnvir As New Environment

        ''' <summary>
        ''' Run R# script program from text data.
        ''' </summary>
        ''' <param name="script$"></param>
        ''' <returns></returns>
        Public Function Evaluate(script$) As Object
            Return Codes.TryParse(script).RunProgram(globalEnvir)
        End Function

        ''' <summary>
        ''' Run script file.
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
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