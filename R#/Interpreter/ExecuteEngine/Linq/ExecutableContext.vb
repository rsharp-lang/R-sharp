Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class ExecutableContext

        ReadOnly environment As Environment

        Public ReadOnly Property stackFrame As StackFrame
            Get
                Return environment.stackFrame
            End Get
        End Property

        Sub New(env As Environment)
            environment = env
        End Sub

        Public Sub AddSymbol(symbolName$, type As TypeCodes)
            Call environment.Push(symbolName, Nothing, [readonly]:=False, mode:=type)
        End Sub

        Public Function FindSymbol(symbolName As String) As Symbol
            Return environment.FindSymbol(symbolName)
        End Function

        Public Overrides Function ToString() As String
            Return environment.ToString
        End Function

        Public Shared Narrowing Operator CType(context As ExecutableContext) As Environment
            Return context.environment
        End Operator
    End Class
End Namespace