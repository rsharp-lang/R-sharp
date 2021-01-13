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
