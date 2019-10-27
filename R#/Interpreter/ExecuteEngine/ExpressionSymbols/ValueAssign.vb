Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public Class ValueAssign : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        ''' <summary>
        ''' 可能是对tuple做赋值
        ''' 所以应该是多个变量名称
        ''' </summary>
        Dim targetSymbols As String()
        Dim isByRef As Boolean
        Dim value As Expression

        Sub New(tokens As List(Of Token()))
            targetSymbols = DeclareNewVariable.getNames(tokens(Scan0))
            isByRef = tokens(Scan0)(Scan0).text = "="
            value = tokens.Skip(2).AsList.DoCall(AddressOf Expression.CreateExpression)
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim value As Object = Me.value.Evaluate(envir)

            If targetSymbols.Length = 1 Then
                Call assignSymbol(envir(targetSymbols(Scan0)), value)
            Else
                ' assign tuples
                Call assignTuples(envir, value)
            End If

            Return value
        End Function

        Private Sub assignTuples(envir As Environment, value As Object)
            If value.GetType.IsInheritsFrom(GetType(Array)) Then
                Dim array As Array = value

                If array.Length = 1 Then
                    ' all assign the same value result
                    For Each name As String In targetSymbols
                        Call assignSymbol(envir(name), value)
                    Next
                ElseIf array.Length = targetSymbols.Length Then
                    ' one by one
                Else
                    ' 数量不对
                    Throw New InvalidCastException
                End If
            Else
                Throw New NotImplementedException
            End If
        End Sub

        Private Sub assignSymbol(target As Variable, value As Object)
            If isByRef Then
                target.value = value
            Else
                If value.GetType.IsInheritsFrom(GetType(Array)) Then
                    target.value = DirectCast(value, Array).Clone
                Else
                    target.value = value
                End If
            End If
        End Sub
    End Class
End Namespace