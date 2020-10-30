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