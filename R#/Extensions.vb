#Region "Microsoft.VisualBasic::569e71dbd7a54933f9cf74059576487b, ..\R-sharp\R#\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime

''' <summary>
''' Internal runtime extensions
''' </summary>
Module Extensions

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Argv(type As Type, name$, i%) As RParameterInfo
        Return New RParameterInfo(name, type, pos:=i)
    End Function

    Public Function EnsurePrimitiveVector(obj) As Object

    End Function

    ''' <summary>
    ''' Get R type code from the type constraint expression value.
    ''' </summary>
    ''' <param name="type$"></param>
    ''' <returns></returns>
    <Extension> Public Function GetRTypeCode(type$) As TypeCodes
        If type.StringEmpty Then
            Return TypeCodes.generic
        End If

        Return [Enum].Parse(GetType(TypeCodes), type.ToLower)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function DotNETtype(code As TypeCodes) As Type
        Select Case code
            Case TypeCodes.boolean
                Return GetType(Boolean)
            Case TypeCodes.char
                Return GetType(Char)
            Case TypeCodes.double
                Return GetType(Double)
            Case TypeCodes.integer
                Return GetType(Integer)
            Case TypeCodes.list
                Return GetType(Dictionary(Of String, Object))
            Case TypeCodes.string
                Return GetType(String)
            Case TypeCodes.uinteger
                Return GetType(ULong)
            Case Else
                Throw New InvalidExpressionException("Target is not a primitive type!")
        End Select
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function IsPrimitive(type As TypeCodes) As Boolean
        Return type = TypeCodes.boolean OrElse
            type = TypeCodes.char OrElse
            type = TypeCodes.double OrElse
            type = TypeCodes.integer OrElse
            type = TypeCodes.list OrElse
            type = TypeCodes.string OrElse
            type = TypeCodes.uinteger
    End Function

    ''' <summary>
    ''' DotNET type to R type code
    ''' </summary>
    ''' <param name="type"></param>
    ''' <returns></returns>
    <Extension> Public Function GetRTypeCode(type As Type) As TypeCodes
        Select Case type
            Case GetType(String), GetType(String())
                Return TypeCodes.string
            Case GetType(Integer), GetType(Integer())
                Return TypeCodes.integer
            Case GetType(Double), GetType(Double())
                Return TypeCodes.double
            Case GetType(ULong), GetType(ULong())
                Return TypeCodes.uinteger
            Case GetType(Char), GetType(Char())
                Return TypeCodes.char
            Case GetType(Boolean), GetType(Boolean())
                Return TypeCodes.boolean
            Case GetType(Dictionary(Of String, Object)), GetType(Dictionary(Of String, Object)())
                Return TypeCodes.list
            Case Else
                Return TypeCodes.generic
        End Select
    End Function

    Public Function ClosureStackName(func$, script$, line%) As String
        Return $"<{script.FileName}#{line}::{func}()>"
    End Function
End Module
