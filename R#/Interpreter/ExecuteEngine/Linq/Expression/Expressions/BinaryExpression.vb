#Region "Microsoft.VisualBasic::4baad676d57435cad035c435b530987c, R#\Interpreter\ExecuteEngine\Linq\Expression\Expressions\BinaryExpression.vb"

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

    '     Class BinaryExpression
    ' 
    '         Properties: LikeValueAssign, name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Exec, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' the linq binary expression
    ''' </summary>
    Public Class BinaryExpression : Inherits Expression

        Friend left, right As Expression
        Dim op As String

        Public ReadOnly Property LikeValueAssign As Boolean
            Get
                If op <> "=" Then
                    Return False
                Else
                    Return TypeOf left Is SymbolReference
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property name As String
            Get
                Return $"(binary) a {op} b"
            End Get
        End Property

        Sub New(left As Expression, right As Expression, op As String)
            Me.left = left
            Me.right = right
            Me.op = op
        End Sub

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Dim x As Object = left.Exec(context)
            Dim y As Object = right.Exec(context)

            Select Case op
                Case "+" : Return x + y
                Case "-" : Return x - y
                Case "*" : Return x * y
                Case "/" : Return x / y
                Case "^" : Return x ^ y
                Case ">" : Return x > y
                Case "<" : Return x < y
                Case "==" : Return x = y
                Case "%" : Return x Mod y

                Case "&" : Return any.ToString(x) & any.ToString(y)

                Case ">=" : Return x >= y
                Case "<=" : Return x <= y
                Case "<>", "!=" : Return x <> y

                Case "&&" : Return CBool(x) AndAlso CBool(y)
                Case "||" : Return CBool(x) OrElse CBool(y)

                Case Else
                    Throw New NotImplementedException
            End Select
        End Function

        Public Overrides Function ToString() As String
            Return $"({left} {op} {right})"
        End Function
    End Class
End Namespace
