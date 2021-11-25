#Region "Microsoft.VisualBasic::b456b7b4c8849cd7f333a6ace4a16e49, R#\Interpreter\Syntax\SyntaxImplements\RequireSyntax.vb"

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

    '     Module RequireSyntax
    ' 
    '         Function: [Imports], Require
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module RequireSyntax

        Public Function Require(names As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim packages As New List(Of Expression)

            For Each name As SyntaxResult In names _
                .SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(b) Not b.isComma) _
                .Select(Function(tokens)
                            Return Expression.CreateExpression(tokens, opts)
                        End Function)

                If name.isException Then
                    Return name
                Else
                    packages += name.expression
                End If
            Next

            Return New SyntaxResult(New Require(packages))
        End Function

        Public Function [Imports](code As IEnumerable(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            With code.ToArray
                If .ElementAt(Scan0).isKeyword("imports") AndAlso .ElementAtOrDefault(2).isKeyword("from") Then
                    Dim packages = .ElementAt(1).DoCall(Function(tokens) Expression.CreateExpression(tokens, opts))
                    Dim library As SyntaxResult =
                        .ElementAt(3) _
                        .DoCall(Function(tokens)
                                    Return Expression.CreateExpression(tokens, opts)
                                End Function)

                    If packages.isException Then
                        Return packages
                    ElseIf library.isException Then
                        Return library
                    Else
                        Return New [Imports](
                            packages:=packages.expression,
                            library:=library.expression,
                            source:=Nothing
                        )
                    End If
                ElseIf .ElementAt(Scan0).isKeyword("imports") Then
                    If .Length = 1 Then
                        ' try interpreted as symbol reference
                        Return New SyntaxResult(New SymbolReference(.ElementAt(Scan0)(Scan0)))
                    End If

                    ' imports <*.R/*.dll file>
                    Dim files As SyntaxResult =
                        .ElementAt(1) _
                        .DoCall(Function(tokens)
                                    Return Expression.CreateExpression(tokens, opts)
                                End Function)

                    If files.isException Then
                        Return files
                    Else
                        Return New [Imports](
                            packages:=Nothing,
                            library:=files.expression,
                            source:=opts.source.source
                        )
                    End If
                Else
                    Return SyntaxResult.CreateError(
                        err:=New SyntaxErrorException,
                        opts:=opts.SetCurrentRange(.ElementAt(Scan0))
                    )
                End If
            End With
        End Function
    End Module
End Namespace
