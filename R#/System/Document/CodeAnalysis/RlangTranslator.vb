﻿Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Development.CodeAnalysis

    ''' <summary>
    ''' helper module for translate the R# code to R language
    ''' </summary>
    Public Class RlangTranslator

        ReadOnly lines As Expression()
        ReadOnly symbols As New Dictionary(Of String, String)

        Const CreateSymbol As String = "<create_new_symbol>;"

        Sub New(closure As ClosureExpression)
            lines = closure.program.ToArray
        End Sub

        ''' <summary>
        ''' get R language script
        ''' </summary>
        ''' <param name="env">
        ''' the runtime environment for extract the symbol values for run the script
        ''' </param>
        ''' <returns></returns>
        Public Function GetScript(env As Environment) As String
            Dim script As New List(Of String)

            For Each line As Expression In lines
                Call script.Add(GetScript(line, env) & ";")
            Next

            Return CreateSymbols _
                .JoinIterates(script) _
                .JoinBy(vbCrLf)
        End Function

        Private Iterator Function CreateSymbols() As IEnumerable(Of String)
            For Each symbol In symbols
                If symbol.Value <> CreateSymbol Then
                    Yield $"{symbol.Key} <- {symbol.Value};"
                End If
            Next
        End Function

        Private Function GetScript(line As Expression, env As Environment) As String
            Select Case line.GetType
                Case GetType(FunctionInvoke) : Return GetFunctionInvoke(line, env)
                Case GetType(SymbolReference) : Return GetSymbol(line, env)
                Case GetType(ValueAssignExpression) : Return GetAssignValue(line, env)
                Case GetType(DeclareNewSymbol) : Return AssignNewSymbol(line, env)
                Case GetType(VectorLiteral) : Return Vector(line, env)
                Case GetType(Literal) : Return Literal(line, env)

                Case Else
                    Throw New NotImplementedException(line.GetType.FullName)
            End Select
        End Function

        Private Function Literal(val As Literal, env As Environment) As String
            Dim value As Object = val.Evaluate(env)

            If value Is Nothing Then
                Return "NULL"
            Else
                If TypeOf value Is String Then
                    Return $"'{value}'"
                ElseIf TypeOf value Is Boolean Then
                    Return value.ToString.ToUpper
                Else
                    Return value.ToString
                End If
            End If
        End Function

        Private Function Vector(vec As VectorLiteral, env As Environment) As String
            Dim vals As String() = vec.Select(Function(e) GetScript(e, env)).ToArray
            Dim vec_code As String = Literal(vals)

            Return vec_code
        End Function

        Private Function AssignNewSymbol(create As DeclareNewSymbol, env As Environment) As String
            Dim value As String = GetScript(create.value, env)
            Dim symbols As String() = create.names.ToArray

            If symbols.Length > 1 Then
                Throw New NotImplementedException("tuple deconstructor is not implements in R language.")
            Else
                Call Me.symbols.Add(symbols(0), CreateSymbol)
            End If

            Return $"{symbols(0)} = {value}"
        End Function

        Private Function GetAssignValue(assign As ValueAssignExpression, env As Environment) As String
            Dim symbols As String() = assign.targetSymbols _
                .Select(Function(a) ValueAssignExpression.GetSymbol(a)) _
                .ToArray
            Dim value As String = GetScript(assign.value, env)

            If symbols.Length > 1 Then
                Throw New NotImplementedException("tuple deconstructor is not implements in R language.")
            End If

            Return $"{symbols(0)} = {value}"
        End Function

        Private Function GetSymbol(line As SymbolReference, env As Environment) As String
            Dim name As String = ValueAssignExpression.GetSymbol(line)

            If Not symbols.ContainsKey(name) Then
                Dim val As Symbol = env.FindSymbol(name)

                If val Is Nothing Then
                    Throw New MissingPrimaryKeyException($"missing of the symbol: {name}")
                Else
                    If val.value Is Nothing Then
                        symbols.Add(name, "NULL")
                    Else
                        Dim descriptor As String

                        Select Case val.typeCode
                            Case TypeCodes.boolean
                                descriptor = Literal(CLRVector.asLogical(val.value).Select(Function(b) b.ToString.ToUpper))
                            Case TypeCodes.double, TypeCodes.integer, TypeCodes.raw
                                descriptor = Literal(CLRVector.asNumeric(val.value))
                            Case TypeCodes.string
                                descriptor = Literal(CLRVector.asCharacter(val.value).Select(Function(str) $"'{str}'"))

                            Case Else
                                Throw New NotImplementedException(val.typeCode.ToString)
                        End Select

                        Call symbols.Add(name, descriptor)
                    End If
                End If
            End If

            Return name
        End Function

        Private Shared Function Literal(vals As IEnumerable(Of String)) As String
            Dim vec = vals.ToArray

            If vec.Length = 0 Then
                Return "NULL"
            ElseIf vec.Length = 1 Then
                Return vec(0)
            Else
                Return $"c({vec.JoinBy(", ")})"
            End If
        End Function

        Private Function GetFunctionInvoke(calls As FunctionInvoke, env As Environment) As String
            Dim pars = calls.parameters
            Dim parList As New List(Of String)

            For Each par As Expression In pars
                Call parList.Add(GetScript(par, env))
            Next

            Dim f As String = ValueAssignExpression.GetSymbol(calls.funcName)

            If Not calls.namespace.StringEmpty(, True) Then
                f = $"{calls.namespace}::{f}"
            End If

            Return $"{f}({parList.JoinBy(", ")})"
        End Function

    End Class
End Namespace