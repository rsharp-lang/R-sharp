#Region "Microsoft.VisualBasic::104d7cebbd8bebf94f4599f4413aaf3c, R#\Runtime\Environment.vb"

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
'         Properties: globalEnvironment, isGlobal, last, messages, parent
'                     stackFrame, stackTrace, symbols, types
' 
'         Constructor: (+3 Overloads) Sub New
' 
'         Function: asRVector, Evaluate, FindSymbol, GetEnumerator, IEnumerable_GetEnumerator
'                   Push, ToString
' 
'         Sub: AddMessage, Clear, Delete, (+2 Overloads) Dispose, setStackInfo
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime

    ''' <summary>
    ''' 在一个环境对象容器之中，所有的对象都是以变量来表示的
    ''' </summary>
    Public Class Environment : Implements IEnumerable(Of Symbol)
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
        Public ReadOnly Property stackFrame As StackFrame
        Public ReadOnly Property stackTrace As StackFrame()
            Get
                Return Me.getEnvironmentStack
            End Get
        End Property

        Public ReadOnly Property symbols As Dictionary(Of Symbol)
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

        ''' <summary>
        ''' get value of the special last variable in R# <see cref="Environment"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property last As Object
            Get
                Return FindSymbol(RInterpreter.lastVariableName)?.value
            End Get
        End Property

        Friend ReadOnly ifPromise As New List(Of IfBranch.IfPromise)

        ''' <summary>
        ''' In the constructor function of <see cref="Runtime.GlobalEnvironment"/>, 
        ''' require attach itself to this parent field
        ''' So this parent field should not be readonly
        ''' </summary>
        Protected [global] As GlobalEnvironment

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
        Default Public Property value(name As String) As Symbol
            Get
                Dim symbol As Symbol = FindSymbol(name)

                If symbol Is Nothing Then
                    Return New Symbol(Message.SymbolNotFound(Me, name, TypeCodes.generic))
                Else
                    Return symbol
                End If
            End Get
            Set(value As Symbol)
                If name.First = "["c AndAlso name.Last = "]"c Then
                    globalEnvironment(name.GetStackValue("[", "]")) = value
                Else
                    symbols(name) = value
                End If
            End Set
        End Property

        Const AlreadyExists$ = "Variable ""{0}"" is already existed, can not declare it again!"
        Const ConstraintInvalid$ = "Value can not match the type constraint!!! ({0} <--> {1})"

        Sub New()
            symbols = New Dictionary(Of Symbol)
            types = New Dictionary(Of String, RType)
            parent = Nothing
            [global] = Nothing
            stackFrame = globalStackFrame

            Log4VB.redirectError = AddressOf redirectError
            Log4VB.redirectWarning = AddressOf redirectWarning
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="parent"></param>
        ''' <param name="stackFrame"></param>
        ''' <param name="isInherits">
        ''' If is inherits mode, then all of the modification in sub-environment will affects the <paramref name="parent"/> environment.
        ''' Otherwise, the modification in sub-environment will do nothing to the <paramref name="parent"/> environment.
        ''' </param>
        Sub New(parent As Environment, stackFrame As StackFrame, isInherits As Boolean)
            Call Me.New()

            Me.parent = parent
            Me.stackFrame = stackFrame
            Me.global = parent.globalEnvironment

            If isInherits Then
                symbols = parent.symbols
                types = parent.types
            End If

            Log4VB.redirectError = AddressOf redirectError
            Log4VB.redirectWarning = AddressOf redirectWarning
        End Sub

        Sub New(globalEnv As GlobalEnvironment)
            Call Me.New

            Me.parent = globalEnv
            Me.global = globalEnv
            Me.stackFrame = globalStackFrame

            Log4VB.redirectError = AddressOf redirectError
            Log4VB.redirectWarning = AddressOf redirectWarning
        End Sub

        Protected Sub redirectError(obj$, msg$, level As MSG_TYPES)
            Call Internal.debug.stop({msg, "location: " & obj}, Me)
        End Sub

        Protected Sub redirectWarning(obj$, msg$, level As MSG_TYPES)
            Call AddMessage({msg, "location: " & obj})
        End Sub

        Public Sub AddMessage(message As Object, Optional level As MSG_TYPES = MSG_TYPES.WRN)
            Internal.Invokes _
                .CreateMessageInternal(message, Me, level) _
                .DoCall(AddressOf messages.Add)
        End Sub

        Public Sub setStackInfo(stackframe As StackFrame)
            _stackFrame = stackframe
        End Sub

        Public Sub Clear()
            If Not parent Is Nothing Then
                ' 20200304 fix bugs for environment inherits mode
                If Not symbols Is parent.symbols Then
                    Call symbols.Clear()
                End If
                If Not types Is parent.types Then
                    Call types.Clear()
                End If
            End If

            Call ifPromise.Clear()
            Call messages.Clear()
        End Sub

        ''' <summary>
        ''' 这个函数查找失败的时候只会返回空值
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function FindSymbol(name As String, Optional [inherits] As Boolean = True) As Symbol
            If (name.First = "["c AndAlso name.Last = "]"c) Then
                Return globalEnvironment.FindSymbol(name.GetStackValue("[", "]"))
            End If

            If symbols.ContainsKey(name) Then
                Return symbols(name)
            ElseIf [inherits] AndAlso Not parent Is Nothing Then
                Return parent.FindSymbol(name)
            Else
                Return Nothing
            End If
        End Function

        Public Sub Delete(name As String)
            If FindSymbol(name) Is Nothing Then
                Return
            End If

            If symbols.ContainsKey(name) Then
                Call symbols.Remove(name)
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
        ''' <param name="mode"></param>
        ''' <returns>返回错误消息或者对象值</returns>
        Public Function Push(name$, value As Object, [readonly] As Boolean, Optional mode As TypeCodes = TypeCodes.generic) As Object
            If symbols.ContainsKey(name) Then
                Return Internal.debug.stop({String.Format(AlreadyExists, name)}, Me)
            ElseIf Not value Is Nothing Then
                value = asRVector(mode, value)
            End If

            With New Symbol(mode) With {
                .name = name
            }
                Call .SetValue(value, Me)

                ' 只读开关应该在设置了初始值之后
                ' 再进行设置，否则会无法设置初始值的
                .[readonly] = [readonly]

                If Not .constraintValid Then
                    Return Internal.debug.stop(New Exception(String.Format(ConstraintInvalid, .typeCode, mode)), Me)
                Else
                    Call .DoCall(AddressOf symbols.Add)
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
                Return GetEnvironmentStackTraceString
            End If
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Symbol) Implements IEnumerable(Of Symbol).GetEnumerator
            For Each var As Symbol In symbols.Values
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

                        Log4VB.redirectError = AddressOf parent.redirectError
                        Log4VB.redirectWarning = AddressOf parent.redirectWarning
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
