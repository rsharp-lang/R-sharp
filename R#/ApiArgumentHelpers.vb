#Region "Microsoft.VisualBasic::c92a2d50db4711228186760754cfc114, R#\ApiArgumentHelpers.vb"

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

' Module ApiArgumentHelpers
' 
'     Function: GetDoubleRange
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports REnv = SMRUCC.Rsharp.Runtime

Public Module ApiArgumentHelpers

    Public Function GetNamedValueTuple(Of T)(value As Object, env As Environment, <CallerMemberName> Optional api$ = Nothing) As [Variant](Of NamedValue(Of T), Message)
        If value Is Nothing Then
            Return Nothing
        End If

        Select Case value.GetType
            Case GetType(NamedValue(Of T))
                Return DirectCast(value, NamedValue(Of T))
            Case GetType(list)
                Dim list As list = DirectCast(value, list)
                Dim name As String = list.slots.Keys.First

                value = DirectCast(value, list).slots(name)
                value = RCType.CTypeDynamic(value, GetType(T), env)

                Return New NamedValue(Of T) With {
                    .Name = name,
                    .Value = value,
                    .Description = list.getValue(Of String)("description", env)
                }

            Case Else
                Return debug.stop({
                    "invalid data type for cast to a numeric range!",
                    "required: " & GetType(NamedValue(Of T)).FullName,
                    "given: " & value.GetType.FullName
                }, env)
        End Select
    End Function

    Public Function GetDoubleRange(value As Object, env As Environment,
                                   Optional default$ = "0,1",
                                   <CallerMemberName>
                                   Optional api$ = Nothing) As [Variant](Of DoubleRange, Message)
        If value Is Nothing Then
            Return CType([default], DoubleRange)
        End If

        Select Case value.GetType
            Case GetType(vector)
                Dim v As vector = value
                Dim vec As Double()

                Select Case v.elementType.mode
                    Case TypeCodes.double
                        vec = REnv.asVector(Of Double)(v.data)
                    Case TypeCodes.integer
                        vec = REnv.asVector(Of Double)(v.data)
                    Case TypeCodes.string

                        If v.length = 1 Then
                            Return CType(DirectCast(v.data.GetValue(Scan0), String), DoubleRange)
                        Else
                            vec = v.data _
                                .AsObjectEnumerator(Of String) _
                                .Select(AddressOf Val) _
                                .ToArray
                        End If

                    Case Else
                        Return debug.stop({
                            "invalid vector data type!",
                            "mode: " & v.elementType.mode,
                            "raw: " & v.elementType.fullName
                        }, env)
                End Select

                If vec.Length < 2 Then
                    Return debug.stop({
                        "a numeric range required two boundary value at least!",
                        "api: " & api
                    }, env)
                Else
                    Return New DoubleRange(vec)
                End If
            Case GetType(DoubleRange)
                Return DirectCast(value, DoubleRange)
            Case GetType(String)
                Return CType(DirectCast(value, String), DoubleRange)
            Case Else
                Return debug.stop({
                    "invalid data type for cast to a numeric range!",
                    "required: " & GetType(DoubleRange).FullName,
                    "given: " & value.GetType.FullName
                }, env)
        End Select
    End Function
End Module
