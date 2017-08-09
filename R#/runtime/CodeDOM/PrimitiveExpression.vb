#Region "Microsoft.VisualBasic::6ec2710242439b8521958cd6c73f6832, ..\R-sharp\R#\runtime\CodeDOM\PrimitiveExpression.vb"

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

Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.Expression

Namespace Runtime.CodeDOM

    Public Structure TempValue

        Dim type As TypeCodes
        Dim value As Object

        Public Shared Function Tuple(value As Object, type As TypeCodes) As TempValue
            Return New TempValue With {
                .type = type,
                .value = value
            }
        End Function

        Public Overrides Function ToString() As String
            Return $"[{type}] {CStrSafe(value)}"
        End Function
    End Structure

    ''' <summary>
    ''' The very base expression in the R# language
    ''' </summary>
    Public Class PrimitiveExpression

        Public Overridable Function Evaluate(envir As Environment) As TempValue

        End Function
    End Class

    ''' <summary>
    ''' The expression which can produce the values
    ''' </summary>
    Public Class ValueExpression : Inherits PrimitiveExpression

        ReadOnly tree As Func(Of Environment, SimpleExpression)

        Sub New(tokens As IEnumerable(Of Token(Of LanguageTokens)))
            tree = New Pointer(Of Token(Of LanguageTokens))(tokens).TryParse
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As TempValue
            Dim out = tree(envir).Evaluate(envir)
            Return out
        End Function
    End Class
End Namespace
