#Region "Microsoft.VisualBasic::e44f3cdc456156e6eac7eb505097a9d8, R#\Language\TokenIcer\Escapes.vb"

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

    '   Total Lines: 46
    '    Code Lines: 22 (47.83%)
    ' Comment Lines: 19 (41.30%)
    '    - Xml Docs: 78.95%
    ' 
    '   Blank Lines: 5 (10.87%)
    '     File Size: 1.19 KB


    '     Class Escapes
    ' 
    '         Function: ToString
    ' 
    '         Sub: reset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Language.TokenIcer

    ''' <summary>
    ''' common language escape model between multiple language syntax parser
    ''' </summary>
    Public Class Escapes

        Public comment, [string] As Boolean
        Public stringEscape As Char

        ''' <summary>
        ''' apply for the block comment in other language parser, example as:
        ''' 
        ''' 1. javascript/typescript 
        ''' 
        ''' /**
        '''  *
        '''  *
        ''' */
        ''' 
        ''' 2. matlab/octave language
        ''' 
        ''' %{
        ''' ...
        ''' %}
        ''' </summary>
        Public isBlockComment As Boolean

        Public Sub reset()
            comment = False
            [string] = False
            stringEscape = Nothing
            isBlockComment = False
        End Sub

        Public Overrides Function ToString() As String
            If comment Then
                Return "comment"
            ElseIf [string] Then
                Return $"{stringEscape}string{stringEscape}"
            Else
                Return "code"
            End If
        End Function
    End Class
End Namespace
