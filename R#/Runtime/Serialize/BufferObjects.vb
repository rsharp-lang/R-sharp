﻿#Region "Microsoft.VisualBasic::36331341dde95b87d782c4b8a9e0e419, R#\Runtime\Serialize\BufferObjects.vb"

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

    '   Total Lines: 21
    '    Code Lines: 13 (61.90%)
    ' Comment Lines: 3 (14.29%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 5 (23.81%)
    '     File Size: 585 B


    '     Enum BufferObjects
    ' 
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
        <Description("application/rscript")> rscript

        <Description("rstudio/debug-message")> message = 500
    End Enum
End Namespace
