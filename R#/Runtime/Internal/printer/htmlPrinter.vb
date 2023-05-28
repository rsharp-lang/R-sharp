#Region "Microsoft.VisualBasic::8d5801b5064f688329bb907cfabea170, F:/GCModeller/src/R-sharp/R#//Runtime/Internal/printer/htmlPrinter.vb"

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
    '    Code Lines: 17
    ' Comment Lines: 5
    '   Blank Lines: 6
    '     File Size: 1007 B


    '     Module htmlPrinter
    ' 
    '         Function: GetHtml
    ' 
    '         Sub: AttachHtmlFormatter
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Internal

    Public Module htmlPrinter

        Public Const toHtml_apiName As String = "html"

        ''' <summary>
        ''' Create html document from given object. (<see cref="Object"/> -> html <see cref="String"/>)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="formatter"></param>
        Public Sub AttachHtmlFormatter(Of T)(formatter As GenericFunction)
            Call generic.add("html", GetType(T), formatter)
        End Sub

        Friend Function GetHtml(x As Object, args As list, env As Environment) As Object
            Dim keyType As Type = x.GetType

            If keyType Is GetType(vbObject) Then
                Return GetHtml(DirectCast(x, vbObject).target, args, env)
            Else
                Return generic.invokeGeneric(args, x, env, funcName:=toHtml_apiName)
            End If
        End Function
    End Module
End Namespace
