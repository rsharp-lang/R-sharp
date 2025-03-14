﻿#Region "Microsoft.VisualBasic::bdc223e8acdbcc176c97c4fc6e76b4f2, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\DotNetObject.vb"

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
    '    Code Lines: 114 (76.00%)
    ' Comment Lines: 13 (8.67%)
    '    - Xml Docs: 53.85%
    ' 
    '   Blank Lines: 23 (15.33%)
    '     File Size: 6.13 KB


    '     Class DotNetObject
    ' 
    '         Properties: [object], expressionName, member, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, getReader, getStrictOpt, readSingle, readVectorShadows
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    ''' <summary>
    ''' [x]::property, this syntax only works for the class property
    ''' </summary>
    Public Class DotNetObject : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.generic
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.DotNetMemberReference
            End Get
        End Property

        Public ReadOnly Property [object] As Expression

        ''' <summary>
        ''' the vector shadow reader
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property member As Expression

        Sub New(obj As Expression, member As Expression)
            Me.object = obj
            Me.member = member
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim objVal As Object = [object].Evaluate(envir)
            Dim memberName As String = ValueAssignExpression.GetSymbol(member)

            If Program.isException(objVal) Then
                Return objVal
            ElseIf objVal Is Nothing Then
                Return Nothing
            ElseIf TypeOf objVal Is vbObject Then
                objVal = DirectCast(objVal, vbObject).target
            End If
            If TypeOf objVal Is vector Then
                objVal = DirectCast(objVal, vector).data
            End If
            If objVal.GetType.IsArray AndAlso DirectCast(objVal, Array).Length = 1 Then
                objVal = DirectCast(objVal, Array).GetValue(Scan0)
            End If

            If TypeOf objVal Is list Then
                Return DirectCast(objVal, list).getByName(memberName)
            ElseIf objVal.GetType.IsArray Then
                Return readVectorShadows(TryCastGenericArray(objVal, envir), memberName, envir)
            Else
                Return readSingle(objVal, memberName, envir)
            End If
        End Function

        Private Shared Function getReader(type As Type, memberName As String) As PropertyInfo
            Static schema As New Dictionary(Of Type, Dictionary(Of String, PropertyInfo))

            Dim schemaTable = schema.ComputeIfAbsent(type, Function(cache) DataFramework.Schema(cache, PropertyAccess.Readable, nonIndex:=True))
            Dim reader As PropertyInfo = schemaTable.TryGetValue(memberName)

            Return reader
        End Function

        Private Function readVectorShadows(array As Array, memberName As String, envir As Environment) As Object
            Dim type As Type = array.GetType
            Dim reader As PropertyInfo = getReader(type.GetElementType, memberName)

            If reader Is Nothing Then
                ' 20250214 deal with some special situation
                '
                ' empty vector? -> returns nothing
                If array.IsNullOrEmpty Then
                    Return Nothing
                End If
                ' all elements inside vector is nothing? -> returns a vector with all elements is nothing?
                If array.AllNothing Then
                    If getStrictOpt(envir) Then
                        Return Internal.debug.stop("all of the elements inside the given vector is nothing, can not extract symbol value when in strict mode!", envir)
                    Else
                        Return array
                    End If
                End If

                If getStrictOpt(envir) Then
                    Return Internal.debug.stop($"can not found member symbol '{memberName}' in vector element [{type.GetElementType.FullName}].", envir)
                Else
                    Return Nothing
                End If
            Else
                array = array _
                    .AsObjectEnumerator _
                    .Select(Function(a)
                                ' read clr object instance data
                                ' required of non-null object a
                                If a Is Nothing Then
                                    Return Nothing
                                Else
                                    Return reader.GetValue(a, Nothing)
                                End If
                            End Function) _
                    .ToArray

                Return TryCastGenericArray(array, env:=envir)
            End If
        End Function

        Private Function getStrictOpt(envir As Environment) As Boolean
            Dim globals = envir.globalEnvironment
            Dim strict As Boolean = globals.options.strict

            strict = globals.options _
                .getOption("vector_shadow_strict", strict.ToString) _
                .ParseBoolean

            Return strict
        End Function

        Private Function readSingle(objVal As Object, memberName As String, envir As Environment) As Object
            Dim reader As PropertyInfo = getReader(objVal.GetType, memberName)

            If reader Is Nothing Then
                If getStrictOpt(envir) Then
                    Return Internal.debug.stop($"can not found member symbol '{memberName}' in scalar object [{objVal.GetType.FullName}].", envir)
                Else
                    Return Nothing
                End If
            Else
                Return reader.GetValue(objVal, Nothing)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"[{[object]}]::{member}"
        End Function
    End Class
End Namespace
