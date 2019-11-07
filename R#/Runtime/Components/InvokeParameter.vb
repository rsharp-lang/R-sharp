Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Runtime.Components

    Public Class InvokeParameter

        Public Property value As Expression

        Public ReadOnly Property name As String
            Get
                If value Is Nothing Then
                    Return ""
                ElseIf TypeOf value Is SymbolReference Then
                    Return DirectCast(value, SymbolReference).symbol
                ElseIf TypeOf value Is ValueAssign Then
                    Return DirectCast(value, ValueAssign).targetSymbols(Scan0)
                Else
                    Return value.ToString
                End If
            End Get
        End Property

        Public ReadOnly Property haveSymbolName As Boolean
            Get
                If value Is Nothing Then
                    Return False
                ElseIf (TypeOf value Is SymbolReference OrElse TypeOf value Is ValueAssign) Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Evaluate(envir As Environment) As Object
            If value Is Nothing Then
                Return Nothing
            ElseIf Not TypeOf value Is ValueAssign Then
                Return value.Evaluate(envir)
            Else
                Return DirectCast(value, ValueAssign).value.Evaluate(envir)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return name
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Create(expressions As IEnumerable(Of Expression)) As InvokeParameter()
            Return expressions _
                .Select(Function(e)
                            Return New InvokeParameter With {
                                .value = e
                            }
                        End Function) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateArguments(envir As Environment, arguments As IEnumerable(Of InvokeParameter)) As Dictionary(Of String, Object)
            Return arguments _
                .SeqIterator _
                .ToDictionary(Function(a)
                                  If a.value.haveSymbolName Then
                                      Return a.value.name
                                  Else
                                      Return "$" & a.i
                                  End If
                              End Function,
                              Function(a)
                                  Return a.value.Evaluate(envir)
                              End Function)
        End Function
    End Class
End Namespace