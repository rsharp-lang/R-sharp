Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Delegates
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
        Public ReadOnly Property types As Dictionary(Of String, RType)

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

        Sub New()
            variables = New Dictionary(Of Variable)
            types = New Dictionary(Of String, RType)
        End Sub

        Sub New(parent As Environment, stackTag$)
            Me.parent = parent
            Me.stackTag = stackTag
        End Sub

        ''' <summary>
        ''' Variable declare
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <param name="value"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function Push(name$, value As Object, Optional type As TypeCodes = TypeCodes.generic) As Object
            If variables.ContainsKey(name) Then
                Throw New Exception(String.Format(AlreadyExists, name))
            ElseIf Not value Is Nothing Then
                value = asRVector(type, value)
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

                Return value
            End With
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="value">
        ''' 应该是确保这个变量值是非空的
        ''' </param>
        ''' <returns></returns>
        Private Function asRVector(type As TypeCodes, value As Object) As Object
            If type = TypeCodes.generic Then
                ' 没有定义as type做类型约束的时候
                ' 会需要通过值来推断
                type = value.GetType.GetRTypeCode
            End If

            Select Case type
                Case TypeCodes.boolean
                    value = Runtime.asVector(Of Boolean)(value)
                Case TypeCodes.char
                    If value.GetType Is GetType(Char) Then
                        value = {value.ToString}
                    Else
                        value = {DirectCast(value, IEnumerable(Of Char)).CharString}
                    End If
                Case TypeCodes.double
                    value = Runtime.asVector(Of Double)(value)
                Case TypeCodes.integer
                    value = Runtime.asVector(Of Long)(value)
                Case TypeCodes.string
                    value = Runtime.asVector(Of String)(value)
            End Select

            Return value
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