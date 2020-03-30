#Region "Microsoft.VisualBasic::812746a8ee515c57fea6ed3f03559758, R#\Interpreter\Syntax\SyntaxImplements\ValueAssignSyntax.vb"

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

    '     Module ValueAssignSyntax
    ' 
    '         Function: ValueAssign
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module ValueAssignSyntax

        Public Function ValueAssign(tokens As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim targetSymbols = DeclareNewSymbolSyntax _
                .getNames(tokens(Scan0)) _
                .Select(Function(name) New Literal(name)) _
                .ToArray
            Dim isByRef = tokens(Scan0)(Scan0).text = "="
            Dim value As SyntaxResult = tokens.Skip(2) _
                .AsList _
                .ParseExpression(opts)

            If value.isException Then
                Return value
            Else
                Return New ValueAssign(targetSymbols, value.expression) With {
                    .isByRef = isByRef
                }
            End If
        End Function
    End Module
End Namespace
