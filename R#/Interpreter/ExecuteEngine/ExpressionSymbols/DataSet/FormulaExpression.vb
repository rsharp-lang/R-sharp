Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    Public Class FormulaExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.formula
            End Get
        End Property

        ''' <summary>
        ''' 因变量
        ''' </summary>
        Public ReadOnly Property var As String

        ''' <summary>
        ''' 只能够是符号引用或者双目运算符表达式
        ''' </summary>
        Public ReadOnly Property formula As Expression

        Sub New(y As String, formula As Expression)
            Me.var = y
            Me.formula = formula
        End Sub

        ''' <summary>
        ''' get all symbol names from <see cref="formula"/>
        ''' </summary>
        ''' <returns></returns>
        Public Function GetSymbols(env As Environment) As Object
            Dim result = GetSymbols(formula)

            If result Like GetType(String()) Then
                Return result.TryCast(Of String())
            Else
                Return Internal.debug.stop(result.TryCast(Of Exception), env)
            End If
        End Function

        Public Shared Function GetSymbols(formula As Expression) As [Variant](Of String(), Exception)
            If TypeOf formula Is SymbolReference Then
                Return {DirectCast(formula, SymbolReference).symbol}
            ElseIf TypeOf formula Is BinaryExpression Then
                Dim bin As BinaryExpression = DirectCast(formula, BinaryExpression)
                Dim result As New List(Of String)
                Dim tmp As [Variant](Of String(), Exception)

                tmp = GetSymbols(bin.left)

                If Not tmp Like GetType(String()) Then
                    Return tmp
                Else
                    result.AddRange(tmp.TryCast(Of String()))
                End If

                tmp = GetSymbols(bin.right)

                If Not tmp Like GetType(String()) Then
                    Return tmp
                Else
                    result.AddRange(tmp.TryCast(Of String()))
                End If

                Return result.ToArray
            Else
                Return New ArgumentException($"{formula}: expression type {formula.GetType} is invalid! required symbol reference or binary expression!")
            End If
        End Function

        ''' <summary>
        ''' 这个只是用来表述关系的，并不会产生内容
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return $"{var} ~ {formula}"
        End Function
    End Class
End Namespace