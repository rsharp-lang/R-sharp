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

        ''' <summary>
        ''' Get/set variable value
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' If the current stack does not contains the target variable, then the program will try to find the variable in his parent
        ''' if variable in format like [var], then it means a global or parent environment variable
        ''' </remarks>
        Default Public Property value(name As String) As Variable
            Get
                If (name.First = "["c AndAlso name.Last = "]"c) Then
                    Return GlobalEnvironment(name.GetStackValue("[", "]"))
                End If

                If variables.ContainsKey(name) Then
                    Return variables(name)
                ElseIf Not parent Is Nothing Then
                    Return parent(name)
                Else
                    Throw New EntryPointNotFoundException(name & " was not found in any stack enviroment!")
                End If
            End Get
            Set(value As Variable)
                If name.First = "["c AndAlso name.Last = "]"c Then
                    GlobalEnvironment(name.GetStackValue("[", "]")) = value
                Else
                    variables(name) = value
                End If
            End Set
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