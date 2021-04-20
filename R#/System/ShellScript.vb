Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Development

    ''' <summary>
    ''' the R# shell script commandline arguments helper module
    ''' </summary>
    Public Class ShellScript

        ReadOnly Rscript As Program
        ReadOnly arguments As New List(Of NamedValue(Of String))
        ReadOnly sourceScript As String

        Public ReadOnly Property message As String

        Sub New(Rscript As Rscript)
            Me.Rscript = Program.CreateProgram(Rscript, [error]:=message)
            Me.sourceScript = Rscript.fileName
        End Sub

        Public Sub PrintUsage(dev As TextWriter)
            Dim cli As New List(Of String)
            Dim maxName As String = arguments.Select(Function(a) a.Name).MaxLengthString

            For Each arg As NamedValue(Of String) In arguments
                If arg.Value.StartsWith("<required") Then
                    cli.Add($"{arg.Name} <value>")
                Else
                    cli.Add($"[{arg.Name} <default={arg.Value}>]")
                End If
            Next

            Call dev.WriteLine($"SYNOPSIS")
            Call dev.WriteLine($"Rscript ""{sourceScript}"" {cli.JoinBy(" ")}")
            Call dev.WriteLine()
            Call dev.WriteLine("CommandLine Argument Values:")

            Call dev.WriteLine()

            For Each arg As NamedValue(Of String) In arguments
                Call dev.WriteLine($" {arg.Name}: {New String(" "c, maxName.Length - arg.Name.Length)}{arg.Description}")
            Next

            Call dev.Flush()
        End Sub

        Private Sub AnalysisTree(expr As Expression, attrs As Dictionary(Of String, String()))
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

                Case Else
                    Throw New NotImplementedException(expr.GetType.FullName)
            End Select
        End Sub

        Private Sub analysisTree(expr As FunctionInvoke, attrs As Dictionary(Of String, String()))
            For Each arg As Expression In expr.parameters
                Call AnalysisTree(arg, attrs)
            Next
        End Sub

        Private Sub analysisTree(expr As DeclareNewSymbol)
            Call AnalysisTree(expr.m_value, expr.attributes)
        End Sub

        Private Sub analysisTree(expr As [Imports], attrs As Dictionary(Of String, String()))
            Call analysisTree(expr.packages)
            Call analysisTree(expr.library)
        End Sub

        Private Sub analysisTree(expr As BinaryOrExpression, attrs As Dictionary(Of String, String()))
            Dim left As Expression = expr.left
            Dim right As Expression = expr.right

            If TypeOf left Is ArgumentValue Then
                Dim name As String = DirectCast(left, ArgumentValue).name.ToString.Trim(""""c)
                Dim info As String = Nothing

                If Not attrs.IsNullOrEmpty Then
                    info = attrs.TryGetValue("info").JoinBy(";" & vbCrLf)
                End If

                Call New NamedValue(Of String) With {
                    .Name = name,
                    .Description = info,
                    .Value = parseDefault(right)
                }.DoCall(AddressOf arguments.Add)
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

        Private Sub analysisTree(expr As IfBranch)
            AnalysisTree(expr.ifTest)
            AnalysisTree(expr.trueClosure.body)
        End Sub

        Private Sub analysisTree(closure As ClosureExpression)
            For Each line As Expression In closure.EnumerateCodeLines
                Call AnalysisTree(line)
            Next
        End Sub

        Private Sub analysisTree(expr As SymbolIndexer)
            Call AnalysisTree(expr.symbol)
            Call AnalysisTree(expr.index)
        End Sub

        Public Function AnalysisAllCommands() As ShellScript
            For Each line As Expression In Rscript
                Call AnalysisTree(line, New Dictionary(Of String, String()))
            Next

            Return Me
        End Function

        Public Shared Widening Operator CType(Rscript As Rscript) As ShellScript
            Return New ShellScript(Rscript)
        End Operator
    End Class
End Namespace