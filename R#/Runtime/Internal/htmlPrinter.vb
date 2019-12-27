#Region "Microsoft.VisualBasic::012acc9e1247b184574778c868c96b62, R#\Runtime\Internal\htmlPrinter.vb"

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

    '     Module htmlPrinter
    ' 
    '         Function: GetHtml
    ' 
    '         Sub: AttachHtmlFormatter
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization

Namespace Runtime.Internal

    Public Module htmlPrinter

        ReadOnly RtoHtml As New Dictionary(Of Type, IStringBuilder)

        ''' <summary>
        ''' <see cref="Object"/> -> <see cref="String"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="formatter"></param>
        Public Sub AttachHtmlFormatter(Of T)(formatter As IStringBuilder)
            RtoHtml(GetType(T)) = formatter
        End Sub

        Friend Function GetHtml(x As Object) As String
            Dim keyType As Type = x.GetType

            If keyType Is GetType(vbObject) Then
                Return GetHtml(DirectCast(x, vbObject).target)
            End If

            If RtoHtml.ContainsKey(keyType) Then
                Return RtoHtml(keyType)(x)
            Else
                Throw New InvalidProgramException(keyType.FullName)
            End If
        End Function
    End Module
End Namespace
