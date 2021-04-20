Imports System.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components
Imports r = System.Text.RegularExpressions.Regex

Namespace Development

    Friend Class ArgumentInfo

        Friend attrs As New Dictionary(Of String, String())
        Friend type As TypeCodes = TypeCodes.string

        Default Public ReadOnly Property Item(name As String) As String
            Get
                Return attrs.TryGetValue("info").JoinBy(";" & vbCrLf)
            End Get
        End Property

    End Class

    Friend Class CommandLineArgument

        Public Property name As String
        Public Property defaultValue As String
        Public Property type As String
        Public Property description As String

    End Class

    ''' <summary>
    ''' the R# shell script commandline arguments helper module
    ''' </summary>
    Public Class ShellScript

        ReadOnly Rscript As Program
        ReadOnly arguments As New List(Of CommandLineArgument)
        ReadOnly sourceScript As String
        ReadOnly info As String = "<No description provided.>"
        ReadOnly title As String

        Public ReadOnly Property message As String

        Sub New(Rscript As Rscript)
            Dim metaLines As String() = r.Matches(Rscript.script, "^#.+?$", RegexICMul).ToArray
            Dim meta = parseMetaData(metaLines)

            Me.Rscript = Program.CreateProgram(Rscript, [error]:=message)
            Me.sourceScript = Rscript.fileName

            Me.title = meta.TryGetValue("title") Or sourceScript.BaseName.AsDefault

            If meta.ContainsKey("description") Then
                info = meta!description
            End If
        End Sub

        Private Shared Function parseMetaData(meta As String()) As Dictionary(Of String, String)
            Dim text As String() = meta _
                .Select(Function(line)
                            Return line.Trim(" "c, "#"c, ASCII.CR, ASCII.LF)
                        End Function) _
                .ToArray
            Dim data As Dictionary(Of String, String) = text.ParseTagData

            Return data
        End Function

        Public Sub PrintUsage(dev As TextWriter)
            Dim cli As New List(Of String)
            Dim maxName As String = arguments.Select(Function(a) a.name).MaxLengthString

            Call dev.WriteLine()
            Call dev.WriteLine($"  '{sourceScript}' - {title}")
            Call dev.WriteLine()

            For Each line As String In info.LineTokens
                Call dev.WriteLine("  " & line)
            Next

            Call dev.WriteLine()

            For Each arg As CommandLineArgument In arguments
                If arg.defaultValue.StartsWith("<required") Then
                    cli.Add($"{arg.name} <{arg.type}>")
                Else
                    cli.Add($"[{arg.name} <{arg.type}, default={arg.defaultValue}>]")
                End If
            Next

            Call dev.WriteLine($"SYNOPSIS")
            Call dev.WriteLine($"Rscript ""{sourceScript}"" {cli.JoinBy(" ")}")
            Call dev.WriteLine()
            Call dev.WriteLine("CommandLine Argument Values:")

            Call dev.WriteLine()

            Static none As [Default](Of String) = "-"

            For Each arg As CommandLineArgument In arguments
                Call dev.WriteLine($" {arg.name}: {New String(" "c, maxName.Length - arg.name.Length)}{arg.description Or none}")
            Next

            Call dev.Flush()
        End Sub

        Private Sub AnalysisTree(expr As Expression, attrs As ArgumentInfo)
            If expr Is Nothing OrElse
                TypeOf expr Is Literal OrElse
                TypeOf expr Is SymbolReference Then

                Return
            End If

            Select Case expr.GetType
                Case GetType([Imports]) : Call analysisTree(DirectCast(expr, [Imports]), attrs)
                Case GetType(BinaryOrExpression) : Call analysisTree(DirectCast(expr, BinaryOrExpression), attrs)
                Case GetType(DeclareNewSymbol) : Call analysisTree(DirectCast(expr, DeclareNewSymbol))
                Case GetType(FunctionInvoke) : Call analysisTree(DirectCast(expr, FunctionInvoke), attrs)
                Case GetType(IfBranch) : Call analysisTree(DirectCast(expr, IfBranch), attrs)
                Case GetType(ClosureExpression) : Call analysisTree(DirectCast(expr, ClosureExpression), attrs)
                Case GetType(SymbolIndexer) : Call analysisTree(DirectCast(expr, SymbolIndexer), attrs)
                Case GetType(VectorLiteral) : Call analysisTree(DirectCast(expr, VectorLiteral), attrs)
                Case GetType(BinaryExpression) : Call analysisTree(DirectCast(expr, BinaryExpression), attrs)
                Case GetType(BinaryInExpression) : Call analysisTree(DirectCast(expr, BinaryInExpression), attrs)
                Case GetType(ElseBranch) : Call analysisTree(DirectCast(expr, ElseBranch), attrs)
                Case GetType(StringInterpolation) : Call analysisTree(DirectCast(expr, StringInterpolation), attrs)
                Case GetType(ValueAssign) : Call analysisTree(DirectCast(expr, ValueAssign), attrs)
                Case GetType(DeclareNewFunction) : Call analysisTree(DirectCast(expr, DeclareNewFunction), attrs)
                Case GetType(ForLoop) : Call analysisTree(DirectCast(expr, ForLoop), attrs)
                Case GetType(ReturnValue) : Call analysisTree(DirectCast(expr, ReturnValue), attrs)
                Case GetType(DeclareLambdaFunction) : Call analysisTree(DirectCast(expr, DeclareLambdaFunction), attrs)
                Case GetType(AppendOperator) : Call analysisTree(DirectCast(expr, AppendOperator), attrs)
                Case GetType(UnaryNot) : Call analysisTree(DirectCast(expr, UnaryNot), attrs)
                Case GetType(SequenceLiteral) : Call analysisTree(DirectCast(expr, SequenceLiteral), attrs)

                Case Else
                    Throw New NotImplementedException(expr.GetType.FullName)
            End Select
        End Sub

        Private Sub analysisTree(expr As SequenceLiteral, attrs As ArgumentInfo)
            Call AnalysisTree(expr.from, attrs)
            Call AnalysisTree(expr.to, attrs)
            Call AnalysisTree(expr.steps, attrs)
        End Sub

        Private Sub analysisTree(expr As UnaryNot, attrs As ArgumentInfo)
            Call AnalysisTree(expr.logical, attrs)
        End Sub

        Private Sub analysisTree(expr As AppendOperator, attrs As ArgumentInfo)
            Call AnalysisTree(expr.target, attrs)
            Call AnalysisTree(expr.appendData, attrs)
        End Sub

        Private Sub analysisTree(expr As DeclareLambdaFunction, attrs As ArgumentInfo)
            Call AnalysisTree(expr.closure, attrs)
        End Sub

        Private Sub analysisTree(expr As ReturnValue, attrs As ArgumentInfo)
            Call AnalysisTree(expr.value, attrs)
        End Sub

        Private Sub analysisTree(expr As ForLoop, attrs As ArgumentInfo)
            Call AnalysisTree(expr.sequence, attrs)
            Call analysisTree(expr.body.body, attrs)
        End Sub

        Private Sub analysisTree(expr As DeclareNewFunction, attrs As ArgumentInfo)
            For Each arg In expr.params
                Call analysisTree(arg)
            Next

            Call analysisTree(expr.body, attrs)
        End Sub

        Private Sub analysisTree(expr As ValueAssign, attrs As ArgumentInfo)
            Call AnalysisTree(expr.value, attrs)
        End Sub

        Private Sub analysisTree(expr As StringInterpolation, attrs As ArgumentInfo)
            For Each part As Expression In expr.stringParts
                Call AnalysisTree(part, attrs)
            Next
        End Sub

        Private Sub analysisTree(expr As ElseBranch, attrs As ArgumentInfo)
            Call analysisTree(DirectCast(expr.closure.body, ClosureExpression), attrs)
        End Sub

        Private Sub analysisTree(expr As BinaryInExpression, attrs As ArgumentInfo)
            Call AnalysisTree(expr.a, attrs)
            Call AnalysisTree(expr.b, attrs)
        End Sub

        Private Sub analysisTree(expr As BinaryExpression, attrs As ArgumentInfo)
            Call AnalysisTree(expr.left, attrs)
            Call AnalysisTree(expr.right, attrs)
        End Sub

        Private Sub analysisTree(expr As VectorLiteral, attrs As ArgumentInfo)
            For Each element As Expression In expr
                Call AnalysisTree(element, attrs)
            Next
        End Sub

        Private Sub analysisTree(expr As FunctionInvoke, attrs As ArgumentInfo)
            For Each arg As Expression In expr.parameters
                Call AnalysisTree(arg, attrs)
            Next
        End Sub

        Private Sub analysisTree(expr As DeclareNewSymbol)
            Dim type As TypeCodes = expr.type
            Dim attrs As New ArgumentInfo With {.attrs = expr.attributes}

            If type = TypeCodes.generic Then
                type = TypeCodes.string
            End If

            attrs.type = type

            If TypeOf expr.m_value Is ArgumentValue Then
                ' default is NULL
                Call AddArgumentValue(expr.m_value, "", attrs)
            Else
                Call AnalysisTree(expr.m_value, attrs)
            End If
        End Sub

        Private Sub analysisTree(expr As [Imports], attrs As ArgumentInfo)
            Call AnalysisTree(expr.packages, attrs)
            Call AnalysisTree(expr.library, attrs)
        End Sub

        ''' <summary>
        ''' add a command line argument value
        ''' </summary>
        ''' <param name="expr"></param>
        ''' <param name="default$"></param>
        ''' <param name="attrs"></param>
        Private Sub AddArgumentValue(expr As Expression, default$, attrs As ArgumentInfo)
            Dim name As String = DirectCast(expr, ArgumentValue).name.ToString.Trim(""""c)
            Dim info As String = Nothing

            If Not attrs Is Nothing Then
                info = attrs!info

                If attrs.type = TypeCodes.boolean Then
                    [default] = "FALSE"
                End If
            End If

            Call New CommandLineArgument With {
                .name = name,
                .description = info,
                .defaultValue = [default],
                .type = attrs.type.ToString
            }.DoCall(AddressOf arguments.Add)
        End Sub

        Private Sub analysisTree(expr As BinaryOrExpression, attrs As ArgumentInfo)
            Dim left As Expression = expr.left
            Dim right As Expression = expr.right

            If TypeOf left Is ArgumentValue Then
                Call AddArgumentValue(left, parseDefault(right), attrs)
            Else
                Call AnalysisTree(left, attrs)
                Call AnalysisTree(right, attrs)
            End If
        End Sub

        Private Function parseDefault(def As Expression) As String
            If TypeOf def Is FunctionInvoke Then
                If DirectCast(def, FunctionInvoke).funcName.ToString.Trim(""""c) = "stop" Then
                    Return $"<required: {DirectCast(def, FunctionInvoke).parameters(Scan0)}>"
                End If
            End If

            Return def.ToString
        End Function

        Private Sub analysisTree(expr As IfBranch, attrs As ArgumentInfo)
            AnalysisTree(expr.ifTest, attrs)
            analysisTree(expr.trueClosure.body, attrs)
        End Sub

        Private Sub analysisTree(closure As ClosureExpression, attrs As ArgumentInfo)
            For Each line As Expression In closure.EnumerateCodeLines
                Call AnalysisTree(line, attrs)
            Next
        End Sub

        Private Sub analysisTree(expr As SymbolIndexer, attrs As ArgumentInfo)
            Call AnalysisTree(expr.symbol, attrs)
            Call AnalysisTree(expr.index, attrs)
        End Sub

        Public Function AnalysisAllCommands() As ShellScript
            For Each line As Expression In Rscript
                Call AnalysisTree(line, New ArgumentInfo)
            Next

            Return Me
        End Function

        Public Shared Widening Operator CType(Rscript As Rscript) As ShellScript
            Return New ShellScript(Rscript)
        End Operator
    End Class
End Namespace