#Region "Microsoft.VisualBasic::92d2f7b31b35ac08d79208c5d9e68db0, D:/GCModeller/src/R-sharp/studio/Rsharp_IL/nts//stdlib/system.vb"

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
    '    Code Lines: 19
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 692 B


    '     Module system
    ' 
    '         Function: [Date], parseFloat, parseInt
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace jsstd

    Public Module system

        <ExportAPI("Date")>
        Public Function [Date]() As Object
            Return Now
        End Function

        <ExportAPI("parseInt")>
        Public Function parseInt(<RRawVectorArgument> x As Object) As Object
            Return CLRVector.asLong(x)
        End Function

        <ExportAPI("parseFloat")>
        Public Function parseFloat(<RRawVectorArgument> x As Object) As Object
            Return CLRVector.asNumeric(x)
        End Function
    End Module
End Namespace
