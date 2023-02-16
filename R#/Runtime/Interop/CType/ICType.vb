#Region "Microsoft.VisualBasic::8588970bbe4e932b7f95d54aa39f3362, E:/GCModeller/src/R-sharp/R#//Runtime/Interop/CType/ICType.vb"

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

    '   Total Lines: 18
    '    Code Lines: 6
    ' Comment Lines: 7
    '   Blank Lines: 5
    '     File Size: 426 B


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
