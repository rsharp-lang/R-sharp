#Region "Microsoft.VisualBasic::d2fcb9a4f2e546adee8f38f52e9e2712, R#\Language\TokenLine.vb"

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

    '     Class TokenLine
    ' 
    '         Properties: length, tokens
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: StripDelimiterTokens, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language

    Public Class TokenLine

        Public ReadOnly Property tokens As Token()

        ''' <summary>
        ''' the size of the <see cref="tokens"/> array
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property length As Integer
            Get
                Return tokens.Length
            End Get
        End Property

        Default Public ReadOnly Property getToken(i As Integer) As Token
            Get
                If i < 0 Then
                    i = tokens.Length + i
                End If

                Return tokens(i)
            End Get
        End Property

        Sub New(tokens As IEnumerable(Of Token))
            Me.tokens = tokens
        End Sub

        Friend Function StripDelimiterTokens() As TokenLine
            _tokens = tokens _
                .Where(Function(t)
                           Return Not t.name = TokenType.delimiter
                       End Function) _
                .ToArray

            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return tokens.Select(Function(t) t.text).JoinBy(" ")
        End Function
    End Class
End Namespace
