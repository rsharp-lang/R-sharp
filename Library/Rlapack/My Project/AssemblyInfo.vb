Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports SMRUCC.Rsharp.Runtime.Interop

' General Information about an assembly is controlled through the following
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes
#if netcore5= 0 then 
<Assembly: AssemblyTitle("R# mathematics and data science library module.")>
<Assembly: AssemblyDescription("R# mathematics and data science library module.")>
<Assembly: AssemblyCompany("SMRUCC/Rsharp")>
<Assembly: AssemblyProduct("R-sharp")>
<Assembly: AssemblyCopyright("Copyright © xie.guigang@gcmodeller.org 2020")>
<Assembly: AssemblyTrademark("GCModeller")>

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("54e71e9d-4b07-4fba-bd81-2fcf24f5066b")>

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

<Assembly: AssemblyVersion("1.331.*")>
<Assembly: AssemblyFileVersion("2.981.*")>
#end if
<Assembly: RPackageModule>