#Region "Microsoft.VisualBasic::52f67186f8d238b89fc1273173be3f77, G:/GCModeller/src/R-sharp/studio/Rsharp_IL/nts//SyntaxParser/TsScanner.vb"

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

    '   Total Lines: 29
    '    Code Lines: 24
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 1.04 KB


    ' Class TsScanner
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Parser
Imports SMRUCC.Rsharp.Language.TokenIcer

Public Class TsScanner : Inherits Scanner

    Shared ReadOnly tsKeywords As String() = {
        "if", "for", "let", "const", "super", "class", "var", "in", "of", "continue",
        "module", "namespace", "function", "return", "typeof", "instanceof", "extends",
        "import", "from",
        "throw"
    }

    Public Sub New(source As [Variant](Of String, CharPtr), Optional tokenStringMode As Boolean = False)
        MyBase.New(source, tokenStringMode)

        Call keywords.Clear()
        Call keywords.Add(tsKeywords).ToArray
        Call nullLiteral.Clear()
        Call nullLiteral.Add("null")
        Call shortOperators.Clear()
        Call shortOperators.AddList("."c, "+"c, "-"c, "*"c, "/"c, "\"c, "!"c, "|"c, "&"c, "^")
        Call longOperatorParts.Add("/"c)

        keepsDelimiter = True
        dollarAsSymbol = True
        commentFlag = "//"
    End Sub
End Class
