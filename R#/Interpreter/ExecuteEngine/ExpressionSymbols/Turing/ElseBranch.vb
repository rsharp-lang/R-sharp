#Region "Microsoft.VisualBasic::f8c5d03f25244875dbbdfa08e26eb2ad, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\ElseBranch.vb"

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

    '     Class ElseBranch
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate
    ' 
    '     Class ElseIfBranch
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class ElseBranch : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Dim closure As DeclareNewFunction

        Sub New(code As Token())
            closure = New DeclareNewFunction With {
                .body = code _
                    .Skip(1) _
                    .Take(code.Length - 2) _
                    .DoCall(AddressOf ClosureExpression.ParseExpressionTree),
                .funcName = "else_branch_internal",
                .params = {}
            }
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            If envir.ifPromise = 0 Then
                Throw New SyntaxErrorException
            Else
                Dim last As IfBranch.IfPromise

                If envir.ifPromise.Last.Result = True Then
                    last = envir.ifPromise.Pop
                Else
                    last = New IfBranch.IfPromise(closure.Invoke(envir, {}), False) With {
                        .assignTo = envir.ifPromise.Last.assignTo
                    }
                End If

                Call last.DoValueAssign(envir)

                Return last
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"else {{
    {closure}
}}"
        End Function
    End Class

    Public Class ElseIfBranch : Inherits IfBranch

        Public Sub New(tokens As IEnumerable(Of Token))
            MyBase.New(tokens)
        End Sub
    End Class
End Namespace
