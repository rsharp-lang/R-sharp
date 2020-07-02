#Region "Microsoft.VisualBasic::30276d0c0252caea06abd7ae41f3fb8d, R#\Runtime\Interop\RsharpApi\RMethodInfo.vb"

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

'     Class RMethodInfo
' 
'         Properties: invisible, name, parameters, returns
' 
'         Constructor: (+3 Overloads) Sub New
'         Function: createNormalArguments, CreateParameterArrayFromListArgument, GetPackageInfo, GetPrintContent, GetRawDeclares
'                   getValue, (+2 Overloads) Invoke, missingParameter, parseParameters, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.System.Package

Namespace Runtime.Interop

    ''' <summary>
    ''' Use for R# package method
    ''' </summary>
    Public Class RMethodInfo : Implements RFunction, RPrint

        ''' <summary>
        ''' The function name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property name As String Implements RFunction.name
        Public ReadOnly Property returns As RType
        Public ReadOnly Property parameters As RMethodArgument()

        ''' <summary>
        ''' Do not print the value of this function on console
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property invisible As Boolean

        ReadOnly api As MethodInvoke

        ''' <summary>
        ''' 
        ''' </summary>
        Friend ReadOnly listObjectMargin As ListObjectArgumentMargin = ListObjectArgumentMargin.none

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="closure">
        ''' Runtime generated .NET method
        ''' </param>
        Sub New(name$, closure As [Delegate])
            Me.name = name
            Me.api = New MethodInvoke With {
                .method = closure.Method,
                .target = closure.Target
            }
            Me.returns = RType.GetRSharpType(closure.Method.ReturnType)
            Me.parameters = closure.Method.DoCall(AddressOf parseParameters)
            Me.listObjectMargin = RArgumentList.objectListArgumentMargin(Me)
        End Sub

        ''' <summary>
        ''' Static method
        ''' </summary>
        ''' <param name="api"></param>
        Sub New(api As NamedValue(Of MethodInfo))
            Call Me.New(api.Name, api.Value, Nothing)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="closure"><see cref="MethodInfo"/> from parsing .NET dll module file.</param>
        ''' <param name="target"></param>
        Sub New(name$, closure As MethodInfo, target As Object)
            Me.name = name
            Me.api = New MethodInvoke With {.method = closure, .target = target}
            Me.returns = RType.GetRSharpType(closure.ReturnType)
            Me.parameters = closure.DoCall(AddressOf parseParameters)
            Me.invisible = RSuppressPrintAttribute.IsPrintInvisible(closure)
            Me.listObjectMargin = RArgumentList.objectListArgumentMargin(Me)
        End Sub

        ''' <summary>
        ''' Gets the <see cref="MethodInfo"/> represented by the delegate.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetRawDeclares() As MethodInfo
            Return api.method
        End Function

        Public Function GetPackageInfo() As Package
            Return GetRawDeclares.DeclaringType.ParsePackage(strict:=False)
        End Function

        Public Function GetPrintContent() As String Implements RPrint.GetPrintContent
            Return markdown
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function parseParameters(method As MethodInfo) As RMethodArgument()
            Return method _
                .GetParameters _
                .Select(AddressOf RMethodArgument.ParseArgument) _
                .ToArray
        End Function

        Public Function Invoke(parameters As Object(), env As Environment) As Object
            Dim result As Object

            For Each arg In parameters
                If Not arg Is Nothing AndAlso arg.GetType Is GetType(Message) Then
                    Return arg
                End If
            Next

            If env.globalEnvironment.Rscript.debug Then
                result = api.Invoke(parameters)
            Else
                Try
                    result = api.Invoke(parameters)
                Catch ex As Exception
                    Return Internal.debug.stop(ex, env)
                End Try
            End If

            If invisible Then
                Return New invisible With {
                    .value = result
                }
            Else
                Return result
            End If
        End Function

        Public Function CreateParameterArrayFromListArgument(envir As Environment, list As Dictionary(Of String, Object)) As Object()
            Return createNormalArguments(envir, arguments:=list).ToArray
        End Function

        Public Function Invoke(envir As Environment, params As InvokeParameter()) As Object Implements RFunction.Invoke
            Dim parameters As Object()
            Dim apiStackFrame As New StackFrame With {
                .File = GetRawDeclares.DeclaringType.Assembly.Location.FileName,
                .Line = "<unknown>",
                .Method = New Method With {
                    .Method = name,
                    .[Module] = "R#_interop::",
                    .[Namespace] = GetPackageInfo.namespace
                }
            }

            Using env As New Environment(envir, apiStackFrame, isInherits:=True)
                If listObjectMargin <> ListObjectArgumentMargin.none Then
                    parameters = RArgumentList.CreateObjectListArguments(Me, env, params).ToArray
                Else
                    parameters = InvokeParameter _
                        .CreateArguments(env, params, hasObjectList:=False) _
                        .DoCall(Function(args)
                                    Return createNormalArguments(env, args)
                                End Function) _
                        .ToArray
                End If

                Return Invoke(parameters, env)
            End Using
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="arguments">
        ''' required of replace dot(.) to underline(_)?
        ''' </param>
        ''' <returns></returns>
        Private Iterator Function createNormalArguments(envir As Environment, arguments As Dictionary(Of String, Object)) As IEnumerable(Of Object)
            Dim arg As RMethodArgument
            Dim keys As String() = arguments.Keys.ToArray
            Dim nameKey As String
            Dim apiTrace As String = name

            For Each value As Object In arguments.Values
                If Not value Is Nothing AndAlso value.GetType Is GetType(Message) Then
                    Yield value
                    Return
                End If
            Next

            For i As Integer = 0 To Me.parameters.Length - 1
                arg = Me.parameters(i)

                If arguments.ContainsKey(arg.name) Then
                    Yield getValue(arg, arguments(arg.name), apiTrace, envir, False)
                ElseIf i >= arguments.Count Then
                    ' default value
                    If arg.type.raw Is GetType(Environment) Then
                        Yield envir
                    ElseIf Not arg.isOptional Then
                        Yield missingParameter(arg, envir, name)
                    Else
                        Yield arg.default
                    End If
                Else
                    nameKey = $"${i}"

                    If arguments.ContainsKey(nameKey) Then
                        Yield getValue(arg, arguments(nameKey), apiTrace, envir, False)
                    Else
                        If arg.isOptional Then
                            Yield arg.default
                        Else
                            Yield missingParameter(arg, envir, name)
                        End If
                    End If
                End If
            Next
        End Function

        Friend Shared Function missingParameter(arg As RMethodArgument, envir As Environment, funcName$) As Object
            Dim messages$() = {
                $"Missing parameter value for '{arg.name}'!",
                $"parameter: {arg.name}",
                $"type: {arg.type.raw.FullName}",
                $"function: {funcName}",
                $"environment: {envir.ToString}"
            }

            Return Internal.debug.stop(messages, envir)
        End Function

        ''' <summary>
        ''' Get type converted object value for match the parameter type. 
        ''' </summary>
        ''' <param name="arg"></param>
        ''' <param name="value"></param>
        ''' <param name="trace"></param>
        ''' <param name="envir"></param>
        ''' <param name="trygetListParam">
        ''' Fix bugs for list arguments when the parameter input have no symbol name
        ''' In such situation, then type is mismatch due to the reason of invalid 
        ''' offset bugs
        ''' </param>
        ''' <returns></returns>
        Friend Shared Function getValue(arg As RMethodArgument, value As Object, trace$, ByRef envir As Environment, trygetListParam As Boolean) As Object
            If arg.type Like GetType(Environment) Then
                Return envir
            ElseIf Program.isException(value, envir) Then
                Return value
            ElseIf value Is Nothing Then
                Return Nothing
            ElseIf value.GetType Is arg.type.raw Then
                Return value
            End If

            If arg.type.isArray Then
                value = CObj(Runtime.asVector(value, arg.type.GetRawElementType, env:=envir))
            ElseIf arg.type.isCollection Then
                ' ienumerable
                value = value
            ElseIf Not arg.isRequireRawVector Then
                value = Runtime.getFirst(value)
            ElseIf arg.isRequireRawVector AndAlso Not arg.rawVectorFlag.vector Is Nothing Then
                Return Runtime.asVector(value, arg.rawVectorFlag.vector, envir)
            End If

            If arg.type Like GetType(Object) Then
                Return value
            End If

            Try
                Return RCType.CTypeDynamic(value, arg.type.raw, env:=envir)
            Catch ex As Exception When trygetListParam
                Return GetType(Void)
            Catch ex As Exception
                Return Internal.debug.stop(New InvalidCastException("api: " & trace, ex), envir)
            End Try
        End Function

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {api.ToString}"
        End Function

        Public Function getReturns(env As Environment) As RType Implements RFunction.getReturns
            Return returns
        End Function
    End Class
End Namespace
