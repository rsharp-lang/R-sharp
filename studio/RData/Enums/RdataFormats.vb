﻿#Region "Microsoft.VisualBasic::946740d77948c2ed1b00f1f60f43a6cf, studio\RData\Enums\RdataFormats.vb"

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

    '   Total Lines: 24
    '    Code Lines: 9 (37.50%)
    ' Comment Lines: 12 (50.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (12.50%)
    '     File Size: 539 B


    '     Enum RdataFormats
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

Namespace Flags

    ''' <summary>
    ''' encoding format of a R file.
    ''' </summary>
    Public Enum RdataFormats
        Unknown = 0

        ''' <summary>
        ''' XDR encode
        ''' </summary>
        <Description("XDR")> XDR
        ''' <summary>
        ''' ASCII encode
        ''' </summary>
        <Description("ASCII")> ASCII
        ''' <summary>
        ''' binary encode
        ''' </summary>
        <Description("binary")> binary
    End Enum
End Namespace
