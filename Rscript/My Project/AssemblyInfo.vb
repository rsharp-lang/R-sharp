Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices
#if netcore5=0 then
' General Information about an assembly is controlled through the following
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes

<Assembly: AssemblyTitle("R# scripting host")>
<Assembly: AssemblyDescription("R# scripting host")>
<Assembly: AssemblyCompany("SMRUCC genomics Institute <genomics@SMRUCC.org>")>
<Assembly: AssemblyProduct("GCModeller")>
<Assembly: AssemblyCopyright("Copyright © SMRUCC genomics  2020")>
<Assembly: AssemblyTrademark("R#")>

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("16d477b1-e7fb-41eb-9b61-7ea75c5d2939")>

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

<Assembly: AssemblyVersion("1.99.*")>
<Assembly: AssemblyFileVersion("2.321.*")>
#end if
