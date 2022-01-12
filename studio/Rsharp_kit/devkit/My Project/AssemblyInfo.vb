﻿Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports SMRUCC.Rsharp.Runtime.Interop

' General Information about an assembly is controlled through the following
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes
#if netcore5=0 then
<Assembly: AssemblyTitle("R# and VB.NET application development kit")>
<Assembly: AssemblyDescription("R# and VB.NET application development kit")>
<Assembly: AssemblyCompany("SMRUCC")>
<Assembly: AssemblyProduct("devkit")>
<Assembly: AssemblyCopyright("Copyright © I@xieguigang.me 2019")>
<Assembly: AssemblyTrademark("")>

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("5b10fbee-8f78-4949-b1ef-4089843f8210")>

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

<Assembly: AssemblyVersion("1.13.*")>
<Assembly: AssemblyFileVersion("1.10.*")>
#end if