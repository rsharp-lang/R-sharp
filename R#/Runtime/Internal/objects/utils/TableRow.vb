#Region "Microsoft.VisualBasic::f4b1442b8ca5e864fef16486fb6cc0f1, R#\Runtime\Internal\objects\utils\TableRow.vb"

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
    '    Code Lines: 34
    ' Comment Lines: 6
    '   Blank Lines: 6
    '     File Size: 1.57 KB


    '     Class TableRow
    ' 
    '         Function: CreateRows, (+2 Overloads) ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Text
Imports any = Microsoft.VisualBasic.Scripting

Namespace Runtime.Internal.Object.Utils

    ''' <summary>
    ''' table row object for save dataframe to file
    ''' </summary>
    Public Class TableRow

        Friend cells As Object()
        ''' <summary>
        ''' the row id
        ''' </summary>
        Friend id As String

        Public Overrides Function ToString() As String
            Return ToString(",")
        End Function

        Public Overloads Function ToString(format As String) As String
            Return cells _
                .Select(Function(obj) any.ToString(obj)) _
                .Select(Function(str)
                            If str.IndexOfAny({","c, ASCII.TAB}) > -1 Then
                                Return $"""{str}"""
                            Else
                                Return str
                            End If
                        End Function) _
                .JoinBy(format)
        End Function

        Public Shared Function CreateRows(table As dataframe, Optional orders As String() = Nothing) As TableRow()
            Return table _
                .forEachRow(colKeys:=orders) _
                .Select(Function(r)
                            Return New TableRow With {
                                .id = r.name,
                                .cells = r.value
                            }
                        End Function) _
                .ToArray
        End Function
    End Class
End Namespace
