#Region "Microsoft.VisualBasic::4b20ce65b01ccbeaf5cb9d0ef37cc151, R-sharp\studio\Rserver\RInvoke.vb"

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

    '   Total Lines: 14
    '    Code Lines: 8
    ' Comment Lines: 3
    '   Blank Lines: 3
    '     File Size: 446.00 B


    ' Class RInvoke
    ' 
    '     Properties: content_type, err, server_time, warnings
    ' 
    ' /********************************************************************************/

#End Region

Imports Flute.Http.AppEngine
Imports SMRUCC.Rsharp.Runtime.Components

''' <summary>
''' <see cref="RInvoke.info"/>是一个base64字符串，主要是为了兼容文本与图像输出
''' </summary>
Public Class RInvoke : Inherits JsonResponse(Of String)

    Public Property content_type As String
    Public Property warnings As Message()
    Public Property err As Message
    Public Property server_time As Date = Now

End Class
