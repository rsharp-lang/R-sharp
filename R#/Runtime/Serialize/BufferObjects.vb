#Region "Microsoft.VisualBasic::97c7f230b044bd19169c8fcec5098c17, R#\Runtime\Serialize\BufferObjects.vb"

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

'     Enum BufferObjects
' 
'         bitmap, dataframe, list, raw
' 
'  
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace Runtime.Serialize

    ''' <summary>
    ''' 
    ''' </summary>
    Public Enum BufferObjects
        <Description("application/octet-stream")> raw

        <Description("html/text")> text = 100
        <Description("image/bitmap")> bitmap

        <Description("data/vector")> vector = 200
        <Description("application/json")> list
        <Description("text/csv")> dataframe

        <Description("rstudio/debug-message")> message = 500
    End Enum
End Namespace
