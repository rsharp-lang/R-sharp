#Region "Microsoft.VisualBasic::7879461321e5eaf37b11d301b284dd44, R-sharp\R#\Language\Syntax\SyntaxTree\BinaryExpressionTree\NamespaceReference.vb"

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

    '   Total Lines: 84
    '    Code Lines: 67
    ' Comment Lines: 3
    '   Blank Lines: 14
    '     File Size: 3.50 KB


    '     Class NamespaceReferenceProcessor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: expression, view
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Language.Syntax.SyntaxParser

    Friend Class NamespaceReferenceProcessor : Inherits GenericSymbolOperatorProcessor

        Sub New()
            Call MyBase.New("::")
        End Sub

        Protected Overrides Function view() As String
            Return "namespace::symbol"
        End Function

        Protected Overrides Function expression(a As [Variant](Of SyntaxResult, String),
                                                b As [Variant](Of SyntaxResult, String),
                                                opts As SyntaxBuilderOptions) As SyntaxResult
            Dim namespaceRef As Expression
            Dim syntaxTemp As SyntaxResult = a.TryCast(Of SyntaxResult)

            If syntaxTemp.isException Then
                Return syntaxTemp
            ElseIf b.VA.isException Then
                Return b.VA
            End If

            If TypeOf a.VA.expression Is VectorLiteral AndAlso TypeOf b.VA.expression Is SymbolReference Then
                ' [x]::name
                Dim obj = DirectCast(a.VA.expression, VectorLiteral).First
                Dim dotnet As New DotNetObject(obj, b.VA.expression)
                Dim syntax As New SyntaxResult(dotnet)

                Return syntax
            End If

            Dim nsSymbol$ = DirectCast(syntaxTemp.expression, SymbolReference).symbol

            If TypeOf b.VA.expression Is FunctionInvoke Then
                ' a::b() function invoke
                Dim calls As FunctionInvoke = b.VA.expression
                calls.namespace = nsSymbol
                namespaceRef = calls
            ElseIf TypeOf b.VA.expression Is SymbolReference Then
                Dim nsRef As String = $"{nsSymbol}::{b.VA}"
                Dim stack As New StackFrame With {
                    .File = opts.source.fileName,
                    .Line = opts.currentLine,
                    .Method = New Method With {
                        .Method = nsRef,
                        .[Namespace] = nsSymbol,
                        .[Module] = "callFunc"
                    }
                }

                ' a::b view function help info
                namespaceRef = New NamespaceFunctionSymbolReference(nsSymbol, b.VA.expression, stack)
            ElseIf TypeOf b.VA.expression Is SymbolIndexer Then
                Dim indexer As SymbolIndexer = b.VA.expression
                Dim nsRef = expression(a, New SyntaxResult(indexer.symbol), opts)

                If nsRef.isException Then
                    Return nsRef
                Else
                    indexer = New SymbolIndexer(
                        symbol:=nsRef.expression,
                        index:=indexer.index,
                        indexType:=indexer.indexType
                    )
                End If

                Return indexer
            Else
                Return SyntaxResult.CreateError(New SyntaxErrorException, opts)
            End If

            Return New SyntaxResult(namespaceRef)
        End Function
    End Class
End Namespace
