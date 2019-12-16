#Region "Microsoft.VisualBasic::0c67910d9f9b14928e6590feebbd1a29, R#\Runtime\Extensions.vb"

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
    '         Function: [GetType], ClosureStackName, (+2 Overloads) GetRTypeCode, IsPrimitive, MeasureArrayElementType
    '                   TryCatch
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime

    <HideModuleName> Public Module Extensions

        Public Function TryCatch(runScript As Func(Of Object)) As Object
            Try
                Return runScript()
            Catch ex As Exception
                Return ex
            End Try
        End Function

        Public Function MeasureArrayElementType(array As Array) As Type
            Dim x As Object

            For i As Integer = 0 To array.Length - 1
                x = array.GetValue(i)

                If Not x Is Nothing Then
                    Return x.GetType
                End If
            Next

            Return GetType(Void)
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
