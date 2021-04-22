#Region "Microsoft.VisualBasic::48112bd73ad09e6a0ecb1540b498c0a5, R#\Runtime\Internal\objects\utils\TableRow.vb"

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

    '     Class TableRow
    ' 
    '         Function: (+2 Overloads) ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Imports Microsoft.VisualBasic.Text

Namespace Runtime.Internal.Object.Utils

    ''' <summary>
    ''' table row object for save dataframe to file
    ''' </summary>
    Public Class TableRow

        Friend cells As String()

        Public Overrides Function ToString() As String
            Return ToString(",")
        End Function

        Public Overloads Function ToString(format As String) As String
            Return cells _
                .Select(Function(str)
                            If str.IndexOfAny({","c, ASCII.TAB}) > -1 Then
                                Return $"""{str}"""
                            Else
                                Return str
                            End If
                        End Function) _
                .JoinBy(format)
        End Function
    End Class
End Namespace
