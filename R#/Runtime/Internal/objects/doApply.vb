#Region "Microsoft.VisualBasic::9c28e113c2c5957cdc5f491da980e205, E:/GCModeller/src/R-sharp/R#//Runtime/Internal/objects/doApply.vb"

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

    '   Total Lines: 19
    '    Code Lines: 14
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 431 B


    '     Enum margins
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module doApply
    ' 
    '         Function: apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.Internal.Object

    Public Enum margins
        row = 1
        column = 2
    End Enum

    Module doApply

        Public Function apply(df As dataframe, margin As margins, FUN As Object, env As Environment) As Object
            If margin = margins.row Then
            Else

            End If

            Throw New NotImplementedException
        End Function
    End Module
End Namespace
