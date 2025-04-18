﻿#Region "Microsoft.VisualBasic::4691ab01abe2202dacd93adcaa2fed31, R#\Runtime\Internal\objects\dataset\factor.vb"

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

    '   Total Lines: 159
    '    Code Lines: 110 (69.18%)
    ' Comment Lines: 26 (16.35%)
    '    - Xml Docs: 96.15%
    ' 
    '   Blank Lines: 23 (14.47%)
    '     File Size: 5.57 KB


    '     Class factor
    ' 
    '         Properties: factorId, levels, nlevel
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) asCharacter, asFactor, asNumeric, checkSize, CreateFactor
    '                   GetFactor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.TypeCast
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports std = System.Math

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' string to integer dictionary
    ''' </summary>
    ''' <remarks>
    ''' 这个对象只是相当于unit的一个存在
    ''' </remarks>
    Public Class factor : Inherits RsharpDataObject

        ''' <summary>
        ''' order in character asc
        ''' </summary>
        ReadOnly m_levels As New Dictionary(Of String, Integer)

        ''' <summary>
        ''' level的数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property nlevel As Integer
            Get
                Return m_levels.Count
            End Get
        End Property

        Public ReadOnly Property levels As String()
            Get
                Return m_levels.Keys.ToArray
            End Get
        End Property

        Public ReadOnly Property factorId As String
            Get
                Return m_levels.Keys.JoinBy("|").MD5
            End Get
        End Property

        Sub New()
            m_type = RType.GetRSharpType(GetType(Integer))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function asNumeric(labels As IEnumerable(Of String)) As Double()
            Return labels _
                .Select(Function(si) CDbl(GetFactor(si))) _
                .ToArray
        End Function

        Public Shared Function CreateFactor(x$(),
                                            Optional exclude$() = Nothing,
                                            Optional ordered As Boolean = False,
                                            Optional nmax% = Nothing) As factor

            If Not exclude.IsNullOrEmpty Then
                With exclude.Indexing
                    x = x _
                        .Where(Function(s) .IndexOf(s) = -1) _
                        .ToArray
                End With
            End If

            If Not ordered Then
                x = x.OrderBy(Function(s) s).ToArray
            End If

            Dim factor As New factor
            Dim uniqueIndex As String()

            If nmax > 0 Then
                uniqueIndex = x.Distinct.Take(nmax).ToArray
            Else
                uniqueIndex = x.Distinct.ToArray
            End If

            For Each level As String In uniqueIndex
                Call factor.m_levels.Add(level, factor.m_levels.Count + 1)
            Next

            Return factor
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="lb"></param>
        ''' <returns>
        ''' get 1 based factor level integer value, zero means nothing or missing
        ''' </returns>
        Public Function GetFactor(lb As String) As Integer
            If lb Is Nothing Then
                Return 0
            ElseIf Not m_levels.ContainsKey(lb) Then
                Return 0
            Else
                Return m_levels(lb)
            End If
        End Function

        Public Shared Function asFactor(raw As String(), factor As factor) As vector
            Dim vector As Integer() = raw _
                .Select(AddressOf factor.GetFactor) _
                .ToArray

            Return New vector(vector, factor.elementType) With {
                .factor = factor
            }
        End Function

        ''' <summary>
        ''' 将目标等级值转换为原始字符串值
        ''' </summary>
        ''' <param name="levels"></param>
        ''' <param name="factor"></param>
        ''' <returns></returns>
        Public Shared Function asCharacter(levels As Integer(), factor As factor) As String()
            Dim factors As String() = factor.levels
            Dim chars As String() = (From i As Integer
                                     In levels
                                     Select factors(i - 1)).ToArray
            Return chars
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function asCharacter(data As vector) As String()
            Return asCharacter(
                levels:=CLRVector.asInteger(data.data),
                factor:=data.factor
            )
        End Function

        Public Shared Function checkSize(factor_size As Integer, factors As String()) As String
            If std.Abs(factor_size - factors.Length) / factors.Length < 0.05 Then
                If DataFramework.IsNumericType(DataImports.SampleForType(factors)) Then
                    Return "the given factor collection is probably a numeric vector, consider convert as numeric vector."
                Else
                    Return "the given character vector is not recommended used as the factor due to the reason of factor size is too large."
                End If
            End If

            Return Nothing
        End Function

        Public Shared Narrowing Operator CType(factors As factor) As Dictionary(Of String, Integer)
            If factors Is Nothing Then
                Return Nothing
            Else
                Return factors.m_levels
            End If
        End Operator
    End Class
End Namespace
