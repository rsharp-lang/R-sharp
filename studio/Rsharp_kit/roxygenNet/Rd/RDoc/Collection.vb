#Region "Microsoft.VisualBasic::2b1cf73859d154c522e738445d250e46, E:/GCModeller/src/R-sharp/studio/Rsharp_kit/roxygenNet//Rd/RDoc/Collection.vb"

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

    '   Total Lines: 15
    '    Code Lines: 9
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 389 B


    ' Class Item
    ' 
    '     Properties: description, name
    ' 
    ' Class Enumerate
    ' 
    '     Properties: items
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

Public Class Item : Implements INamedValue

    Public Property name As String Implements IKeyedEntity(Of String).Key
    Public Property description As Doc

End Class

Public Class Enumerate

    Public Property items As Doc()

End Class
