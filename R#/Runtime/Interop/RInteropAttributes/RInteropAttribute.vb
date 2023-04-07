#Region "Microsoft.VisualBasic::da8538dfd866e3648dfac4be1a6d93a1, E:/GCModeller/src/R-sharp/R#//Runtime/Interop/RInteropAttributes/RInteropAttribute.vb"

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

    '   Total Lines: 78
    '    Code Lines: 40
    ' Comment Lines: 22
    '   Blank Lines: 16
    '     File Size: 2.81 KB


    '     Class RInteropAttribute
    ' 
    ' 
    ' 
    '     Class RInitializeAttribute
    ' 
    ' 
    ' 
    '     Class RByRefValueAssignAttribute
    ' 
    ' 
    ' 
    '     Class RNameAliasAttribute
    ' 
    '         Properties: [alias]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class RBasePackageAttribute
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class RTypeExportAttribute
    ' 
    '         Properties: model, name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Development.Package

Namespace Runtime.Interop

    <AttributeUsage(AttributeTargets.All, AllowMultiple:=False, Inherited:=True)>
    Public Class RInteropAttribute : Inherits Attribute
    End Class

    ''' <summary>
    ''' 如果使用sub new初始化的话，则在导入程序包的时候sub new里面的代码是不会被自动调用的
    ''' 对sub new构造函数的调用只在发生实际的api调用的时候才会发生
    ''' 所以才在这里使用这个属性来标记一些需要在导入程序包的时候自动化运行的代码来进行一些初始化操作
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class RInitializeAttribute : Inherits RInteropAttribute

    End Class

    ''' <summary>
    ''' 这个参数是接受``a(x) &lt;- y``操作之中的``y``结果值的
    ''' </summary>
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RByRefValueAssignAttribute : Inherits RInteropAttribute
    End Class

    <AttributeUsage(AttributeTargets.Parameter Or AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class RNameAliasAttribute : Inherits RInteropAttribute

        Public ReadOnly Property [alias] As String

        Sub New([alias] As String)
            Me.alias = [alias]
        End Sub

        Public Overrides Function ToString() As String
            Return [alias]
        End Function
    End Class

    Public Class RBasePackageAttribute : Inherits PackageAttribute

        Public Sub New([Namespace] As String)
            Call MyBase.New([Namespace])
        End Sub
    End Class

    ''' <summary>
    ''' export data type to ``R#`` runtime environment
    ''' </summary>
    ''' <remarks>
    ''' type information will be imports into current runtime 
    ''' context environment when do package module imports at 
    ''' function invoke 
    ''' <see cref="ImportsPackage.ImportsStaticInternalImpl"/>
    ''' </remarks>
    ''' 
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=True, Inherited:=True)>
    Public Class RTypeExportAttribute : Inherits RInteropAttribute

        ''' <summary>
        ''' type name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property name As String
        Public ReadOnly Property model As Type

        Sub New(name As String, target As Type)
            Me.name = name
            Me.model = target
        End Sub

        Public Overrides Function ToString() As String
            Return $"imports '{name}' = {model.FullName}"
        End Function

    End Class
End Namespace
