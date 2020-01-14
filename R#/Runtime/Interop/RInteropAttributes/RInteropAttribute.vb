#Region "Microsoft.VisualBasic::c17b840f1c20d6e3f4a0b6914db20e1b, R#\Runtime\Interop\RInteropAttributes\RInteropAttribute.vb"

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

    '     Class RInteropAttribute
    ' 
    ' 
    ' 
    '     Class RByRefValueAssignAttribute
    ' 
    ' 
    ' 
    '     Class RRawVectorArgumentAttribute
    ' 
    ' 
    ' 
    '     Class RParameterNameAliasAttribute
    ' 
    '         Properties: [alias]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.Interop

    <AttributeUsage(AttributeTargets.All, AllowMultiple:=False, Inherited:=True)>
    Public Class RInteropAttribute : Inherits Attribute
    End Class

    ''' <summary>
    ''' 这个参数是接受``a(x) &lt;- y``操作之中的``y``结果值的
    ''' </summary>
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RByRefValueAssignAttribute : Inherits RInteropAttribute
    End Class

    Public Interface IVectorExpressionLiteral

        Function ParseVector(default$, schema As Type) As Array

    End Interface

    ''' <summary>
    ''' 表示这个参数是一个数组，环境系统不应该自动调用getFirst取第一个值
    ''' </summary>
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

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="vector">The element type of the target vector type</param>
        ''' <param name="parser"><see cref="IVectorExpressionLiteral"/></param>
        Sub New(Optional vector As Type = Nothing, Optional parser As Type = Nothing)
            Me.vector = vector
            Me.parser = parser
        End Sub

        Public Function GetVector([default] As String) As Array
            Dim literal As IVectorExpressionLiteral = DirectCast(Activator.CreateInstance(parser), IVectorExpressionLiteral)
            Dim vector As Array = literal.ParseVector([default], schema:=Me.vector)

            Return vector
        End Function
    End Class

    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RParameterNameAliasAttribute : Inherits RInteropAttribute

        Public ReadOnly Property [alias] As String

        Sub New([alias] As String)
            Me.alias = [alias]
        End Sub

        Public Overrides Function ToString() As String
            Return [alias]
        End Function
    End Class
End Namespace
