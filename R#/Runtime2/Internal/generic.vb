#Region "Microsoft.VisualBasic::0535ab7b4cbab0e59e438285f41770c9, R#\Runtime\Internal\generic.vb"

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

        Friend Function invokeGeneric(funcName$, x As Object, args As list, env As Environment) As Object
            Dim type As Type = x.GetType
            Dim apiCalls As GenericFunction = generics(funcName)(type)
            Dim result As Object = apiCalls(x, args, env)

            Return result
        End Function
    End Module
End Namespace
