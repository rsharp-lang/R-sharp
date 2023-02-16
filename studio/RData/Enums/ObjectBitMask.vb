#Region "Microsoft.VisualBasic::7ffd67ef553080387e65644dc4fd6518, E:/GCModeller/src/R-sharp/studio/RData//Enums/ObjectBitMask.vb"

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

    '   Total Lines: 11
    '    Code Lines: 7
    ' Comment Lines: 3
    '   Blank Lines: 1
    '     File Size: 263 B


    '     Enum ObjectBitMask
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Flags

    ''' <summary>
    ''' R object attribute data mask
    ''' </summary>
    Public Enum ObjectBitMask
        IS_OBJECT_BIT_MASK = 1 << 8
        HAS_ATTR_BIT_MASK = 1 << 9
        HAS_TAG_BIT_MASK = 1 << 10
    End Enum
End Namespace
