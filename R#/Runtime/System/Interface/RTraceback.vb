﻿#Region "Microsoft.VisualBasic::5b4c49aea06241b33981be650d231509, R#\Runtime\System\Interface\RTraceback.vb"

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

    '   Total Lines: 16
    '    Code Lines: 9 (56.25%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (43.75%)
    '     File Size: 363 B


    '     Interface IRuntimeTrace
    ' 
    '         Properties: stackFrame
    ' 
    '     Interface INamespaceReferenceSymbol
    ' 
    '         Properties: [namespace]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace Runtime.Components.Interface

    Public Interface IRuntimeTrace

        ReadOnly Property stackFrame As StackFrame

    End Interface

    Public Interface INamespaceReferenceSymbol

        ReadOnly Property [namespace] As String

    End Interface
End Namespace
