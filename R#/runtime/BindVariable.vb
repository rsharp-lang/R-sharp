#Region "Microsoft.VisualBasic::e027220eddfbcd2575b718b43d9b8f7b, R#\runtime\BindVariable.vb"

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

    '     Class BindVariable
    ' 
    '         Properties: Source, Value
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates

Namespace Runtime

    ''' <summary>
    ''' ``R#``脚本会通过这个变量对象将包之中的模块变量或者Class之中的共享变量作为一个R的内部变量
    ''' </summary>
    Public Class BindVariable : Inherits Variable

        ''' <summary>
        ''' 编译之后所产生的变量值的设置或者获取的过程
        ''' </summary>
        ReadOnly getValue As Func(Of Object), setValue As Action(Of Object)

        ''' <summary>
        ''' 这个属性获取的是包之中的模块变量的值
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Property Value As Object
            Get
                Return getValue()
            End Get
            Set(value As Object)
                Call setValue(value)
                MyBase.Value = value
            End Set
        End Property

        ''' <summary>
        ''' 这个变量对象在运行时环境之中的原来的位置
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Source As String

        Sub New(field As FieldInfo)
            Call Me.New(src:=field, [typeOf]:=field.FieldType)

            With field
                getValue = StaticFieldGet(.DeclaringType, .Name)
                setValue = StaticFieldSet(.DeclaringType, .Name)
            End With
        End Sub

        Private Sub New(src As MemberInfo, [typeOf] As Type)
            Call MyBase.New([typeOf].GetRTypeCode)

            Name = src.Name
            Source = src.Source
        End Sub

        Sub New([property] As PropertyInfo)
            Call Me.New(src:=[property], [typeOf]:=[property].PropertyType)

            With [property]
                getValue = StaticPropertyGet(.DeclaringType, .Name)
                setValue = StaticPropertySet(.DeclaringType, .Name)
            End With
        End Sub
    End Class
End Namespace
