#Region "Microsoft.VisualBasic::d01444ef2a92d3039732c3dfac268142, F:/GCModeller/src/R-sharp/studio/Rsharp_IL/npy//Language/PythonLine.vb"

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

    '   Total Lines: 28
    '    Code Lines: 21
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 770 B


    ' Class PythonLine
    ' 
    '     Properties: levels
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

#If netcore5 = 1 Then
Imports System.Data
#End If

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Public Class PythonLine : Inherits TokenLine

    Public ReadOnly Property levels As Integer

    Sub New(tokens As Token())
        Call MyBase.New(tokens)

        Me.levels = Me.tokens _
            .TakeWhile(Function(t)
                           Return t.name = TokenType.delimiter
                       End Function) _
            .Count
        Me.StripDelimiterTokens()
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{levels}]" & vbTab & ": " & tokens.Select(Function(t) t.text).JoinBy(" ")
    End Function

End Class
