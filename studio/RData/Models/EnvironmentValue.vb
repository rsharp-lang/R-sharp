#Region "Microsoft.VisualBasic::2a710c885b1bbbbfe3d9dbaa9a354820, F:/GCModeller/src/R-sharp/studio/RData//Models/EnvironmentValue.vb"

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
    '    Code Lines: 9
    ' Comment Lines: 3
    '   Blank Lines: 4
    '     File Size: 354 B


    '     Class EnvironmentValue
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.RDataSet.Struct.LinkedList

Namespace Struct

    ''' <summary>
    ''' Value of an environment.
    ''' </summary>
    Public Class EnvironmentValue

        Public locked As Boolean
        Public enclosure As RObject
        Public frame As RObject
        Public hash_table As RObject

    End Class
End Namespace
