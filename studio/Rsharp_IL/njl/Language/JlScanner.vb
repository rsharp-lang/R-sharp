#Region "Microsoft.VisualBasic::9af7205c6bd8b3e1c2abe628a6b52e92, studio\Rsharp_IL\njl\Language\JlScanner.vb"

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

    '   Total Lines: 30
    '    Code Lines: 24
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 1.06 KB


    '     Class JlScanner
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Parser
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language

    Public Class JlScanner : Inherits Scanner

        Shared ReadOnly jlKeywords As String() = {
            "baremodule", "begin", "break", "catch", "const",
            "continue", "do", "else", "elseif", "end", "export",
            "false", "finally", "for", "function", "global",
            "if", "import", "include", "let", "local", "macro", "module",
            "quote", "return", "struct", "true", "try", "using",
            "while"
        }

        <DebuggerStepThrough>
        Sub New(source As [Variant](Of String, CharPtr), Optional tokenStringMode As Boolean = False)
            Call MyBase.New(source, tokenStringMode)

            Call keywords.Clear()
            Call keywords.Add(jlKeywords).ToArray
            Call nullLiteral.Clear()
            Call nullLiteral.Add("nothing")

            keepsDelimiter = True
        End Sub
    End Class
End Namespace
