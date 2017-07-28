Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Runtime

    ''' <summary>
    ''' The variable model in R# language
    ''' </summary>
    Public Class Variable : Implements INamedValue, Value(Of Object).IValueOf

        Public Property Name As String Implements IKeyedEntity(Of String).Key
        ''' <summary>
        ''' 当前的这个变量被约束的类型
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Constraint As TypeCodes
        Public Overridable Property Value As Object Implements Value(Of Object).IValueOf.value
        ''' <summary>
        ''' <see cref="RType.Identity"/>, key for <see cref="Environment.Types"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property TypeID As String

        ''' <summary>
        ''' Get the type of the current object <see cref="Value"/>.
        ''' </summary>
        ''' <returns></returns>
        Public Overloads ReadOnly Property [TypeOf] As Type
            Get
                If Value Is Nothing Then
                    Return GetType(Object)
                Else
                    Return Value.GetType
                End If
            End Get
        End Property

        ''' <summary>
        ''' 当前的这个变量的值所具有的类型代码
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TypeCode As TypeCodes
            Get
                Return Me.TypeOf.GetRTypeCode
            End Get
        End Property

        ''' <summary>
        ''' 当前的变量值的类型代码是否满足类型约束条件
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ConstraintValid As Boolean
            Get
                If Constraint = TypeCodes.generic Then
                    Return True   ' 没有类型约束，则肯定是有效的
                Else
                    Return Constraint = TypeCode
                End If
            End Get
        End Property

        Sub New(constraint As TypeCodes)
            Me.Constraint = constraint
        End Sub

        Public Overrides Function ToString() As String
            Return $"Dim {Name} As ({TypeCode}){Me.TypeOf.FullName} = {CStrSafe(Value)}"
        End Function
    End Class
End Namespace