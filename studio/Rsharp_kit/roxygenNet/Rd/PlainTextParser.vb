#Region "Microsoft.VisualBasic::88866724af55290d1f10e3e272ec491b, E:/GCModeller/src/R-sharp/studio/Rsharp_kit/roxygenNet//Rd/PlainTextParser.vb"

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

    '   Total Lines: 26
    '    Code Lines: 20
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 602 B


    ' Class PlainTextParser
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: GetCurrentText
    ' 
    '     Sub: walkChar
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Marshal

Public Class PlainTextParser : Inherits RDocParser

    Protected endContent As Boolean = False

    Friend Sub New(text As Pointer(Of Char))
        Me.text = text
    End Sub

    Protected Overrides Sub walkChar(c As Char)
        If c = "}"c Then
            endContent = True
        Else
            buffer += c
        End If
    End Sub

    Public Function GetCurrentText() As String
        Do While Not endContent
            Call walkChar(++text)
        Loop

        Return New String(buffer)
    End Function
End Class
