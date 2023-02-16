#Region "Microsoft.VisualBasic::2e8f2a6d6f46602c1c7b83a8f7d0901b, E:/GCModeller/src/R-sharp/R#//Runtime/Internal/generic.vb"

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

    '   Total Lines: 150
    '    Code Lines: 101
    ' Comment Lines: 26
    '   Blank Lines: 23
    '     File Size: 6.07 KB


    '     Delegate Function
    ' 
    ' 
    '     Module generic
    ' 
    '         Function: exists, (+3 Overloads) invokeGeneric, missingGenericSymbol, parseGeneric
    ' 
    '         Sub: add
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Internal

    ''' <summary>
    ''' 类型通用函数重载申明
    ''' fun(x, ...)
    ''' </summary>
    ''' <param name="x">脚本引擎会根据这个参数的类型进行通用函数的调用</param>
    ''' <param name="args"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 可以将这个机制看作为函数重载
    ''' </remarks>
    Public Delegate Function GenericFunction(x As Object, args As list, env As Environment) As Object

    ''' <summary>
    ''' Typped generic function invoke
    ''' </summary>
    Public Module generic

        ReadOnly generics As New Dictionary(Of String, Dictionary(Of Type, GenericFunction))

        ''' <summary>
        ''' overloads <paramref name="name"/> = (x As <see cref="Object"/>, args As <see cref="list"/>, env As <see cref="Environment"/>) As <see cref="Object"/>
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="x"></param>
        ''' <param name="[overloads]"></param>
        Public Sub add(name$, x As Type, [overloads] As GenericFunction)
            If Not generics.ContainsKey(name) Then
                generics(name) = New Dictionary(Of Type, GenericFunction)
            End If

            generics(name)(x) = [overloads]
        End Sub

        Public Function exists(funcName As String) As Boolean
            Return generics.ContainsKey(funcName)
        End Function

        ''' <summary>
        ''' ``genericName.&lt;type>``
        ''' </summary>
        ''' <param name="funcName"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Function parseGeneric(funcName As String, env As Environment) As NamedValue(Of Type)
            For Each type As String In env.globalEnvironment.types.Keys
                If funcName.EndsWith("." & type) Then
                    Dim genericName As String = funcName.Replace("." & type, "")
                    Dim target As Type = env.globalEnvironment.types(type)

                    If exists(genericName) Then
                        Return New NamedValue(Of Type) With {
                            .Name = genericName,
                            .Value = target
                        }
                    End If
                End If
            Next

            Return Nothing
        End Function

        Public Function invokeGeneric(generic As NamedValue(Of Type), args As list, x As Object, env As Environment) As Object
            If Not exists(generic.Name) Then
                Return missingGenericSymbol(generic.Name, env)
            Else
                Return invokeGeneric(args, x, env, generic.Name, generic)
            End If
        End Function

        Private Function missingGenericSymbol(funcName As String, env As Environment) As Message
            Return debug.stop({$"missing loader entry for generic function '{funcName}'!", "consider load required package at first!"}, env)
        End Function

        <Extension>
        Friend Function invokeGeneric(args As list, x As Object, env As Environment, <CallerMemberName> Optional funcName$ = Nothing) As Object
            Dim type As Type

            If x Is Nothing Then
                Return Internal.debug.stop("object 'x' is nothing!", env)
            Else
                type = x.GetType
            End If

            Dim acceptorArgs As Dictionary(Of String, Object) = env.GetAcceptorArguments

            For Each key As String In acceptorArgs.Keys
                ' 20211008 do not overrides the original parameters
                If Not args.hasName(key) Then
                    Call args.add(key, acceptorArgs(key))
                End If
            Next

            If Not generics.ContainsKey(funcName) Then
                Return missingGenericSymbol(funcName, env)
            Else
                Return invokeGeneric(args, x, env, funcName, type)
            End If
        End Function

        <Extension>
        Friend Function invokeGeneric(args As list, x As Object, env As Environment, funcName$, type As Type) As Object
            Dim apiCalls As GenericFunction

            If type Is GetType(vbObject) AndAlso Not generics(funcName).ContainsKey(type) Then
                x = DirectCast(x, vbObject).target
                type = x.GetType
            End If
            If type Is GetType(vector) AndAlso Not generics(funcName).ContainsKey(type) Then
                x = DirectCast(x, vector).data
                type = x.GetType
            End If

            If Not generics(funcName).ContainsKey(type) Then
                If TypeOf x Is Object() Then
                    x = MeasureRealElementType(DirectCast(x, Array)) _
                        .DoCall(Function(constrain)
                                    Return New vector(constrain, DirectCast(x, Array), env)
                                End Function) _
                        .data
                    type = x.GetType

                    If generics(funcName).ContainsKey(type) Then
                        apiCalls = generics(funcName)(type)
                        GoTo RUN_GENERIC
                    End If
                End If

                Return debug.stop({
                    $"missing loader entry for generic function '{funcName}'!",
                    $"missing implementation for overloads type: {type.FullName}!",
                    $"consider load required package at first!"
                }, env)
            Else
                apiCalls = generics(funcName)(type)
            End If
RUN_GENERIC:
            Dim result As Object = apiCalls(x, args, env)

            Return result
        End Function
    End Module
End Namespace
