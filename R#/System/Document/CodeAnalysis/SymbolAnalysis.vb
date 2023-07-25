#Region "Microsoft.VisualBasic::8b039c1c209951dc9d0455bbc09ba530, D:/GCModeller/src/R-sharp/R#//System/Document/CodeAnalysis/SymbolAnalysis.vb"

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

    '   Total Lines: 330
    '    Code Lines: 257
    ' Comment Lines: 17
    '   Blank Lines: 56
    '     File Size: 14.57 KB


    '     Class SymbolAnalysis
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetNameEnums, GetSymbolReferenceList, SymbolAccess, ToString
    ' 
    '         Sub: GetSymbolReferenceList, (+18 Overloads) GetSymbols, Push
    '         Class Context
    ' 
    '             Constructor: (+2 Overloads) Sub New
    ' 
    '             Function: ToString
    ' 
    '             Sub: Create, (+2 Overloads) Push
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Development.CodeAnalysis

    ''' <summary>
    ''' analysis of the static symbol reference
    ''' </summary>
    Public Class SymbolAnalysis

        Dim envir As String
        Dim declareSymbols As New Dictionary(Of String, PropertyAccess)
        Dim parent As SymbolAnalysis

        Sub New(Optional name As String = "global")
            envir = name
        End Sub

        Sub New(parent As SymbolAnalysis, name As String)
            Me.envir = name
            Me.parent = parent
        End Sub

        Public Iterator Function GetNameEnums() As IEnumerable(Of NamedValue(Of PropertyAccess))
            For Each symbolRef In declareSymbols
                Yield New NamedValue(Of PropertyAccess)(symbolRef.Key, symbolRef.Value, envir)
            Next
        End Function

        Public Sub Push(symbolName As String)
            declareSymbols.Add(symbolName, PropertyAccess.Writeable)
        End Sub

        Public Function SymbolAccess(symbolName As String, ByRef context As SymbolAnalysis) As PropertyAccess
            If declareSymbols.ContainsKey(symbolName) Then
                context = Me
                Return declareSymbols(symbolName)
            ElseIf Not parent Is Nothing Then
                Return parent.SymbolAccess(symbolName, context)
            Else
                context = Nothing
                Return PropertyAccess.NotSure
            End If
        End Function

        Public Overrides Function ToString() As String
            Return envir
        End Function

        Public Shared Function GetSymbolReferenceList(code As Expression, Optional envir As String = "global") As IEnumerable(Of NamedValue(Of PropertyAccess))
            Dim ref As New List(Of NamedValue(Of PropertyAccess))
            Dim context As New Context With {
                .context = New SymbolAnalysis(envir),
                .ref = ref
            }

            Call GetSymbolReferenceList(code, context)

            Return ref
        End Function

        Private Shared Sub GetSymbolReferenceList(code As Expression, context As Context)
            If code Is Nothing OrElse TypeOf code Is Literal Then
                Return
            ElseIf TypeOf code Is Require Then
                ' require(xxx)
                ' no symbol reference
                Return
            End If

            Select Case code.GetType
                Case GetType(BinaryExpression),
                     GetType(BinaryInExpression),
                     GetType(BinaryOrExpression)

                    Call GetSymbols(DirectCast(code, IBinaryExpression), context)

                Case GetType(ElseBranch) : Call GetSymbols(DirectCast(code, ElseBranch), context)
                Case GetType(SymbolIndexer) : Call GetSymbols(DirectCast(code, SymbolIndexer), context)
                Case GetType(IfBranch) : Call GetSymbols(DirectCast(code, IfBranch), context)
                Case GetType(UnaryNot) : Call GetSymbols(DirectCast(code, UnaryNot), context)
                Case GetType(FunctionInvoke) : Call GetSymbols(DirectCast(code, FunctionInvoke), context)
                Case GetType(DeclareNewFunction) : Call GetSymbols(DirectCast(code, DeclareNewFunction), context)
                Case GetType(DeclareNewSymbol) : Call GetSymbols(DirectCast(code, DeclareNewSymbol), context)
                Case GetType(AcceptorClosure), GetType(ClosureExpression) : Call GetSymbols(DirectCast(code, ClosureExpression), context)
                Case GetType(ExternalCommandLine) : Call GetSymbols(DirectCast(code, ExternalCommandLine), context)
                Case GetType(StringInterpolation) : Call GetSymbols(DirectCast(code, StringInterpolation), context)
                Case GetType(SymbolReference)
                    Call context.Push(DirectCast(code, SymbolReference), PropertyAccess.Readable)
                Case GetType(ValueAssignExpression) : Call GetSymbols(DirectCast(code, ValueAssignExpression), context)
                Case GetType(UsingClosure) : Call GetSymbols(DirectCast(code, UsingClosure), context)
                Case GetType(ForLoop) : Call GetSymbols(DirectCast(code, ForLoop), context)
                Case GetType(SequenceLiteral) : GetSymbols(DirectCast(code, SequenceLiteral), context)
                Case GetType(VectorLiteral) : GetSymbols(DirectCast(code, VectorLiteral), context)
                Case GetType([Imports]) : GetSymbols(DirectCast(code, [Imports]), context)
                Case GetType(DeclareLambdaFunction) : GetSymbols(DirectCast(code, DeclareLambdaFunction), context)

                Case Else
                    Throw New NotImplementedException(code.GetType.FullName)
            End Select
        End Sub

        Private Shared Sub GetSymbols(code As DeclareLambdaFunction, context As Context)
            ' 20230626 the function parameter is not a symbol that comsume
            ' from the outside environment
            '
            ' For Each name As String In code.parameterNames
            '    Call context.Push(name, PropertyAccess.Readable)
            ' Next
            Dim par As Index(Of String) = code.parameterNames.Indexing
            Dim ref As New List(Of NamedValue(Of PropertyAccess))
            Dim context2 As New Context With {
                .context = New SymbolAnalysis("fetch_lambda"),
                .ref = ref
            }

            ' just handling of the lambda closure body
            Call GetSymbolReferenceList(code.closure, context2)

            ' filter out of the lambda parameter name reference
            For Each symbol As NamedValue(Of PropertyAccess) In context2.ref
                If Not symbol.Name Like par Then
                    Call context.Push(symbol.Name, symbol.Value)
                End If
            Next
        End Sub

        Private Shared Sub GetSymbols(code As [Imports], context As Context)
            Call GetSymbolReferenceList(code.packages, context)
            Call GetSymbolReferenceList(code.library, context)
        End Sub

        Private Shared Sub GetSymbols(code As VectorLiteral, context As Context)
            For Each item As Expression In code.values
                Call GetSymbolReferenceList(item, context)
            Next
        End Sub

        Private Shared Sub GetSymbols(code As SequenceLiteral, context As Context)
            Call GetSymbolReferenceList(code.from, context)
            Call GetSymbolReferenceList(code.to, context)
            Call GetSymbolReferenceList(code.steps, context)
        End Sub

        Private Shared Sub GetSymbols(code As ForLoop, context As Context)
            Call GetSymbolReferenceList(code.sequence, context)
            Call GetSymbolReferenceList(code.body, context)

            For Each name As String In code.variables
                Call context.Push(name, PropertyAccess.Readable)
            Next
        End Sub

        Private Shared Sub GetSymbols(code As UsingClosure, context As Context)
            Call GetSymbolReferenceList(code.params, context)
            Call GetSymbolReferenceList(code.closure, context)
        End Sub

        Private Shared Sub GetSymbols(code As StringInterpolation, context As Context)
            For Each part As Expression In code.stringParts
                Call GetSymbolReferenceList(part, context)
            Next
        End Sub

        Private Shared Sub GetSymbols(code As ExternalCommandLine, context As Context)
            Call GetSymbolReferenceList(code.cli, context)
        End Sub

        Private Shared Sub GetSymbols(code As ElseBranch, context As Context)
            Call GetSymbols(code.closure, context)
        End Sub

        Private Shared Sub GetSymbols(code As UnaryNot, context As Context)
            If TypeOf code.logical Is SymbolReference Then
                Call context.Push(code.logical, PropertyAccess.Readable)
            Else
                Call GetSymbolReferenceList(code.logical, context)
            End If
        End Sub

        Private Shared Sub GetSymbols(code As IfBranch, context As Context)
            Call GetSymbolReferenceList(code.ifTest, context)
            Call GetSymbolReferenceList(code.trueClosure, context)
        End Sub

        Private Shared Sub GetSymbols(code As ValueAssignExpression, context As Context)
            For Each target In code.targetSymbols
                If TypeOf target Is Literal Then
                    Call context.Push(DirectCast(target, Literal).ValueStr, PropertyAccess.Writeable)
                Else
                    Call GetSymbolReferenceList(target, context)
                End If
            Next

            Call GetSymbolReferenceList(code.value, context)
        End Sub

        Private Shared Sub GetSymbols(code As SymbolIndexer, context As Context)
            If TypeOf code.symbol Is SymbolReference Then
                Call context.Push(code.symbol, PropertyAccess.Readable)
            ElseIf TypeOf code.symbol Is Literal Then
                Call context.Push(DirectCast(code.symbol, Literal).ValueStr, PropertyAccess.Readable)
            Else
                Call GetSymbolReferenceList(code.symbol, context)
            End If

            If TypeOf code.index Is SymbolReference Then
                Call context.Push(code.index, PropertyAccess.Readable)
            Else
                Call GetSymbolReferenceList(code.index, context)
            End If
        End Sub

        Private Shared Sub GetSymbols(code As ClosureExpression, context As Context)
            context = New Context(context, $"closure_{code.GetHashCode.ToHexString}")

            For Each line As Expression In code.EnumerateCodeLines
                Call GetSymbolReferenceList(line, context)
            Next
        End Sub

        Private Shared Sub GetSymbols(code As DeclareNewFunction, context As Context)
            Call context.Push(code.funcName, PropertyAccess.Writeable)

            context = New Context(context, code.funcName)

            For Each arg In code.parameters
                Call GetSymbols(arg, context)
            Next

            context.ref.AddRange(context.context.GetNameEnums)
        End Sub

        Private Shared Sub GetSymbols(code As DeclareNewSymbol, context As Context)
            For Each name As String In code.names
                Call context.Create(name)
            Next

            If Not code.value Is Nothing Then
                Call GetSymbolReferenceList(code.value, context)
            End If
        End Sub

        Private Shared Sub GetSymbols(code As FunctionInvoke, context As Context)
            If TypeOf code.funcName Is SymbolReference Then
                Call context.Push(code.funcName, PropertyAccess.Readable)
            ElseIf TypeOf code.funcName Is Literal Then
                Call context.Push(DirectCast(code.funcName, Literal).ValueStr, PropertyAccess.Readable)
            Else
                Call GetSymbolReferenceList(code.funcName, context)
            End If

            For Each arg As Expression In code.parameters
                If TypeOf arg Is SymbolReference Then
                    Call context.Push(arg, PropertyAccess.Readable)
                Else
                    Call GetSymbolReferenceList(arg, context)
                End If
            Next
        End Sub

        Private Shared Sub GetSymbols(code As IBinaryExpression, context As Context)
            If TypeOf code.left Is SymbolReference Then
                Call context.Push(code.left, PropertyAccess.Readable)
            Else
                Call GetSymbolReferenceList(code.left, context)
            End If

            If TypeOf code.right Is SymbolReference Then
                Call context.Push(code.right, PropertyAccess.Readable)
            Else
                Call GetSymbolReferenceList(code.right, context)
            End If
        End Sub

        Private Class Context

            Public context As SymbolAnalysis
            Public ref As List(Of NamedValue(Of PropertyAccess))

            Sub New()
            End Sub

            Sub New(parent As Context, name As String)
                ref = parent.ref
                context = New SymbolAnalysis(parent.context, name)
            End Sub

            Public Sub Push(symbol As SymbolReference, access As PropertyAccess)
                Call Push(symbol.symbol, access)
            End Sub

            ''' <summary>
            ''' declare new symbol or function parameters
            ''' </summary>
            ''' <param name="symbol"></param>
            Public Sub Create(symbol As String)
                ref.Add(New NamedValue(Of PropertyAccess)(symbol, PropertyAccess.Writeable, context.envir))
                context.Push(symbol)
            End Sub

            Public Sub Push(symbol As String, access As PropertyAccess)
                Dim context As SymbolAnalysis = Me.context
                Dim currentAccess As PropertyAccess = context.SymbolAccess(symbol, context)

                If currentAccess = PropertyAccess.NotSure Then
                    ref.Add(New NamedValue(Of PropertyAccess)(symbol, access, "global"))
                Else
                    If currentAccess <> access AndAlso currentAccess <> PropertyAccess.ReadWrite Then
                        context.declareSymbols(symbol) = PropertyAccess.ReadWrite
                    End If
                End If
            End Sub

            Public Overrides Function ToString() As String
                If context.parent Is Nothing Then
                    Return context.envir
                Else
                    Return $"{context.parent} -> {context.envir}"
                End If
            End Function
        End Class
    End Class
End Namespace
