#Region "Microsoft.VisualBasic::929ed840043e204bf5858d6bdba00852, R#\Runtime\Interop\RInteropAttributes\VectorMask\RRawVectorArgumentAttribute.vb"

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

    '   Total Lines: 77
    '    Code Lines: 35
    ' Comment Lines: 31
    '   Blank Lines: 11
    '     File Size: 2.99 KB


    '     Class RRawVectorArgumentAttribute
    ' 
    '         Properties: containsLiteral, parser, vector
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetVector, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Interop

    ''' <summary>
    ''' 表示这个参数是一个数组，环境系统不应该自动调用getFirst取第一个值
    ''' </summary>
    ''' <remarks>
    ''' there are some string literal rule for the default 
    ''' vector string parser:
    ''' 
    ''' 1. 字符串类型默认使用``|``作为分隔符
    ''' 2. 数值类型默认使用``,``作为分隔符
    ''' </remarks>
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RRawVectorArgumentAttribute : Inherits RInteropAttribute

        ''' <summary>
        ''' The element type of the target vector type
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' If this property is not null, then it means the optional argument have 
        ''' a default string expression value which could be parsed as current vector
        ''' type.
        ''' </remarks>
        Public ReadOnly Property vector As Type
        Public ReadOnly Property parser As Type

        Public ReadOnly Property containsLiteral As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Not (vector Is Nothing OrElse parser Is Nothing)
            End Get
        End Property

        ''' <summary>
        ''' <paramref name="parser"/>参数的默认值为<see cref="DefaultVectorParser"/>
        ''' </summary>
        ''' <param name="vector">The element type of the target vector type</param>
        ''' <param name="parser">
        ''' <see cref="IVectorExpressionLiteral"/>
        ''' 
        ''' use <see cref="DefaultVectorParser"/> by default.
        ''' </param>
        Sub New(vector As Type, Optional parser As Type = Nothing)
            Me.vector = vector
            Me.parser = If(parser, GetType(DefaultVectorParser))
        End Sub

        ''' <summary>
        ''' construct a vector flag with any object type and use the <see cref="DefaultVectorParser"/>.
        ''' </summary>
        Sub New()
            Me.vector = Nothing
            Me.parser = GetType(DefaultVectorParser)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(vector As TypeCodes, Optional parser As Type = Nothing)
            Call Me.New(RType.GetType(vector).GetRawElementType, parser)
        End Sub

        Public Function GetVector([default] As String) As Array
            Dim literal As IVectorExpressionLiteral = DirectCast(Activator.CreateInstance(parser), IVectorExpressionLiteral)
            Dim vector As Array = literal.ParseVector([default], schema:=Me.vector)

            Return vector
        End Function

        Public Overrides Function ToString() As String
            Return MyBase.ToString()
        End Function
    End Class

End Namespace
