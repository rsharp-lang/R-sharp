
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Annotation

    ''' <summary>
    ''' ``@profile``
    ''' 
    ''' 开始进行性能计数
    ''' </summary>
    Public Class Profiler : Inherits Expression
        Implements IRuntimeTrace

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
        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Sub New(evaluate As Expression, sourceMap As StackFrame)
            target = evaluate
            stackFrame = sourceMap
        End Sub

        Public Overrides Function ToString() As String
            Return $"@profile -> ( {target.ToString} )"
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim openProfiler As New Environment(
                parent:=envir,
                stackFrame:=stackFrame,
                isInherits:=False,
                openProfiler:=True
            )
            Dim result As Object = target.Evaluate(openProfiler)

            Return result
        End Function
    End Class
End Namespace