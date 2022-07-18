#Region "Microsoft.VisualBasic::eb9e39c8fefe9417d89c4f0259bf229b, R-sharp\studio\Rsharp_kit\roxygenNet\rdocumentation.vb"

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

'   Total Lines: 13
'    Code Lines: 11
' Comment Lines: 0
'   Blank Lines: 2
'     File Size: 471.00 B


' Module rdocumentation
' 
'     Function: rdocumentation
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("rdocumentation")>
Public Module rdocumentation

    <ExportAPI("documentation")>
    Public Function rdocumentation(func As RFunction, template As String, Optional env As Environment = Nothing) As String
        Return New [function]().createHtml(func, template, env)
    End Function

    <ExportAPI("getFunctions")>
    Public Function getFunctions(package As String, Optional env As Environment = Nothing) As list
        Dim apis As NamedValue(Of MethodInfo)() = env.globalEnvironment.packages _
           .FindPackage(package, Nothing) _
           .DoCall(AddressOf ImportsPackage.GetAllApi) _
           .ToArray
        Dim funcs As New list With {.slots = New Dictionary(Of String, Object)}
        Dim func As RMethodInfo

        For Each f As NamedValue(Of MethodInfo) In apis
            func = New RMethodInfo(f)
            funcs.add(func.name, func)
        Next

        Return funcs
    End Function
End Module
