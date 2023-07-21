#Region "Microsoft.VisualBasic::7a297d56a2ac72ab688c5a5957f0436f, D:/GCModeller/src/R-sharp/R#//Runtime/Interop/CType/ICType.vb"

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

    '   Total Lines: 31
    '    Code Lines: 9
    ' Comment Lines: 14
    '   Blank Lines: 8
    '     File Size: 816 B


    '     Interface ICTypeList
    ' 
    '         Function: toList
    ' 
    '     Interface ICTypeDataframe
    ' 
    '         Function: toDataframe
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Interop.CType

    ''' <summary>
    ''' An interface for cast current .NET clr object to R# list object.
    ''' </summary>
    Public Interface ICTypeList

        ''' <summary>
        ''' cast current .NET clr object to R# list object.
        ''' </summary>
        ''' <returns></returns>
        Function toList() As list

    End Interface

    ''' <summary>
    ''' An interface for cast current .NET clr object to R# dataframe object.
    ''' </summary>
    Public Interface ICTypeDataframe

        ''' <summary>
        ''' cast current .NET clr object to R# dataframe object.
        ''' </summary>
        ''' <returns></returns>
        Function toDataframe() As dataframe

    End Interface

End Namespace
