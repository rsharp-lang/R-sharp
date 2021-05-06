#Region "Microsoft.VisualBasic::02e819c8b9f6de0bebbe5ee448dda48e, R#\Runtime\Interop\CType\ICType.vb"

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

    '     Interface ICTypeList
    ' 
    '         Function: toList
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Interop.CType

    ''' <summary>
    ''' An interface for cast current object to R# list object.
    ''' </summary>
    Public Interface ICTypeList

        ''' <summary>
        ''' cast current object to R# list object.
        ''' </summary>
        ''' <returns></returns>
        Function toList() As list

    End Interface

End Namespace
