#Region "Microsoft.VisualBasic::af2c2af6e011636cf1762f012c75681f, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\BinaryInExpression.vb"

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

    '     Class BinaryInExpression
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, getIndex, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class BinaryInExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.boolean
            End Get
        End Property

        ''' <summary>
        ''' left
        ''' </summary>
        Friend a As Expression
        ''' <summary>
        ''' right
        ''' </summary>
        Friend b As Expression

        Sub New(a As Expression, b As Expression)
            Me.a = a
            Me.b = b
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim index As Index(Of Object) = getIndex(b, envir).Indexing
            Dim findTest As Boolean() = getIndex(a, envir) _
                .Select(Function(x)
                            Return x Like index
                        End Function) _
                .ToArray

            Return findTest
        End Function

        Private Shared Function getIndex(src As Expression, env As Environment) As Object()
            Dim isList As Boolean = False
            Dim seq = LinqExpression.produceSequenceVector(src, env, isList)

            If isList Then
                Return DirectCast(seq, KeyValuePair(Of String, Object)()) _
                    .Select(Function(t) t.Value) _
                    .ToArray
            Else
                Return DirectCast(seq, Object())
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"({a} %in% index<{b}>)"
        End Function
    End Class
End Namespace
