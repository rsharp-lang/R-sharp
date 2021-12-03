#Region "Microsoft.VisualBasic::b8591dda9f3a03a93c2d41c13f8cf778, studio\RData\Enums\FileTypes.vb"

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

    ' Enum FileTypes
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
    ''' Type of file containing a R file.
    ''' </summary>
    Public Enum FileTypes
        Unknown = 0

        <Description("bz2")> bzip2
        <Description("gzip")> gzip
        <Description("xz")> xz
        <Description("rdata version 2 (binary)")> rdata_binary_v2
        <Description("rdata version 3 (binary)")> rdata_binary_v3
    End Enum
End Namespace