#Region "Microsoft.VisualBasic::2260a7e5c42e3b376bceb228c6c3d028, R#\Interpreter\Syntax\SyntaxBuilderOptions.vb"

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

'     Class SyntaxBuilderOptions
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.SyntaxParser

    Friend Class SyntaxBuilderOptions

        Public debug As Boolean = False
        Public source As Rscript

        Public Function GetStackTrace(token As Token) As StackFrame
            Return New StackFrame With {
                .File = source.ToString,
                .Line = token.span.line,
                .Method = New Method With {
                    .Method = token.text,
                    .[Module] = "n/a",
                    .[Namespace] = "R#"
                }
            }
        End Function
    End Class
End Namespace
