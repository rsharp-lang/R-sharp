Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Python.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser.SyntaxImplements
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components


Public Class FunctionTag : Inherits TaggedObject

    Public Property funcname As String
    Public Property arguments As Expression()
    Public Property stackframe As StackFrame

    Public Overrides Function ToExpression(release As Index(Of String)) As Expression
        Return New DeclareNewFunction(funcname, arguments, MyBase.ToExpression(release), stackframe)
    End Function

End Class