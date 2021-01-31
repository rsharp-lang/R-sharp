#Region "Microsoft.VisualBasic::f6085eaf7bf4bc20f0e6cdc3543ddd77, studio\Rsharp_kit\roxygenNet\rdocumentation.vb"

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

    ' Module rdocumentation
    ' 
    '     Function: rdocumentation
    ' 
    ' /********************************************************************************/

#End Region


Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.Interface

<Package("rdocumentation")>
Public Module rdocumentation

    <ExportAPI("documentation")>
    Public Function rdocumentation(func As RFunction, Optional env As Environment = Nothing) As String
        Return New [function]().createHtml(func, env)
    End Function
End Module

