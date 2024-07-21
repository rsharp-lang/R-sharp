Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
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
        ''' <summary>
        ''' filtering of the parent symbols
        ''' </summary>
        ReadOnly filters As New Index(Of String)

        Const CreateSymbol As String = "<create_new_symbol>;"

        Sub New(closure As ClosureExpression)
            lines = closure.program.ToArray
        End Sub

        Private Sub New(closure As ClosureExpression, symbols As Dictionary(Of String, String))
            Me.symbols = New Dictionary(Of String, String)(symbols)
            Me.lines = closure.program.ToArray
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
                Case GetType(VectorLiteral) : Return Vector(line, env)
                Case GetType(Literal) : Return Literal(line, env)
                Case GetType(IfBranch) : Return GetIf(line, env)
                Case GetType(ElseBranch) : Return GetElse(line, env)
                Case GetType(BinaryExpression), GetType(BinaryInExpression)
                    Return GetBinaryOp(line, env)
                Case GetType(SymbolIndexer) : Return GetSymbolIndexSubset(line, env)
                Case GetType(Operators.UnaryNot) : Return GetUnaryNot(line, env)

                Case Else
                    Throw New NotImplementedException(line.GetType.FullName)
            End Select
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

            Select Case line.indexType
                Case SymbolIndexers.dataframeColumns : script = $"{symbol}[, {indexer}]"
                Case SymbolIndexers.dataframeRows : script = $"{symbol}[{indexer}, ]"
                Case SymbolIndexers.nameIndex : script = $"{symbol}[[{indexer}]]"
                Case SymbolIndexers.vectorIndex : script = $"{symbol}[{indexer}]"
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
            Dim closure As String = New RlangTranslator(else_branch.closure.body, symbols).GetScript(env)

            Return $"else {{
{closure}
}}"
        End Function

        Private Function GetIf(if_branch As IfBranch, env As Environment) As String
            Dim test As String = GetScript(if_branch.ifTest, env)
            Dim closure As String = New RlangTranslator(if_branch.trueClosure.body, symbols).GetScript(env)

            Return $"if( {test} ) {{
{closure}
}}"
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
                        Dim castError As Boolean

                        descriptor = castLiteral(val, val.typeCode, castError)

                        If castError Then
                            descriptor = castLiteral(val, val.TryGetValueType, castError)
                        End If
                        If castError Then
                            Throw New NotImplementedException(val.typeCode.ToString)
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