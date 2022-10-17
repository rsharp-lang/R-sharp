#Region "Microsoft.VisualBasic::d0d39ff5e9d51a570ce73a883c9efd0b, R-sharp\R#\System\Document\ShellScript\ShellScript.vb"

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

    '   Total Lines: 475
    '    Code Lines: 374
    ' Comment Lines: 16
    '   Blank Lines: 85
    '     File Size: 20.21 KB


    '     Class ShellScript
    ' 
    '         Properties: argumentList, message
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: AnalysisAllCommands, GetCommandArgument, loadMetaLines, parseDefault, parseMetaData
    ' 
    '         Sub: AddArgumentValue, (+27 Overloads) analysisTree, AnalysisTree, PrintUsage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Annotation
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports RunCommandLine = SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.ExternalCommandLine

Namespace Development.CommandLine

    ''' <summary>
    ''' the R# shell script commandline arguments helper module
    ''' </summary>
    Public Class ShellScript

        ReadOnly Rscript As Program
        ReadOnly arguments As New List(Of CommandLineArgument)
        ReadOnly sourceScript As String
        ReadOnly info As String = "<No description provided.>"
        ReadOnly title As String
        ReadOnly dependency As New List(Of Dependency)
        ReadOnly authors As String()

        Public ReadOnly Property message As String
        Public ReadOnly Property argumentList As New Dictionary(Of String, ArgumentInfo)

        Sub New(Rscript As Rscript)
            Dim metaLines As String() = Rscript.script _
                .LineTokens _
                .DoCall(AddressOf loadMetaLines) _
                .ToArray
            Dim meta As Dictionary(Of String, String) = parseMetaData(metaLines)

            Me.Rscript = Program.CreateProgram(Rscript, [error]:=message)
            Me.sourceScript = Rscript.fileName

            Me.title = meta.TryGetValue("title") Or sourceScript.BaseName.AsDefault

            If meta.ContainsKey("description") Then
                info = meta!description
            End If

            If meta.ContainsKey("author") Then
                authors = meta("author").StringSplit("\s*[,;]\s*")
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetCommandArgument(name As String) As CommandLineArgument
            Return arguments _
                .Where(Function(a) a.name.TextEquals(name)) _
                .FirstOrDefault
        End Function

        Private Shared Iterator Function loadMetaLines(lines As IEnumerable(Of String)) As IEnumerable(Of String)
            Dim beginRegion As Boolean = False
            Dim commentPattern As New Regex("^((#)|(#'))((\s.+?)?|(\s+))$", RegexICMul)

            For Each line As String In lines
                If beginRegion Then
                    If line <> "" AndAlso line = commentPattern.Match(line).Value Then
                        Yield line
                    Else
                        Exit For
                    End If
                Else
                    If line <> "" AndAlso line = commentPattern.Match(line).Value Then
                        beginRegion = True
                        Yield line
                    End If
                End If
            Next
        End Function

        Private Shared Function parseMetaData(meta As String()) As Dictionary(Of String, String)
            Dim text As String() = meta _
                .Select(Function(line)
                            Return line.Trim(" "c, "#"c, "'"c, ASCII.CR, ASCII.LF, ASCII.TAB)
                        End Function) _
                .ToArray
            Dim data As Dictionary(Of String, String) = text.ParseTagData(strict:=False)

            Return data
        End Function

        Public Sub PrintUsage(dev As TextWriter)
            Dim cli As New List(Of String)
            Dim maxName As String = arguments.Select(Function(a) a.name).MaxLengthString
            Dim valueStr As String

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
                ElseIf arg.isNumeric Then
                    cli.Add($"[{arg.name} <{arg.type}, default={Strings.Trim(arg.defaultValue).Trim(""""c)}>]")
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
                Dim prefix As String = $" {arg.name}: {New String(" "c, maxName.Length - arg.name.Length)}"
                Dim descriptionBlock As String = Paragraph _
                    .SplitParagraph(arg.description Or none, 65) _
                    .JoinBy(vbCrLf & New String(" "c, prefix.Length))

                If arg.defaultValue.StartsWith("<required") Then
                    valueStr = ($"<required>")
                ElseIf arg.isNumeric Then
                    valueStr = ($"[{arg.type}, default={Strings.Trim(arg.defaultValue).Trim(""""c)}]")
                Else
                    valueStr = ($"[{arg.type}, default={arg.defaultValue}]")
                End If

                Call dev.WriteLine(prefix & descriptionBlock) ' & valueStr)

                If descriptionBlock.Contains(vbCr) OrElse descriptionBlock.Contains(vbLf) Then
                    Call dev.WriteLine()
                End If
            Next

            If Not authors.IsNullOrEmpty Then
                Call dev.WriteLine()
                Call dev.WriteLine("Authors:")
                Call authors.printContentArray(Nothing, Nothing, 80, dev)
            End If

            If dependency > 0 Then
                Dim requires = dependency.Where(Function(deps) deps.library.StringEmpty).ToArray
                Dim import = dependency.Where(Function(deps) Not deps.library.StringEmpty).ToArray

                Call dev.WriteLine()
                Call dev.WriteLine("Dependency List:")

                If Not requires.IsNullOrEmpty Then
                    Dim allList As String() = requires _
                        .Select(Function(pkg) pkg.packages) _
                        .IteratesALL _
                        .Distinct _
                        .ToArray

                    Call dev.WriteLine()
                    Call dev.WriteLine(" Loading: ")
                    Call allList.printContentArray(Nothing, Nothing, 80, dev)
                End If

                If Not import.IsNullOrEmpty Then
                    Dim allList = import.Select(Function(ref) $"{ref.library}::[{ref.packages.JoinBy(", ")}]").ToArray

                    Call dev.WriteLine()
                    Call dev.WriteLine(" Imports: ")
                    Call allList.printContentArray(Nothing, Nothing, 80, dev)
                End If
            End If

            Call dev.Flush()
        End Sub

        Private Sub AnalysisTree(expr As Expression, attrs As ArgumentInfo)
            If expr Is Nothing OrElse
                TypeOf expr Is Literal OrElse
                TypeOf expr Is SymbolReference OrElse
                TypeOf expr Is ScriptSymbol OrElse
                TypeOf expr Is BreakPoint OrElse
                TypeOf expr Is Regexp OrElse
                TypeOf expr Is ContinuteFor OrElse
                TypeOf expr Is VectorLoop OrElse
                TypeOf expr Is TryCatchExpression Then

                Return
            End If

            Select Case expr.GetType
                Case GetType([Imports]) : Call analysisTree(DirectCast(expr, [Imports]), attrs)
                Case GetType(BinaryOrExpression) : Call analysisTree(DirectCast(expr, BinaryOrExpression), attrs)
                Case GetType(DeclareNewSymbol) : Call analysisTree(DirectCast(expr, DeclareNewSymbol))
                Case GetType(FunctionInvoke) : Call analysisTree(DirectCast(expr, FunctionInvoke), attrs)
                Case GetType(IfBranch) : Call analysisTree(DirectCast(expr, IfBranch), attrs)

                Case GetType(ClosureExpression), GetType(AcceptorClosure)
                    Call analysisTree(DirectCast(expr, ClosureExpression), attrs)

                Case GetType(SymbolIndexer) : Call analysisTree(DirectCast(expr, SymbolIndexer), attrs)
                Case GetType(VectorLiteral) : Call analysisTree(DirectCast(expr, VectorLiteral), attrs)
                Case GetType(BinaryExpression) : Call analysisTree(DirectCast(expr, BinaryExpression), attrs)
                Case GetType(BinaryInExpression) : Call analysisTree(DirectCast(expr, BinaryInExpression), attrs)
                Case GetType(ElseBranch) : Call analysisTree(DirectCast(expr, ElseBranch), attrs)
                Case GetType(StringInterpolation) : Call analysisTree(DirectCast(expr, StringInterpolation), attrs)
                Case GetType(ValueAssignExpression) : Call analysisTree(DirectCast(expr, ValueAssignExpression), attrs)
                Case GetType(DeclareNewFunction) : Call analysisTree(DirectCast(expr, DeclareNewFunction), attrs)
                Case GetType(ForLoop) : Call analysisTree(DirectCast(expr, ForLoop), attrs)
                Case GetType(ReturnValue) : Call analysisTree(DirectCast(expr, ReturnValue), attrs)
                Case GetType(DeclareLambdaFunction) : Call analysisTree(DirectCast(expr, DeclareLambdaFunction), attrs)
                Case GetType(AppendOperator) : Call analysisTree(DirectCast(expr, AppendOperator), attrs)
                Case GetType(UnaryNot) : Call analysisTree(DirectCast(expr, UnaryNot), attrs)
                Case GetType(SequenceLiteral) : Call analysisTree(DirectCast(expr, SequenceLiteral), attrs)
                Case GetType(IIfExpression) : Call analysisTree(DirectCast(expr, IIfExpression), attrs)
                Case GetType(Require) : Call analysisTree(DirectCast(expr, Require), attrs)
                Case GetType(MemberValueAssign) : Call analysisTree(DirectCast(expr, MemberValueAssign), attrs)
                Case GetType(RunCommandLine) : Call analysisTree(DirectCast(expr, RunCommandLine), attrs)
                Case GetType(UsingClosure) : Call analysisTree(DirectCast(expr, UsingClosure), attrs)
                Case GetType(ByRefFunctionCall) : Call analysisTree(DirectCast(expr, ByRefFunctionCall), attrs)
                Case GetType(UnaryNumeric) : Call AnalysisTree(DirectCast(expr, UnaryNumeric).numeric, attrs)
                Case GetType(JSONLiteral) : Call analysisTree(DirectCast(expr, JSONLiteral), attrs)
                Case GetType(ArgumentValue)
                    ' do nothing 

                Case Else
                    Throw New NotImplementedException(expr.GetType.FullName)
            End Select
        End Sub

        Private Sub analysisTree(expr As JSONLiteral, attrs As ArgumentInfo)
            For Each member As NamedValue(Of Expression) In expr.members
                Call AnalysisTree(member.Value, attrs)
            Next
        End Sub

        'Private Sub analysisTree(expr As VectorLoop, attrs As ArgumentInfo)
        '    ' do nothing
        'End Sub

        Private Sub analysisTree(expr As ByRefFunctionCall, attrs As ArgumentInfo)
            Call AnalysisTree(expr.funcRef, attrs)
            Call AnalysisTree(expr.target, attrs)
            Call AnalysisTree(expr.value, attrs)

            For Each arg In expr.arguments.SafeQuery
                Call AnalysisTree(arg, attrs)
            Next
        End Sub

        Private Sub analysisTree(expr As UsingClosure, attrs As ArgumentInfo)
            Call AnalysisTree(expr.params, attrs)
            Call analysisTree(expr.closure, attrs)
        End Sub

        Private Sub analysisTree(expr As RunCommandLine, attrs As ArgumentInfo)
            Call AnalysisTree(expr.cli, attrs)
        End Sub

        Private Sub analysisTree(expr As MemberValueAssign, attrs As ArgumentInfo)
            Call analysisTree(expr.memberReference, attrs)
            Call AnalysisTree(expr.value, attrs)
        End Sub

        Private Sub analysisTree(expr As IIfExpression, attrs As ArgumentInfo)
            Call AnalysisTree(expr.ifTest, attrs)
            Call AnalysisTree(expr.trueResult, attrs)
            Call AnalysisTree(expr.falseResult, attrs)
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
            For Each arg In expr.parameters
                Call analysisTree(arg)
            Next

            Call analysisTree(expr.body, attrs)
        End Sub

        Private Sub analysisTree(expr As ValueAssignExpression, attrs As ArgumentInfo)
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
            Call AnalysisTree(expr.left, attrs)
            Call AnalysisTree(expr.left, attrs)
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
            Dim attrs As New ArgumentInfo(type) With {
                .attrs = expr.attributes
            }

            If TypeOf expr.m_value Is ArgumentValue Then
                ' default is NULL
                Call AddArgumentValue(expr.m_value, "", True, attrs)
            Else
                Call AnalysisTree(expr.m_value, attrs)
            End If
        End Sub

#Region "Dependency"

        Private Sub analysisTree(expr As Require, attrs As ArgumentInfo)
            For Each name As Expression In expr.packages
                Call AnalysisTree(name, attrs)
            Next

            ' add package reference
            Call dependency.Add(New Dependency(expr))
        End Sub

        Private Sub analysisTree(expr As [Imports], attrs As ArgumentInfo)
            Call AnalysisTree(expr.packages, attrs)
            Call AnalysisTree(expr.library, attrs)

            ' add package reference
            Call dependency.Add(New Dependency(expr))
        End Sub

#End Region

        ''' <summary>
        ''' add a command line argument value
        ''' </summary>
        ''' <param name="expr"></param>
        ''' <param name="default$"></param>
        ''' <param name="attrs"></param>
        Private Sub AddArgumentValue(expr As Expression, default$, isLiteral As Boolean, attrs As ArgumentInfo)
            Dim name As String = DirectCast(expr, ArgumentValue).name.ToString.Trim(""""c)
            Dim info As String = Nothing

            If Not attrs Is Nothing Then
                info = attrs!info

                If attrs.GetTypeCode = NameOf(TypeCodes.boolean) Then
                    [default] = "FALSE"
                End If
            End If

            Call argumentList.Add(name, attrs)
            Call New CommandLineArgument With {
                .name = name,
                .description = Strings.Trim(info) _
                    .LineTokens _
                    .Select(Function(str)
                                Return str.Trim(" "c, ASCII.TAB)
                            End Function) _
                    .JoinBy(" "),
                .defaultValue = [default],
                .type = attrs.GetTypeCode,
                .isLiteral = isLiteral
            }.DoCall(AddressOf arguments.Add)
        End Sub

        Private Sub analysisTree(expr As BinaryOrExpression, attrs As ArgumentInfo)
            Dim left As Expression = expr.left
            Dim right As Expression = expr.right

            If TypeOf left Is ArgumentValue Then
                Call AddArgumentValue(left, parseDefault(right), TypeOf right Is Literal, attrs)
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

            Return $"""{DefaultFormatter.FormatDefaultString(def)}"""
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
                Call AnalysisTree(line, New ArgumentInfo(TypeCodes.string))
            Next

            Return Me
        End Function

        Public Shared Widening Operator CType(Rscript As Rscript) As ShellScript
            Return New ShellScript(Rscript)
        End Operator
    End Class
End Namespace
