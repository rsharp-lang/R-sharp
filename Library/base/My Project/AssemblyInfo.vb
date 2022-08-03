#Region "Microsoft.VisualBasic::c6653fadfd3016ba96224cf55114dbe3, R-sharp\Library\base\My Project\AssemblyInfo.vb"

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

    '   Total Lines: 40
    '    Code Lines: 17
    ' Comment Lines: 15
    '   Blank Lines: 8
    '     File Size: 1.15 KB


    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports SMRUCC.Rsharp.Runtime.Interop
#If netcore5 = 0 Then
' 有关程序集的一般信息由以下
' 控制。更改这些特性值可修改
' 与程序集关联的信息。

'查看程序集特性的值

<Assembly: AssemblyTitle("R# base packages")>
<Assembly: AssemblyDescription("")>
<Assembly: AssemblyCompany("SMRUCC")>
<Assembly: AssemblyProduct("R.base")>
<Assembly: AssemblyCopyright("Copyright © SMRUCC 2019")>
<Assembly: AssemblyTrademark("")>

<Assembly: ComVisible(False)>

'如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
<Assembly: Guid("8e73d025-8a5f-4249-8a1b-c2f8a7c924a5")>

' 程序集的版本信息由下列四个值组成: 
'
'      主版本
'      次版本
'      生成号
'      修订号
'
' 可以指定所有值，也可以使用以下所示的 "*" 预置版本号和修订号
' 方法是按如下所示使用“*”: :
' <Assembly: AssemblyVersion("1.0.*")>

<Assembly: AssemblyVersion("1.33.*")>
<Assembly: AssemblyFileVersion("3.1.*")>

#end if

<Assembly: RPackageModule>
