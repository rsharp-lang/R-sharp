#Region "Microsoft.VisualBasic::44f596a42146016717507ce2a3203c62, R#\Runtime\RTypeExtensions.vb"

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

    '   Total Lines: 177
    '    Code Lines: 115 (64.97%)
    ' Comment Lines: 49 (27.68%)
    '    - Xml Docs: 87.76%
    ' 
    '   Blank Lines: 13 (7.34%)
    '     File Size: 7.94 KB


    '     Module RTypeExtensions
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) [GetType], (+2 Overloads) GetRTypeCode, IsNumeric, (+2 Overloads) IsPrimitive
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime

    Public Module RTypeExtensions

        ''' <summary>
        ''' Converts the input string text to value <see cref="TypeCodes"/>
        ''' </summary>
        ReadOnly parseTypeCode As Dictionary(Of String, TypeCodes)

        Sub New()
            parseTypeCode = Enums(Of TypeCodes) _
                .ToDictionary(Function(e) e.Description.ToLower,
                              Function(code)
                                  Return code
                              End Function)

            ' add string type alias
            parseTypeCode("character") = TypeCodes.string
            parseTypeCode("logical") = TypeCodes.boolean
            parseTypeCode("numeric") = TypeCodes.double
            parseTypeCode("data.frame") = TypeCodes.dataframe
        End Sub

        ''' <summary>
        ''' Get R type code from the type constraint expression value.
        ''' </summary>
        ''' <param name="type$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetRTypeCode(type As String) As TypeCodes
            If type.StringEmpty Then
                Return TypeCodes.generic
            ElseIf parseTypeCode.ContainsKey(type) Then
                Return parseTypeCode(type)
            ElseIf type = "any" Then
                Return TypeCodes.generic
            Else
                ' .NET type
                Return TypeCodes.NA
            End If
        End Function

        ''' <summary>
        ''' It is R# primitive type?
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsPrimitive(type As Type, Optional includeComplexList As Boolean = True) As Boolean
            Return GetRTypeCode(type).IsPrimitive(includeComplexList)
        End Function

        ''' <summary>
        ''' It is R# primitive type? (bool, double, int, string, list?)
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsPrimitive(type As TypeCodes, Optional includeComplexList As Boolean = True) As Boolean
            Return type = TypeCodes.boolean OrElse
                   type = TypeCodes.double OrElse
                   type = TypeCodes.integer OrElse
                  (type = TypeCodes.list AndAlso includeComplexList) OrElse
                   type = TypeCodes.string
        End Function

        ''' <summary>
        ''' Target R type is a kind of numeric type?
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns>
        ''' this function returns true if the <paramref name="type"/> is 
        ''' <see cref="TypeCodes.double"/> or <see cref="TypeCodes.integer"/>
        ''' </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsNumeric(type As TypeCodes) As Boolean
            Return type = TypeCodes.double OrElse type = TypeCodes.integer
        End Function

        ''' <summary>
        ''' VB.NET CLR object type to R type code mapping
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetRTypeCode(type As Type) As TypeCodes
            Select Case type
                Case GetType(String), GetType(String())
                    Return TypeCodes.string
                Case GetType(Integer), GetType(Integer()), GetType(Long()), GetType(Long), GetType(Short), GetType(Short()),
                     GetType(UInteger), GetType(UInteger()), GetType(ULong), GetType(ULong())
                    Return TypeCodes.integer
                Case GetType(Double), GetType(Double()), GetType(Single), GetType(Single())
                    Return TypeCodes.double
                Case GetType(Char), GetType(Char())
                    Return TypeCodes.string
                Case GetType(Boolean), GetType(Boolean())
                    Return TypeCodes.boolean
                Case GetType(Dictionary(Of String, Object)), GetType(Dictionary(Of String, Object)()), GetType(list)
                    Return TypeCodes.list
                Case GetType(RMethodInfo), GetType(DeclareNewFunction), GetType(RFunction)
                    Return TypeCodes.closure
                Case Else
                    ' if strict, then the environment type comes from the
                    ' different build version of the R# runtime assembly
                    ' will not be equals
                    If type.IsInheritsFrom(GetType(Environment), strict:=False) Then
                        Return TypeCodes.environment
                    Else
                        Return TypeCodes.generic
                    End If
            End Select
        End Function

        ''' <summary>
        ''' Mapping CLR <see cref="TypeCode"/> to .net CLR object type
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function [GetType](type As TypeCode) As Type
            Select Case type
                Case TypeCode.Boolean : Return GetType(Boolean())
                Case TypeCode.Double, TypeCode.Single, TypeCode.Decimal
                    Return GetType(Double())
                Case TypeCode.Int32,
                     TypeCode.Byte,
                     TypeCode.Int16,
                     TypeCode.Int64,
                     TypeCode.SByte,
                     TypeCode.UInt16,
                     TypeCode.UInt32,
                     TypeCode.UInt64
                    Return GetType(Long())
                Case TypeCode.String, TypeCode.Char : Return GetType(String())
                Case TypeCode.Object : Return GetType(Object)
                Case Else
                    Throw New InvalidCastException(type.Description)
            End Select
        End Function

        ''' <summary>
        ''' Mapping R# <see cref="TypeCodes"/> to .net CLR object type
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns>
        ''' the function will returns nothing if the type mapping is not valid
        ''' </returns>
        Public Function [GetType](type As TypeCodes, Optional elementType As Boolean = False) As Type
            Select Case type
                Case TypeCodes.boolean : Return If(elementType, GetType(Boolean), GetType(Boolean()))
                Case TypeCodes.double : Return If(elementType, GetType(Double), GetType(Double()))
                Case TypeCodes.integer : Return If(elementType, GetType(Long), GetType(Long()))
                Case TypeCodes.list : Return GetType(Dictionary(Of String, Object))
                Case TypeCodes.string : Return If(elementType, GetType(String), GetType(String()))
                Case TypeCodes.closure, TypeCodes.clr_delegate : Return GetType([Delegate])
                Case TypeCodes.generic, TypeCodes.NA : Return GetType(Object)
                Case TypeCodes.dataframe : Return GetType(dataframe)
                Case TypeCodes.environment : Return GetType(Environment)

                Case Else
                    Call ("The clr type mapping is not supported for the R# type mode: " & type.Description).Warning
                    Return GetType(Object)
            End Select
        End Function
    End Module
End Namespace
