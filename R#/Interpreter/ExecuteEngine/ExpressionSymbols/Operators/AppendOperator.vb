#Region "Microsoft.VisualBasic::1b65e8b495ff5cb46eaf0bd8cacbff59, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\Append.vb"

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

    '     Class Append
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' ByVal append, not modify the source vector
    ''' 
    ''' ```
    ''' a &lt;&lt; b
    ''' ```
    ''' </summary>
    Public Class AppendOperator : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return a.type
            End Get
        End Property

        ReadOnly a, b As Expression

        Sub New(a As Expression, b As Expression)
            Me.a = a
            Me.b = b
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim x As Object = a.Evaluate(envir)
            Dim y As Object = b.Evaluate(envir)

            If x Is Nothing Then
                Return y
            ElseIf y Is Nothing Then
                Return x
            ElseIf x Is Nothing AndAlso y Is Nothing Then
                Return Nothing
            End If

            Dim type1 As Type = x.GetType
            Dim type2 As Type = y.GetType

            If type1.IsArray OrElse type1 Is GetType(vector) Then
                ' y should be vector
                ' execute the byval append
                Return Runtime.asVector(Of Object)(x) _
                    .AsObjectEnumerator _
                    .JoinIterates(Runtime.asVector(Of Object)(y).AsObjectEnumerator) _
                    .ToArray
            ElseIf type1 Is GetType(list) Then
                Return Internal.stop(New NotImplementedException, envir)
            End If

            Return Internal.stop(New NotImplementedException, envir)
        End Function
    End Class
End Namespace
