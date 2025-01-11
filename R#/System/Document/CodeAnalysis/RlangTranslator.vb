﻿#Region "Microsoft.VisualBasic::910aa4cbd526842686e84629f63f6ceb, R#\System\Document\CodeAnalysis\RlangTranslator.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:


' Code Statistics:

'   Total Lines: 333
'    Code Lines: 258 (77.48%)
' Comment Lines: 17 (5.11%)
'    - Xml Docs: 76.47%
' 
'   Blank Lines: 58 (17.42%)
'     File Size: 13.77 KB


'     Class RlangTranslator
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: AssignNewSymbol, castLiteral, CreateSymbols, GetAssignValue, GetBinaryOp
'                   getByref, GetElse, getForLoop, GetFunctionInvoke, GetIf
'                   (+2 Overloads) GetScript, GetSymbol, GetSymbolIndexSubset, GetUnaryNot, (+2 Overloads) Literal
'                   Vector
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language.TokenIcer
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
        ''' <summary>
        ''' filtering of the parent symbols
        ''' </summary>
        ReadOnly filters As New Index(Of String)
        ReadOnly indent As Integer = 0

        Const CreateSymbol As String = "<create_new_symbol>;"

        Sub New(closure As ClosureExpression)
            lines = closure.program.ToArray
        End Sub

        Private Sub New(closure As ClosureExpression, symbols As Dictionary(Of String, String), indent As Integer)
            Me.symbols = New Dictionary(Of String, String)(symbols)
            Me.lines = closure.program.ToArray
            Me.indent = indent
            Me.filters = symbols.Keys.Indexing

            If lines.Length = 1 AndAlso TypeOf lines(0) Is ClosureExpression Then
                lines = DirectCast(lines(0), ClosureExpression).program.ToArray
            End If
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
            Dim indent As String = New String(" "c, Me.indent)

            For Each line As Expression In lines
                Dim appendTerminator As Boolean = Not (
                    TypeOf line Is IfBranch OrElse
                    TypeOf line Is ElseBranch OrElse
                    TypeOf line Is ForLoop
                )
                Dim line_translate As String = GetScript(line, env)

                line_translate = line_translate _
                    .LineTokens _
                    .Select(Function(si) indent & si) _
                    .JoinBy(vbCrLf)

                If TypeOf line Is ElseBranch OrElse TypeOf line Is ElseIfBranch Then
                    ' else should append after {
                    ' not in new line
                    ' or syntax error in R langhage
                    script(script.Count - 1) = script.Last & line_translate
                Else
                    If appendTerminator Then
                        Call script.Add(line_translate & ";")
                    Else
                        Call script.Add(line_translate)
                    End If
                End If
            Next

            Return CreateSymbols _
                .JoinIterates(script) _
                .JoinBy(vbCrLf)
        End Function

        Private Iterator Function CreateSymbols() As IEnumerable(Of String)
            For Each symbol In symbols
                If symbol.Value <> CreateSymbol Then
                    If Not symbol.Key Like filters Then
                        Yield $"{symbol.Key} <- {symbol.Value};"
                    End If
                End If
            Next
        End Function

        Private Function GetScript(line As Expression, env As Environment) As String
            Select Case line.GetType
                Case GetType(FunctionInvoke) : Return GetFunctionInvoke(line, env)
                Case GetType(SymbolReference) : Return GetSymbol(line, env)
                Case GetType(ValueAssignExpression) : Return GetAssignValue(line, env)
                Case GetType(DeclareNewSymbol) : Return AssignNewSymbol(line, env)
                Case GetType(DeclareNewFunction) : Return createFunction(line, env)
                Case GetType(VectorLiteral) : Return Vector(line, env)
                Case GetType(Literal) : Return Literal(line, env)
                Case GetType(IfBranch) : Return GetIf(line, env)
                Case GetType(ElseBranch) : Return GetElse(line, env)
                Case GetType(BinaryExpression), GetType(BinaryInExpression)
                    Return GetBinaryOp(line, env)
                Case GetType(SymbolIndexer) : Return GetSymbolIndexSubset(line, env)
                Case GetType(Operators.UnaryNot) : Return GetUnaryNot(line, env)
                Case GetType(ByRefFunctionCall) : Return getByref(line, env)
                Case GetType(ForLoop) : Return getForLoop(line, env)
                Case GetType(MemberValueAssign) : Return getMemberValueAssign(line, env)
                Case GetType(FormulaExpression) : Return getFormulaString(line, env)
                Case GetType(Require) : Return requirePkg(line, env)
                Case GetType(SequenceLiteral) : Return getSequence(line, env)
                Case GetType(UnaryNumeric) : Return getNeg(line, env)

                Case Else
                    Dim expr_clr As String = line.GetType.Name
                    Dim rsharp_str As String = line.ToString
                    Dim msg_err As String = $"{expr_clr}: {rsharp_str}"

                    Throw New NotImplementedException(msg_err)
            End Select
        End Function

        Private Function getNeg(neg As UnaryNumeric, env As Environment) As String
            Dim val As String = GetScript(neg.numeric, env)
            Return $"{neg.operator}{val}"
        End Function

        Private Function getSequence(seq As SequenceLiteral, env As Environment) As String
            Dim from As String = GetScript(seq.from, env)
            Dim [to] As String = GetScript(seq.to, env)
            Dim [by] As String = GetScript(seq.steps, env)

            If by <> "1" Then
                Return $"seq(from = {from}, to = {[to]}, by = {by})"
            Else
                Return $"{from}:{[to]}"
            End If
        End Function

        Private Function requirePkg(require As Require, env As Environment) As String
            Dim pkgs = require.packages.Select(Function(p) GetScript(p, env)).ToArray

            If require.options.IsNullOrEmpty Then
                Return $"require({pkgs.JoinBy(", ")})"
            Else
                Dim opts As String() = require.options _
                    .Select(Function(a) GetScript(a, env)) _
                    .ToArray

                Return $"require({pkgs.JoinBy(", ")}, {opts.JoinBy(", ")})"
            End If
        End Function

        Private Function getFormulaString(f As FormulaExpression, env As Environment) As String
            Dim response As String = GetScript(f.var, env)
            Dim data As String = GetScript(f.formula, env)

            Return $"{response} ~ {data}"
        End Function

        Private Function createFunction(f As DeclareNewFunction, env As Environment) As String
            Dim name As String = f.funcName
            Dim args = f.parameters _
                .Select(Function(a)
                            If a.hasInitializeExpression Then
                                Return $"{a.GetSymbolName} = {GetScript(a.value, env)}"
                            Else
                                Return a.GetSymbolName
                            End If
                        End Function) _
                .ToArray
            Dim inner As New Dictionary(Of String, String)(symbols)
            Dim run As String

            For Each arg As DeclareNewSymbol In f.parameters
                inner(arg.GetSymbolName) = CreateSymbol
            Next

            inner(name) = CreateSymbol
            run = New RlangTranslator(f.body, inner, indent + 3).GetScript(env)

            Dim func_body = $"function({args.JoinBy(", ")}) {{
{run}
}}"

            If name.StringEmpty OrElse Not Scanner.CheckIdentifierSymbol(name) Then
                Return func_body
            Else
                Return $"{name} = {func_body}"
            End If
        End Function

        Private Function getForLoop(forLoop As ForLoop, env As Environment) As String
            Dim x As String = forLoop.variables(0)
            Dim seq As String = GetScript(forLoop.sequence, env)
            Dim inner As New Dictionary(Of String, String)(symbols)
            Dim run As String

            inner(x) = CreateSymbol
            run = New RlangTranslator(forLoop.body.body, inner, indent + 3).GetScript(env)

            Return $"for({x} in {seq}) {{
{run}
            }}"
        End Function

        Private Function getMemberValueAssign(line As MemberValueAssign, env As Environment) As String
            Dim target = line.memberReference
            Dim symbol = GetScript(target.symbol, env)
            Dim index = GetScript(target.index, env)
            Dim val_str = GetScript(line.value, env)

            If target.indexType = SymbolIndexers.dataframeColumns Then
                Return $"{symbol}[,{index}]<-{val_str};"
            ElseIf target.indexType = SymbolIndexers.nameIndex Then
                Return $"{symbol}[[{index}]]<-{val_str};"
            ElseIf target.indexType = SymbolIndexers.vectorIndex Then
                Return $"{symbol}[{index}]<-{val_str};"
            Else
                Throw New NotImplementedException($"{target.indexType.Description}: {line.ToString}")
            End If
        End Function

        Private Function getByref(line As ByRefFunctionCall, env As Environment) As String
            Dim byref_call As String = GetScript(line.ConstructByrefCall, env)
            Dim value As String = GetScript(line.value, env)
            Return $"{byref_call} <- {value}"
        End Function

        Private Function GetUnaryNot(unary_not As Operators.UnaryNot, env As Environment) As String
            Dim script As String = GetScript(unary_not.logical, env)
            script = $"!({script})"
            Return script
        End Function

        Private Function GetSymbolIndexSubset(line As SymbolIndexer, env As Environment) As String
            Dim indexer = GetScript(line.index, env)
            Dim symbol = ValueAssignExpression.GetSymbol(line.symbol)
            Dim script As String
            Dim is_symbol As Boolean = Scanner.CheckIdentifierSymbol(indexer.Trim("'"c, """"c))
            Dim symbolName As String = indexer.Trim("'"c, """"c)

            If Not is_symbol Then
                If line.indexType = SymbolIndexers.nameIndex AndAlso symbolName.IndexOf("$") > 0 Then
                    is_symbol = True
                End If
            End If

            Select Case line.indexType
                Case SymbolIndexers.dataframeColumns : script = $"{symbol}[, {indexer}]"
                Case SymbolIndexers.dataframeRows : script = $"{symbol}[{indexer}, ]"
                Case SymbolIndexers.nameIndex
                    If is_symbol Then
                        If symbolName.IndexOf("$") > 0 Then
                            ' `a$b`
                            script = $"{symbol}$`{symbolName}`"
                        Else
                            script = $"{symbol}${symbolName}"
                        End If
                    Else
                        script = $"{symbol}[[{indexer}]]"
                    End If
                Case SymbolIndexers.vectorIndex : script = $"{symbol}[{indexer}]"
                Case SymbolIndexers.dataframeRanges
                    ' a[1,,drop=TRUE]
                    If TypeOf line.index Is VectorLiteral AndAlso DirectCast(line.index, VectorLiteral).Skip(1).All(Function(a) TypeOf a Is ValueAssignExpression) Then
                        Dim index As VectorLiteral = line.index
                        Dim rowIndex As String = GetScript(index(0), env)
                        Dim opt As String = index.Skip(1).Select(Function(a) GetScript(a, env)).JoinBy(", ")

                        script = $"{symbol}[{rowIndex},,{opt}]"
                    Else
                        Throw New NotImplementedException(line.indexType.ToString & ": " & line.index.ToString)
                    End If
                Case Else
                    Throw New NotImplementedException(line.indexType.ToString)
            End Select

            Return script
        End Function

        Private Function GetBinaryOp(bin As IBinaryExpression, env As Environment) As String
            Dim left = GetScript(bin.left, env)
            Dim right = GetScript(bin.right, env)
            Dim op As String = bin.operator

            If op = "in" Then
                op = "%in%"
            End If

            Dim script As String = $"{left} {op} {right}"

            Return script
        End Function

        Private Function GetElse(else_branch As ElseBranch, env As Environment) As String
            Dim closure As String = New RlangTranslator(else_branch.closure.body, symbols, indent + 3).GetScript(env)

            Return $"else {{
{closure}
}}"
        End Function

        Private Function GetIf(if_branch As IfBranch, env As Environment) As String
            Dim test As String = GetScript(if_branch.ifTest, env)
            Dim closure As String = New RlangTranslator(if_branch.trueClosure.body, symbols, indent + 3).GetScript(env)

            Return $"if( {test} ) {{
{closure}
}}"
        End Function

        Private Function Literal(val As Literal, env As Environment) As String
            Dim value As Object = val.Evaluate(env)

            If value Is Nothing Then
                Return "NULL"
            ElseIf value Is GetType(Void) Then
                Return "NA"
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
            Dim left = assign.targetSymbols

            If left.Length > 1 Then
                Throw New NotImplementedException($"tuple deconstructor is not implements in R language: {assign}.")
            End If

            Dim symbol = assign.targetSymbols(0)
            Dim symbol_str As String
            Dim value As String = GetScript(assign.value, env)

            If TypeOf symbol Is SymbolIndexer Then
                symbol_str = GetScript(symbol, env)
            Else
                symbol_str = ValueAssignExpression.GetSymbol(symbol)
            End If

            If Not Scanner.CheckIdentifierSymbol(symbol_str) Then
                If Not TypeOf symbol Is SymbolIndexer Then
                    symbol_str = $"""{symbol_str}"""
                End If
            End If

            Return $"{symbol_str} = {value}"
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
                        Dim castError As Boolean

                        descriptor = castLiteral(val, val.typeCode, castError)

                        If castError Then
                            descriptor = castLiteral(val, val.TryGetValueType, castError)
                        End If
                        If castError Then
                            If val.typeCode = TypeCodes.closure Then
                                ' use the function name as symbol reference
                                ' example as pass the function name as parameter value
                                ' sapply(m,1,sd);
                                descriptor = val.name
                                Call $"closure symbol '{descriptor}' has been used as the parameter value.".Warning
                            Else
                                Throw New NotImplementedException(val.typeCode.ToString)
                            End If
                        End If

                        Call symbols.Add(name, descriptor)
                    End If
                End If
            End If

            Return name
        End Function

        Private Shared Function castLiteral(val As Symbol, code As TypeCodes, ByRef castError As Boolean) As String
            castError = False

            Select Case code
                Case TypeCodes.boolean
                    Return Literal(CLRVector.asLogical(val.value).Select(Function(b) b.ToString.ToUpper))
                Case TypeCodes.double, TypeCodes.integer, TypeCodes.raw
                    Return Literal(CLRVector.asNumeric(val.value).SafeQuery.Select(Function(n) n.ToString))
                Case TypeCodes.string
                    Return Literal(CLRVector.asCharacter(val.value).Select(Function(str) $"'{str}'"))

                Case Else
                    castError = True
            End Select

            Return Nothing
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
            Dim f As String = ValueAssignExpression.GetSymbol(calls.funcName)

            If Not calls.namespace.StringEmpty(, True) Then
                f = $"{calls.namespace}::{f}"
            End If

            If f = "require" OrElse f = "library" Then
                ' load packages
                For Each par As Expression In pars
                    If TypeOf par Is Literal Then
                        Call parList.Add(GetScript(par, env))
                    ElseIf TypeOf par Is SymbolReference Then
                        Call parList.Add(ValueAssignExpression.GetSymbol(par))
                    Else
                        Throw New NotImplementedException($"can not extract package name from expression of type: {par.GetType.FullName}")
                    End If
                Next
            Else
                For Each par As Expression In pars
                    Call parList.Add(GetScript(par, env))
                Next
            End If

            Return $"{f}({parList.JoinBy(", ")})"
        End Function

    End Class
End Namespace
