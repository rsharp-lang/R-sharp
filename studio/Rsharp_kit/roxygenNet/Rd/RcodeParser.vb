#Region "Microsoft.VisualBasic::3a06bb0a8dd8ef594a31ac929c204658, R-sharp\studio\Rsharp_kit\roxygenNet\Rd\RcodeParser.vb"

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

    '   Total Lines: 53
    '    Code Lines: 37
    ' Comment Lines: 10
    '   Blank Lines: 6
    '     File Size: 1.67 KB


    ' Class RcodeParser
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: walkChar
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Marshal

''' <summary>
''' ``examples``标签里面为R代码，所以可能会出现字符串和代码注释
''' 因为字符串或者注释之中可能会存在``{}``符号，所以会需要额外的
''' 信息来解决这个问题
''' </summary>
Public Class RcodeParser : Inherits PlainTextParser

    ''' <summary>
    ''' 因为R代码示例之中任然可能含有``{}``符号，所以会需要使用一个栈对象来确定文本结束的位置
    ''' </summary>
    Dim codeStack As New Stack(Of Char)
    Dim commentEscape As Boolean
    Dim stringEscape As Boolean

    Friend Sub New(text As Pointer(Of Char))
        Call MyBase.New(text)
        Call Me.codeStack.Push("{")
    End Sub

    Protected Overrides Sub walkChar(c As Char)
        If c = "#"c Then
            If stringEscape Then
                ' Do nothing
            ElseIf Not commentEscape Then
                commentEscape = True
            End If
        ElseIf c = """"c Then
            If commentEscape Then
                ' Do nothing
            ElseIf Not stringEscape Then
                stringEscape = True
            ElseIf stringEscape Then
                stringEscape = False
            End If
        End If

        If (Not commentEscape) AndAlso (Not stringEscape) Then
            If c = "{"c Then
                codeStack.Push("{"c)
            ElseIf c = "}"c Then
                codeStack.Pop()
            End If
        End If

        If codeStack.Count = 0 Then
            endContent = True
        Else
            buffer += c
        End If
    End Sub
End Class

