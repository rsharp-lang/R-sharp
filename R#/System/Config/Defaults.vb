#Region "Microsoft.VisualBasic::7894811669d32cdde9e6ea8f07496adc, R#\System\Config\Defaults.vb"

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

    '   Total Lines: 8
    '    Code Lines: 5 (62.50%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: NaN%
    ' 
    '   Blank Lines: 3 (37.50%)
    '     File Size: 225 B


    '     Module Defaults
    ' 
    '         Properties: HTTPUserAgent
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Development.Configuration

    Module Defaults

        Public ReadOnly Property HTTPUserAgent As String = $"SMRUCC/R# {App.Version}; R (3.4.4 x86_64-w64-mingw32 x86_64 mingw32)"

    End Module
End Namespace
