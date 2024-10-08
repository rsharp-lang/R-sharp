﻿#Region "Microsoft.VisualBasic::38a6863ed2943ec88557642d5b878f0d, R#\Runtime\Vectorization\UnaryNot.vb"

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

    '   Total Lines: 13
    '    Code Lines: 10 (76.92%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (23.08%)
    '     File Size: 371 B


    '     Module UnaryNot
    ' 
    '         Function: [Not]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Runtime.Vectorization

    Module UnaryNot

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function [Not](x As Boolean()) As Boolean()
            Return (From xi As Boolean In x Select Not xi).ToArray
        End Function
    End Module
End Namespace
