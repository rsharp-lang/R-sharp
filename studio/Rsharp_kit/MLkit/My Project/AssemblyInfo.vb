Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices

' General Information about an assembly is controlled through the following
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes
#if netcore5=0 then 
<Assembly: AssemblyTitle("MLkit: R# machine learning toolkit library")>
<Assembly: AssemblyDescription("MLkit: R# machine learning toolkit library")>
<Assembly: AssemblyCompany("")>
<Assembly: AssemblyProduct("MLkit")>
<Assembly: AssemblyCopyright("Copyright © xie.guigang@gcmodeller.org 2020")>
<Assembly: AssemblyTrademark("")>

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("407c60ac-6f6a-44d4-b6a7-d1d63908efbe")>

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

<Assembly: AssemblyVersion("1.3210.*")>
<Assembly: AssemblyFileVersion("2.3321.*")>
#end if