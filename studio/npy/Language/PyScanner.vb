#Region "Microsoft.VisualBasic::4b2b3b80cded19bc7837f1879b5b20b9, R-sharp\studio\npy\Language\PyScanner.vb"

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

    '   Total Lines: 47
    '    Code Lines: 33
    ' Comment Lines: 4
    '   Blank Lines: 10
    '     File Size: 1.70 KB


    '     Class PyScanner
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetTokens
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Parser
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language

    Public Class PyScanner : Inherits Scanner

        Shared ReadOnly pyKeywords As String() = {
            "and", "as", "assert", "break", "class", "continue", "def", "elif", "else", "except", "false", "finally",
            "for", "from", "global", "if", "import", "in", "is", "lambda", "none", "nonlocal", "not", "or", "pass",
            "raise", "return", "true", "try", "while", "with", "yield"
        }

        <DebuggerStepThrough>
        Sub New(source As [Variant](Of String, CharPtr), Optional tokenStringMode As Boolean = False)
            Call MyBase.New(source, tokenStringMode)

            Call keywords.Clear()
            Call keywords.Add(pyKeywords).ToArray
            Call nullLiteral.Clear()
            Call nullLiteral.Add("None")
            ' Call shortOperators.Add("."c)

            keepsDelimiter = True
        End Sub

        Public Overrides Function GetTokens() As IEnumerable(Of Token)
            Dim all As Token() = MyBase.GetTokens().ToArray

            ' keyword . symbol . symbol
            ' as.data.frame
            ' keyword is not allowed follow . symbol
            For i As Integer = 0 To all.Length - 1
                If all(i).name = TokenType.keyword Then
                    If all(i + 1) = (TokenType.operator, ".") Then
                        all(i).name = TokenType.identifier
                    End If
                End If
            Next

            Return all
        End Function

    End Class
End Namespace
