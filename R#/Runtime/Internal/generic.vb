#Region "Microsoft.VisualBasic::8a02a1ef3d9574db047b833f530922a9, R#\Runtime\Internal\generic.vb"

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

    '     Delegate Function
    ' 
    ' 
    '     Module generic
    ' 
    '         Function: exists, invokeGeneric
    ' 
    '         Sub: add
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
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

        Public Sub add(name$, x As Type, [overloads] As GenericFunction)
            If Not generics.ContainsKey(name) Then
                generics(name) = New Dictionary(Of Type, GenericFunction)
            End If

            generics(name)(x) = [overloads]
        End Sub

        Public Function exists(funcName As String) As Boolean
            Return generics.ContainsKey(funcName)
        End Function

        <Extension>
        Friend Function invokeGeneric(args As list, x As Object, env As Environment, <CallerMemberName> Optional funcName$ = Nothing) As Object
            Dim type As Type = x.GetType
            Dim apiCalls As GenericFunction

            If Not generics.ContainsKey(funcName) Then
                Return debug.stop({$"missing loader entry for generic function '{funcName}'!", "consider load required package at first!"}, env)
            ElseIf Not generics(funcName).ContainsKey(type) Then
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
                    "consider load required package at first!"
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
