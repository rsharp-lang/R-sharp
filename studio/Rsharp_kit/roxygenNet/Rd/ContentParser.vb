﻿#Region "Microsoft.VisualBasic::d107b77a62036d506e130d190d1b607e, studio\Rsharp_kit\roxygenNet\Rd\ContentParser.vb"

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

    '   Total Lines: 20
    '    Code Lines: 12 (60.00%)
    ' Comment Lines: 4 (20.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (20.00%)
    '     File Size: 576 B


    ' Class ContentParser
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: GetCurrentContent
    ' 
    '     Sub: walkChar
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Marshal

Public Class ContentParser : Inherits RDocParser

    Friend Sub New(text As Pointer(Of Char))
        Me.text = text
    End Sub

    Protected Overrides Sub walkChar(c As Char)
        Throw New NotImplementedException()
    End Sub

    ''' <summary>
    ''' 可能包含有纯文本以及以下的标签：``\code{}``,``\link{}``,``\enumerate{}``
    ''' </summary>
    ''' <returns></returns>
    Public Function GetCurrentContent() As Doc
        Throw New NotImplementedException
    End Function
End Class
