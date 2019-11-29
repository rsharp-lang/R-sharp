#Region "Microsoft.VisualBasic::3f6c5d0cf78b32631e840d877706a461, R#\Runtime\Extensions.vb"

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

'     Module Extensions
' 
'         Function: [GetType], (+2 Overloads) asVector, ClosureStackName, getFirst, (+2 Overloads) GetRTypeCode
'                   IsPrimitive
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime

    <HideModuleName> Public Module Extensions

        ''' <summary>
        ''' Get first element in the input <paramref name="value"/> sequence
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Friend Function getFirst(value As Object) As Object
            Dim valueType As Type

            If value Is Nothing Then
                Return Nothing
            Else
                valueType = value.GetType
            End If

            If valueType.IsInheritsFrom(GetType(Array)) Then
                With DirectCast(value, Array)
                    If .Length = 0 Then
                        Return Nothing
                    Else
                        Return .GetValue(Scan0)
                    End If
                End With
            Else
                Return value
            End If
        End Function

        ''' <summary>
        ''' Ensure that the input <paramref name="value"/> object is a sequence. 
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function asVector(value As Object, type As Type) As Array
            Dim arrayType As Type = type.MakeArrayType
            Dim valueType As Type

            If value Is Nothing Then
                Return Nothing
            Else
                valueType = value.GetType
            End If

            If Not valueType Is arrayType Then
                If valueType.IsArray Then
                    Dim array As Array = Array.CreateInstance(type, DirectCast(value, Array).Length)
                    Dim src As Array = value

                    For i As Integer = 0 To array.Length - 1
                        array.SetValue(RConversion.CTypeDynamic(src.GetValue(i), type), i)
                    Next

                    Return array
                Else
                    Dim array As Array = Array.CreateInstance(type, 1)
                    array.SetValue(RConversion.CTypeDynamic(value, type), Scan0)
                    Return array
                End If
            Else
                Return value
            End If
        End Function

        ''' <summary>
        ''' 这个函数会确保返回的输出值都是一个数组
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function asVector(Of T)(value As Object) As Array
            Dim valueType As Type = value.GetType
            Dim typeofT As Type = GetType(T)

            If valueType Is typeofT Then
                Return {DirectCast(value, T)}
            ElseIf valueType.IsInheritsFrom(GetType(Array)) Then
                If DirectCast(value, Array) _
                    .AsObjectEnumerator _
                    .All(Function(i)
                             If Not i.GetType.IsInheritsFrom(GetType(Array)) Then
                                 Return True
                             Else
                                 Return DirectCast(i, Array).Length = 1
                             End If
                         End Function) Then

                    value = DirectCast(value, Array) _
                        .AsObjectEnumerator _
                        .Select(Function(o)
                                    If Not o.GetType Is typeofT Then
                                        If o.GetType.IsInheritsFrom(GetType(Array)) Then
                                            o = DirectCast(o, Array).GetValue(Scan0)
                                        End If
                                    End If
                                    If Not o.GetType Is typeofT Then
                                        ' 进行一些类型转换

                                        ' if apply the RConversion.CTypeDynamic
                                        ' then it may decouple object from vbObject container
                                        o = Conversion.CTypeDynamic(o, typeofT)
                                    End If

                                    Return DirectCast(o, T)
                                End Function) _
                        .ToArray
                End If

                Return value
            ElseIf valueType Is GetType(T()) Then
                Return value
            ElseIf valueType.IsInheritsFrom(GetType(IEnumerable(Of T))) Then
                Return DirectCast(value, IEnumerable(Of T)).ToArray
            Else
                Return {value}
            End If
        End Function

        ''' <summary>
        ''' Converts the input string text to value <see cref="TypeCodes"/>
        ''' </summary>
        ReadOnly parseTypecode As Dictionary(Of String, TypeCodes) = Enums(Of TypeCodes) _
            .ToDictionary(Function(e) e.Description.ToLower,
                          Function(code)
                              Return code
                          End Function)

        ''' <summary>
        ''' Get R type code from the type constraint expression value.
        ''' </summary>
        ''' <param name="type$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetRTypeCode(type As String) As TypeCodes
            If type.StringEmpty Then
                Return TypeCodes.generic
            ElseIf parseTypecode.ContainsKey(type) Then
                Return parseTypecode(type)
            Else
                ' .NET type
                Return TypeCodes.ref
            End If
        End Function

        ''' <summary>
        ''' It is R# primitive type?
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsPrimitive(type As TypeCodes) As Boolean
            Return type = TypeCodes.boolean OrElse
                   type = TypeCodes.double OrElse
                   type = TypeCodes.integer OrElse
                   type = TypeCodes.list OrElse
                   type = TypeCodes.string
        End Function

        ''' <summary>
        ''' VB.NET type to R type code mapping
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetRTypeCode(type As Type) As TypeCodes
            Select Case type
                Case GetType(String), GetType(String())
                    Return TypeCodes.string
                Case GetType(Integer), GetType(Integer()), GetType(Long()), GetType(Long)
                    Return TypeCodes.integer
                Case GetType(Double), GetType(Double())
                    Return TypeCodes.double
                Case GetType(Char), GetType(Char())
                    Return TypeCodes.string
                Case GetType(Boolean), GetType(Boolean())
                    Return TypeCodes.boolean
                Case GetType(Dictionary(Of String, Object)), GetType(Dictionary(Of String, Object)())
                    Return TypeCodes.list
                Case GetType(RMethodInfo), GetType(DeclareNewFunction) ', GetType(envir)
                    Return TypeCodes.closure
                Case Else
                    Return TypeCodes.generic
            End Select
        End Function

        ''' <summary>
        ''' Mapping R# <see cref="TypeCodes"/> to VB.NET type
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function [GetType](type As TypeCodes) As Type
            Select Case type
                Case TypeCodes.boolean : Return GetType(Boolean())
                Case TypeCodes.double : Return GetType(Double())
                Case TypeCodes.integer : Return GetType(Long())
                Case TypeCodes.list : Return GetType(Dictionary(Of String, Object))
                Case TypeCodes.string : Return GetType(String())
                Case TypeCodes.closure : Return GetType([Delegate])
                Case Else
                    Throw New InvalidCastException(type.Description)
            End Select
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="func$"></param>
        ''' <param name="script$"></param>
        ''' <param name="line%"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' How to keeps the script path reference?
        ''' </remarks>
        Public Function ClosureStackName(func$, script$, line%) As String
            Return $"<{script.FileName}#{line}::{func}()>"
        End Function
    End Module
End Namespace
