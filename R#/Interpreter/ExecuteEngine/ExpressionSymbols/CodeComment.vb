#Region "Microsoft.VisualBasic::90e3aaa45d12a1ad052f9f1f40ed40ec, R#\Interpreter\ExecuteEngine\ExpressionSymbols\CodeComment.vb"

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

'     Class CodeComment
' 
'         Properties: expressionName, type
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: Evaluate, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Development.Package.File
Imports Microsoft.VisualBasic.Text

Namespace Interpreter.ExecuteEngine.ExpressionSymbols

    Public Class CodeComment : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.NA
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Comment
            End Get
        End Property

        Dim comment$

        Sub New(comment As String)
            Me.comment = comment
        End Sub

        Sub New(comment As Token)
            Me.comment = comment.text
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return Nothing
        End Function

        Public Shared Function FromBlockComments(block As IEnumerable(Of Token)) As CodeComment
            Dim lines As String() = block.Select(Function(t) t.Trim("#"c, " "c)).ToArray
            Dim text As String = lines.JoinBy(ASCII.LF)

            Return New CodeComment(text)
        End Function

        Public Overrides Function ToString() As String
            Return "// " & comment
        End Function
    End Class
End Namespace
