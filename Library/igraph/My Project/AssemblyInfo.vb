#Region "Microsoft.VisualBasic::9bae3c7a0fc4b7d856ce638430983050, R-sharp\Library\igraph\My Project\AssemblyInfo.vb"

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

    '   Total Lines: 38
    '    Code Lines: 17
    ' Comment Lines: 15
    '   Blank Lines: 6
    '     File Size: 1.33 KB


    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports SMRUCC.Rsharp.Runtime.Interop
#If netcore5 = 0 Then
' General Information about an assembly is controlled through the following
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes

<Assembly: AssemblyTitle("R# network graph analysis library module")>
<Assembly: AssemblyDescription("R# network graph analysis library module")>
<Assembly: AssemblyCompany("SMRUCC")>
<Assembly: AssemblyProduct("R#")>
<Assembly: AssemblyCopyright("Copyright © SMRUCC 2019")>
<Assembly: AssemblyTrademark("R#")>

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("1cf52e3b-d166-44f9-b394-9915bc85dd3a")>

' Version information for an assembly consists of the following four values:
'
'      Major Version
'      Minor Version
'      Build Number
'      Revision
'
' You can specify all the values or you can default the Build and Revision Numbers
' by using the '*' as shown below:
' <Assembly: AssemblyVersion("1.0.*")>

<Assembly: AssemblyVersion("1.231.*")>
<Assembly: AssemblyFileVersion("1.20.*")>
#End If
<Assembly: RPackageModule>
