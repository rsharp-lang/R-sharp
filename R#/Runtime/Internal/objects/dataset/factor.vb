#Region "Microsoft.VisualBasic::832846094ab84115f2ae438f8da686c0, R-sharp\R#\Runtime\Internal\objects\dataset\factor.vb"

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

    '   Total Lines: 111
    '    Code Lines: 78
    ' Comment Lines: 16
    '   Blank Lines: 17
    '     File Size: 3.79 KB


    '     Class factor
    ' 
    '         Properties: levels, nlevel
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) asCharacter, asFactor, CreateFactor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' string to integer dictionary
    ''' </summary>
    ''' <remarks>
    ''' 这个对象只是相当于unit的一个存在
    ''' </remarks>
    Public Class factor : Inherits RsharpDataObject

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

        Sub New()
            m_type = RType.GetRSharpType(GetType(Integer))
        End Sub

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

        Public Shared Function asFactor(raw As String(), factor As factor) As vector
            Dim vector As Integer() = raw _
                .Select(Function(str)
                            If str Is Nothing Then
                                Return 0
                            ElseIf Not factor.m_levels.ContainsKey(str) Then
                                Return 0
                            Else
                                Return factor.m_levels(str)
                            End If
                        End Function) _
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
                levels:=REnv.asVector(Of Integer)(data.data),
                factor:=data.factor
            )
        End Function
    End Class
End Namespace
