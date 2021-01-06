Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices

' 有关程序集的一般信息由以下
' 控制。更改这些特性值可修改
' 与程序集关联的信息。

'查看程序集特性的值
#if netcore5=0 then
<Assembly: AssemblyTitle("roxygenNet: In-Line Documentation for R")>
<Assembly: AssemblyDescription("In-Line Documentation for R")>
<Assembly: AssemblyCompany("")>
<Assembly: AssemblyProduct("roxygenNet")>
<Assembly: AssemblyCopyright("Copyright © i@xieguigang.me 2020")>
<Assembly: AssemblyTrademark("")>

<Assembly: ComVisible(False)>

'如果此项目向 COM 公开，则下列 GUID 用于 typelib 的 ID
<Assembly: Guid("9403880e-316f-41c5-b4c2-8df21a99a84f")>

' 程序集的版本信息由下列四个值组成: 
'
'      主版本
'      次版本
'      生成号
'      修订号
'
'可以指定所有这些值，也可以使用“生成号”和“修订号”的默认值
'通过使用 "*"，如下所示:
' <Assembly: AssemblyVersion("1.0.*")>

<Assembly: AssemblyVersion("1.0.0.0")>
<Assembly: AssemblyFileVersion("1.0.0.0")>
#end if