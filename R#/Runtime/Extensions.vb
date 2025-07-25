﻿#Region "Microsoft.VisualBasic::1cc642ee62dcbf7aa49745a2642ea421, R#\Runtime\Extensions.vb"

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

    '   Total Lines: 248
    '    Code Lines: 158 (63.71%)
    ' Comment Lines: 56 (22.58%)
    '    - Xml Docs: 78.57%
    ' 
    '   Blank Lines: 34 (13.71%)
    '     File Size: 9.18 KB


    '     Module Extensions
    ' 
    '         Function: debugOption, isCallable, MeasureArrayElementType, MeasureRealElementType, MeasureVectorType
    '                   MeasureVectorTypes, Push, strictOption, TryCatch, verboseOption
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports vbObject = SMRUCC.Rsharp.Runtime.Internal.Object.vbObject

Namespace Runtime

    <HideModuleName> Public Module Extensions

        ''' <summary>
        ''' If exception happens, then this function will catch 
        ''' the exceptin object and then returns the error.
        ''' </summary>
        ''' <param name="runScript"></param>
        ''' <returns></returns>
        Public Function TryCatch(runScript As Func(Of Object), debug As Boolean) As Object
            If debug Then
                Return runScript()
            Else
                Try
                    Return runScript()
                Catch ex As Exception
                    Return ex
                End Try
            End If
        End Function

        ''' <summary>
        ''' 这个函数只会尝试第一个不为空的元素的类型
        ''' </summary>
        ''' <param name="array"></param>
        ''' <returns></returns>
        Public Function MeasureArrayElementType(array As Array) As Type
            Dim x As Object
            Dim arrayType As Type = array.GetType

            If arrayType.HasElementType AndAlso Not arrayType.GetElementType Is GetType(Object) Then
                Return arrayType.GetElementType
            End If

            For i As Integer = 0 To array.Length - 1
                x = array.GetValue(i)

                If x IsNot Nothing AndAlso
                    x IsNot invalidObject.value AndAlso
                    x IsNot GetType(Void) Then

                    Return x.GetType
                End If
            Next

            Return GetType(Void)
        End Function

        Public Function MeasureVectorTypes(array As Array, Optional unique As Boolean = True, Optional excludeInterfaceOrAbstract As Boolean = False) As IEnumerable(Of Type)
            Dim arrayType As Type = array.GetType
            Dim x As Object
            Dim types As New List(Of Type)

            If array.Length = 0 Then
                Return New Type() {}
            End If

            Dim elType As Type = arrayType.GetElementType

            If arrayType.HasElementType AndAlso Not elType Is GetType(Object) Then
                If excludeInterfaceOrAbstract Then
                    If Not (elType.IsAbstract OrElse elType.IsInterface) Then
                        Return New Type() {elType}
                    End If
                Else
                    Return New Type() {elType}
                End If
            End If

            For i As Integer = 0 To array.Length - 1
                x = array.GetValue(i)

                If x IsNot Nothing AndAlso
                    x IsNot invalidObject.value AndAlso
                    x IsNot GetType(Void) Then

                    arrayType = x.GetType

                    If arrayType Is GetType(vbObject) Then
                        arrayType = DirectCast(x, vbObject).target.GetType
                    End If

                    types.Add(arrayType)
                End If
            Next

            If unique Then
                Return types.Distinct
            Else
                Return types
            End If
        End Function

        Public Function MeasureVectorType(types As Type()) As Type
            If types.Length = 0 Then
                Return GetType(Object)
            End If
            If types.Length = 1 OrElse types.Distinct.Count = 1 Then
                Return types(Scan0)
            End If

            Dim tg = types _
                .GroupBy(Function(t) t.FullName) _
                .OrderByDescending(Function(k) k.Count) _
                .ToArray

            ' 都是相同类型
            If tg.Length = 1 Then
                Return tg(Scan0).First
            End If

            ' 按照类型缩放原则进行类型的选取
            Dim allTypes As Type() = tg.Select(Function(g) g.First).ToArray

            ' 排序之后，一般sub type会排在最开始
            ' base type会排在最后
            allTypes = allTypes _
                .Sort(Function(a, b)
                          If a.IsInheritsFrom(b) Then
                              Return -1
                          Else
                              ' 按照full name排序
                              Return a.FullName.CompareTo(b.FullName)
                          End If
                      End Function) _
                .ToArray

            ' 如果最开始的类型可以继承自最末尾的类型
            ' 则返回最末尾的类型
            If allTypes(Scan0).IsInheritsFrom(allTypes.Last) Then
                Return allTypes.Last
            ElseIf allTypes.Any(Function(t) t Is GetType(Double)) AndAlso
                allTypes.All(Function(t) DataFramework.IsNumericType(t)) Then

                Return GetType(Double)
            ElseIf allTypes.All(Function(t) DataFramework.IsIntegerType(t)) Then
                Return GetType(Long)
            Else
                ' 反之说明类型间没有继承关系，即互不兼容，则返回object类型
                Return GetType(Object)
            End If
        End Function

        ''' <summary>
        ''' 这个会遵循类型缩放的原则返回最大的类型
        ''' </summary>
        ''' <param name="array"></param>
        ''' <returns>
        ''' if all of the element in the given <paramref name="array"/> is nothing
        ''' then function returns the <paramref name="defaultType"/> value
        ''' </returns>
        Public Function MeasureRealElementType(array As Array, Optional defaultType As Type = Nothing) As Type
            Dim types As Type() = MeasureVectorTypes(array, unique:=False).ToArray

            If types.Length = 0 Then
                Return If(defaultType, GetType(Void))
            Else
                Return MeasureVectorType(types)
            End If
        End Function

        ''' <summary>
        ''' test target object is a <see cref="RFunction"/> or not
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function isCallable(x As Object) As Boolean
            If x Is Nothing Then
                Return False
            ElseIf x.GetType.ImplementInterface(Of RFunction) Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' push runtime function parameters
        ''' </summary>
        ''' <param name="env"></param>
        ''' <param name="parameters"></param>
        ''' <returns></returns>
        <Extension>
        Friend Function Push(env As Environment, parameters As IEnumerable(Of DeclareNewSymbol)) As Environment
            For Each symbol As DeclareNewSymbol In parameters
                For Each name As String In symbol.names
                    Call env.Push(name, Nothing, [readonly]:=False)
                Next
            Next

            Return env
        End Function

        ''' <summary>
        ''' get strict options value
        ''' </summary>
        ''' <param name="env">The runtime context</param>
        ''' <param name="opt"></param>
        ''' <returns></returns>
        <Extension>
        Public Function strictOption(env As Environment, Optional opt As Boolean? = Nothing) As Boolean
            If opt Is Nothing Then
                Return env.globalEnvironment.options.strict
            Else
                Return opt
            End If
        End Function

        ''' <summary>
        ''' get verbose options value
        ''' </summary>
        ''' <param name="env">
        ''' The runtime context
        ''' </param>
        ''' <param name="opt"></param>
        ''' <returns></returns>
        <Extension>
        Public Function verboseOption(env As Environment, Optional opt As Boolean? = Nothing) As Boolean
            ' is the parameter value is nothing, then
            ' use the config from the runtime configuration
            If opt Is Nothing Then
                Return env.globalEnvironment.options.verbose
            Else
                ' else the user input parameter value always 
                ' overrides the runtime configuration
                Return opt
            End If
        End Function

        <Extension>
        Public Function debugOption(env As Environment, Optional opt As Boolean? = Nothing) As Boolean
            If opt Is Nothing Then
                Return env.globalEnvironment.options.getOption("debug", Boolean.FalseString).ParseBoolean
            Else
                Return opt
            End If
        End Function
    End Module
End Namespace
