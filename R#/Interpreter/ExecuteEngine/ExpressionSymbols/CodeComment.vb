#Region "Microsoft.VisualBasic::5ef37b2f7ec68913a82dd524d40b0741, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\CodeComment.vb"

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

    '   Total Lines: 95
    '    Code Lines: 73
    ' Comment Lines: 6
    '   Blank Lines: 16
    '     File Size: 3.19 KB


    '     Enum Annotations
    ' 
    '         BlockComment, EndRegion, LineComment, RegionStart
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class CodeComment
    ' 
    '         Properties: comment, CommentAnnotation, expressionName, span, type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Evaluate, FromBlockComments, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Development.Package.File
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Interpreter.ExecuteEngine.ExpressionSymbols

    Public Enum Annotations
        LineComment
        BlockComment
        RegionStart
        EndRegion
    End Enum

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

        Public ReadOnly Property comment As String
        Public ReadOnly Property span As CodeSpan

        Public ReadOnly Property CommentAnnotation As Annotations
            Get
                If comment = "#end region" Then
                    Return Annotations.EndRegion
                ElseIf comment.StartsWith("#region """) Then
                    Return Annotations.RegionStart
                ElseIf comment.Contains(ASCII.CR) OrElse comment.Contains(ASCII.LF) Then
                    Return Annotations.BlockComment
                Else
                    Return Annotations.LineComment
                End If
            End Get
        End Property

        Sub New(comment As String)
            comment = Strings.Trim(comment)

            If comment = "end region" Then
                _comment = "#end region"
            ElseIf comment.StartsWith("region """) Then
                _comment = "#" & comment
            Else
                _comment = comment
            End If
        End Sub

        Sub New(comment As Token)
            ' strip for R# notebook single line comment annotation
            ' like 
            '     #region "code block start"
            '     #end region
            '
            Call Me.New(Strings.RTrim(Mid(comment.text, 2)))

            Dim annotation = CommentAnnotation

            If annotation <> Annotations.LineComment AndAlso annotation <> Annotations.BlockComment Then
                span = comment.span
            End If
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return Nothing
        End Function

        Public Shared Function FromBlockComments(block As IEnumerable(Of Token)) As CodeComment
            Dim lines As String() = block _
                .Select(Function(t)
                            ' strip for R# notebook markdown code comment block
                            Return Strings.RTrim(Mid(t.text, 3))
                        End Function) _
                .ToArray
            Dim text As String = lines.JoinBy(ASCII.LF)

            Return New CodeComment(text)
        End Function

        Public Overrides Function ToString() As String
            Return "// " & comment
        End Function
    End Class
End Namespace
