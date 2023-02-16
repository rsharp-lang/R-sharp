#Region "Microsoft.VisualBasic::27e774b1b1515d718894f1701de73bc4, E:/GCModeller/src/R-sharp/R#//Runtime/Interop/RsharpApi/ListObjectArgumentMargin.vb"

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

    '   Total Lines: 26
    '    Code Lines: 8
    ' Comment Lines: 17
    '   Blank Lines: 1
    '     File Size: 817 B


    '     Enum ListObjectArgumentMargin
    ' 
    '         invalid, left, none, right
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.Interop

    ''' <summary>
    ''' indicates the location of ``...`` list arguments
    ''' </summary>
    Public Enum ListObjectArgumentMargin
        ''' <summary>
        ''' the function didn't contains any ``...``
        ''' </summary>
        none
        ''' <summary>
        ''' invalid function information
        ''' </summary>
        invalid
        ''' <summary>
        ''' the first parameter of the function is a ``...`` list parameter
        ''' </summary>
        left
        ''' <summary>
        ''' the last parameter or the last -1 parameter(if the last parameter 
        ''' is a preserved <see cref="Environment"/> object) of the function 
        ''' is a ``...`` list parameter.
        ''' </summary>
        right
    End Enum
End Namespace
