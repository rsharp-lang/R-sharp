Imports System.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates

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

    Sub New(field As FieldInfo)
        With field
            getValue = StaticFieldGet(.DeclaringType, .Name)
            setValue = StaticFieldSet(.DeclaringType, .Name)
        End With

        Name = field.Name
    End Sub

    Sub New([property] As PropertyInfo)
        With [property]
            getValue = StaticPropertyGet(.DeclaringType, .Name)
            setValue = StaticPropertySet(.DeclaringType, .Name)
        End With

        Name = [property].Name
    End Sub

End Class
