#Region "Microsoft.VisualBasic::0deed29022d285ba1b62c1b4f0f81e7a, ..\R-sharp\R#\runtime\RType.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
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

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Scripting.TokenIcer.OperatorExpression

Namespace Runtime

    ''' <summary>
    ''' Type proxy for <see cref="TypeCodes.list"/> or system primitives
    ''' </summary>
    Public Class RType : Implements IReadOnlyId

        Public ReadOnly Property TypeCode As TypeCodes = TypeCodes.list
        Public ReadOnly Property FullName As String

        ''' <summary>
        ''' Using this property as the indentify key in the R# runtime <see cref="Environment"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Identity As String Implements IReadOnlyId.Identity
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.ToString.MD5
            End Get
        End Property

        ''' <summary>
        ''' The collection of unary operators for current R# type
        ''' </summary>
        Dim UnaryOperators As Dictionary(Of String, MethodInfo)
        Dim BinaryOperator1 As Dictionary(Of String, MethodInfo)
        Dim BinaryOperator2 As Dictionary(Of String, MethodInfo)

        Sub New(code As TypeCodes, name$)
            TypeCode = code
            FullName = name
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"[{TypeCode}] {FullName}"
        End Function

        ''' <summary>
        ''' ``operator me``
        ''' </summary>
        ''' <param name="operator$"></param>
        ''' <returns></returns>
        Public Function GetUnaryOperator(operator$) As Func(Of Object, Object)

        End Function

        ''' <summary>
        ''' ``other operator me``
        ''' </summary>
        ''' <param name="operator$"></param>
        ''' <param name="a"></param>
        ''' <returns></returns>
        Public Function GetBinaryOperator1(operator$, a As Type) As Func(Of Object, Object, Object)

        End Function

        ''' <summary>
        ''' ``me operator other``
        ''' </summary>
        ''' <param name="operator$"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Function GetBinaryOperator2(operator$, b As Type) As Func(Of Object, Object, Object)

        End Function

        ''' <summary>
        ''' Imports the .NET type as R# type
        ''' </summary>
        ''' <param name="dotnet"></param>
        ''' <returns></returns>
        Public Shared Function [Imports](dotnet As Type) As RType
            Dim operators = dotnet _
                .GetMethods(PublicShared) _
                .Where(Function(m) m.Name.StartsWith("op_")) _
                .ToArray
            Dim unarys = operators _
                .Where(Function(m) m.GetParameters.Length = 1) _
                .ToDictionary(Function(key)
                                  Dim linqName = opName2Linq(key.Name)
                                  Dim op$ = Linq2Symbols(linqName)
                                  Return op
                              End Function)
            Dim binarys = operators.Where(Function(m) m.GetParameters.Length = 2).GroupBy(Function(m) m.Name)
            Dim code As TypeCodes = dotnet.GetRTypeCode

            Return New RType(code, dotnet.FullName) With {
                .UnaryOperators = unarys
            }
        End Function
    End Class
End Namespace
