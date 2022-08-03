#Region "Microsoft.VisualBasic::7a4d7a33634a8c727a6d74e91a01bf8f, R-sharp\studio\Rsharp_kit\webKit\My Project\AssemblyInfo.vb"

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
    '     File Size: 1.22 KB


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

<Assembly: AssemblyTitle("Http and web toolkit for R# scripting")>
<Assembly: AssemblyDescription("Http and web toolkit for R# scripting")>
<Assembly: AssemblyCompany("SMRUCC")>
<Assembly: AssemblyProduct("R#")>
<Assembly: AssemblyCopyright("Copyright © xie.guigang@gcmodeller.org 2020")>
<Assembly: AssemblyTrademark("R#")>

<Assembly: ComVisible(False)>

'如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
<Assembly: Guid("492d2598-6f6e-4a3e-8437-60cc837ede15")>

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

<Assembly: AssemblyVersion("1.0.0.0")>
<Assembly: AssemblyFileVersion("1.0.0.0")>
#end if
<Assembly: RPackageModule>
