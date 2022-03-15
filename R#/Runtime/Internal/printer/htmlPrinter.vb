#Region "Microsoft.VisualBasic::8bdf6bab9130abed11583db7f700513f, R-sharp\R#\Runtime\Internal\printer\htmlPrinter.vb"

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


     Code Statistics:

        Total Lines:   33
        Code Lines:    21
        Comment Lines: 5
        Blank Lines:   7
        File Size:     1.15 KB


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
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Internal

    Public Module htmlPrinter

        ReadOnly RtoHtml As New Dictionary(Of Type, IStringBuilder)

        ''' <summary>
        ''' Create html document from given object. (<see cref="Object"/> -> html <see cref="String"/>)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="formatter"></param>
        Public Sub AttachHtmlFormatter(Of T)(formatter As IStringBuilder)
            RtoHtml(GetType(T)) = formatter
        End Sub

        Friend Function GetHtml(x As Object, env As Environment) As Object
            Dim keyType As Type = x.GetType

            If keyType Is GetType(vbObject) Then
                Return GetHtml(DirectCast(x, vbObject).target, env)
            End If

            If RtoHtml.ContainsKey(keyType) Then
                Return RtoHtml(keyType)(x)
            Else
                Return debug.stop(New InvalidProgramException(keyType.FullName), env)
            End If
        End Function
    End Module
End Namespace
