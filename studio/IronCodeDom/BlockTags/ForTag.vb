Imports System.Data
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components


Public Class ForTag : Inherits PythonCodeDOM

    Public Property vars As Expression()
    Public Property data As Expression
    Public Property stackFrame As StackFrame

    Public Overrides Function ToExpression() As Expression
        Dim varNames As String() = vars _
            .Select(Function(v)
                        Return ValueAssignExpression.GetSymbol(v)
                    End Function) _
            .ToArray
        Dim varSymbols = varNames.Select(Function(s) New DeclareNewSymbol({s}, Nothing, TypeCodes.generic, [readonly]:=False, stackFrame)).ToArray
        Dim loopBody As New DeclareNewFunction("for_loop", varSymbols, MyBase.ToExpression(), stackFrame)

        Return New ForLoop(varNames, data, loopBody, False, stackFrame)
    End Function

End Class