Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq

Namespace Runtime

    Public Class Environment

        ''' <summary>
        ''' 最顶层的closure环境的parent是空值来的
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property parent As Environment
        ''' <summary>
        ''' The name of this current stack closure.(R function name, closure id, etc)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property stackTag As String
        Public ReadOnly Property variables As Dictionary(Of Variable)

        ''' <summary>
        ''' 当前的环境是否为最顶层的全局环境？
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isGlobal As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return parent Is Nothing
            End Get
        End Property

        Public ReadOnly Property GlobalEnvironment As Environment
            Get
                If isGlobal Then
                    Return Me
                Else
                    Return parent.GlobalEnvironment
                End If
            End Get
        End Property

        Const AlreadyExists$ = "Variable ""{0}"" is already existed, can not declare it again!"
        Const ConstraintInvalid$ = "Value can not match the type constraint!!! ({0} <--> {1})"

        ''' <summary>
        ''' Variable declare
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <param name="value"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function Push(name$, value As Object, Optional type As TypeCodes = TypeCodes.generic) As Integer
            If variables.ContainsKey(name) Then
                Throw New Exception(String.Format(AlreadyExists, name))
            End If

            With New Variable(type) With {
                .name = name,
                .value = value
            }
                If Not .constraintValid Then
                    Throw New Exception(String.Format(ConstraintInvalid, .typeCode, type))
                Else
                    Call .DoCall(AddressOf variables.Add)
                End If

                ' 位置值，相当于函数指针
                Return variables.Count - 1
            End With
        End Function

        Public Overrides Function ToString() As String
            If isGlobal Then
                Return $"Global({NameOf(Environment)})"
            Else
                Return parent?.ToString & "->" & stackTag
            End If
        End Function
    End Class
End Namespace