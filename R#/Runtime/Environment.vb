#Region "Microsoft.VisualBasic::386bd812647080bac6d558953e1652cf, R#\Runtime\Environment.vb"

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

    '     Class Environment
    ' 
    '         Properties: globalEnvironment, isGlobal, messages, parent, stackTag
    '                     types, variables
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: asRVector, Evaluate, FindSymbol, GetEnumerator, IEnumerable_GetEnumerator
    '                   Push, ToString
    ' 
    '         Sub: Clear, Delete, (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime

    ''' <summary>
    ''' 在一个环境对象容器之中，所有的对象都是以变量来表示的
    ''' </summary>
    Public Class Environment : Implements IEnumerable(Of Variable)
        Implements IDisposable

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
        ''' 主要是存储警告消息
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property messages As New List(Of Message)

        Public ReadOnly Property globalEnvironment As GlobalEnvironment
            Get
                Return [global]
            End Get
        End Property

        Friend ReadOnly ifPromise As New List(Of IfBranch.IfPromise)
        Friend [global] As GlobalEnvironment

        ''' <summary>
        ''' 当前的环境是否为最顶层的全局环境？
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isGlobal As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return parent Is Nothing AndAlso TypeOf Me Is GlobalEnvironment
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
                Dim symbol As Variable = FindSymbol(name)

                If symbol Is Nothing Then
                    Return New Variable With {
                        .value = Message.SymbolNotFound(Me, name, TypeCodes.generic)
                    }
                Else
                    Return symbol
                End If
            End Get
            Set(value As Variable)
                If name.First = "["c AndAlso name.Last = "]"c Then
                    globalEnvironment(name.GetStackValue("[", "]")) = value
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
            parent = Nothing
            [global] = Nothing
            stackTag = "<globalEnvironment>"
        End Sub

        Sub New(parent As Environment, stackTag$)
            Call Me.New()

            Me.parent = parent
            Me.stackTag = stackTag
            Me.global = parent.globalEnvironment
        End Sub

        Public Sub Clear()
            Call variables.Clear()
            Call types.Clear()
            Call ifPromise.Clear()
            Call messages.Clear()
        End Sub

        ''' <summary>
        ''' 这个函数查找失败的时候只会返回空值
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function FindSymbol(name As String) As Variable
            If (name.First = "["c AndAlso name.Last = "]"c) Then
                Return globalEnvironment.FindSymbol(name.GetStackValue("[", "]"))
            End If

            If variables.ContainsKey(name) Then
                Return variables(name)
            ElseIf Not parent Is Nothing Then
                Return parent.FindSymbol(name)
            Else
                Return Nothing
            End If
        End Function

        Public Sub Delete(name As String)
            If FindSymbol(name) Is Nothing Then
                Return
            End If

            If variables.ContainsKey(name) Then
                Call variables.Remove(name)
            ElseIf Not parent Is Nothing Then
                Call parent.Delete(name)
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Evaluate(exec As IEnumerable(Of Expression)) As Object()
            Return exec _
                .Select(Function(a) a.Evaluate(Me)) _
                .ToArray
        End Function

        ''' <summary>
        ''' Variable declare
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <param name="value"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function Push(name$, value As Object, Optional type As TypeCodes = TypeCodes.generic) As Object
            If variables.ContainsKey(name) Then
                Return Internal.stop({String.Format(AlreadyExists, name)}, Me)
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
        Friend Shared Function asRVector(type As TypeCodes, value As Object) As Object
            If type = TypeCodes.generic Then
                ' 没有定义as type做类型约束的时候
                ' 会需要通过值来推断
                type = value.GetType.GetRTypeCode
            End If

            Select Case type
                Case TypeCodes.boolean
                    value = Runtime.asVector(Of Boolean)(value)
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
                Return parent?.ToString & " :> " & stackTag
            End If
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Variable) Implements IEnumerable(Of Variable).GetEnumerator
            For Each var As Variable In variables.Values
                Yield var
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    If Not parent Is Nothing Then
                        ' 将当前环境中所产生的诸如警告消息之类的信息
                        ' 传递到顶层的全局环境之中
                        Call parent.messages.AddRange(messages)
                    End If

                    Call Clear()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
