
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Annotation

    ''' <summary>
    ''' ``@profile``
    ''' 
    ''' 开始进行性能计数
    ''' </summary>
    Public Class Profiler : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.generic
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Annotation
            End Get
        End Property

        ''' <summary>
        ''' target expression for run profiler
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property target As Expression

        Sub New(evaluate As Expression)
            target = evaluate
        End Sub

        Public Overrides Function ToString() As String
            Return $"@profile -> ( {target.ToString} )"
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace