Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    Public Class UnaryNumeric : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.double
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.UnaryNot
            End Get
        End Property

        Dim [operator] As String
        ''' <summary>
        ''' 可能是一个符号，也可以能是一个对象引用
        ''' </summary>
        Dim numeric As Expression

        Public Overrides Function Evaluate(envir As Environment) As Object
            Throw New NotImplementedException()
        End Function

        Public Overrides Function ToString() As String
            Return $"{[operator]}{numeric}"
        End Function
    End Class
End Namespace