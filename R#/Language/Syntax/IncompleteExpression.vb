#Region "Microsoft.VisualBasic::42d48d1811ae797f16a0b57f5dc9fc46, R#\Language\Syntax\IncompleteExpression.vb"

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

    '   Total Lines: 136
    '    Code Lines: 79 (58.09%)
    ' Comment Lines: 34 (25.00%)
    '    - Xml Docs: 70.59%
    ' 
    '   Blank Lines: 23 (16.91%)
    '     File Size: 4.35 KB


    '     Class IncompleteExpression
    ' 
    '         Properties: Check
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Append, CheckTokenSequence, eval, GetRScript, GetRScriptText
    '                   PopRScriptText, scanStackOpen
    ' 
    '         Sub: Clear
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

Namespace Language.Syntax

    ''' <summary>
    ''' Helper for R shell terminal multiple line editing
    ''' </summary>
    Public NotInheritable Class IncompleteExpression : Inherits RDefaultFunction

        Dim lines As New List(Of String)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns>
        ''' true means the script expression is in-complete
        ''' </returns>
        Public ReadOnly Property Check() As Boolean
            Get
                Dim script As String = lines.JoinBy(vbCrLf)
                Dim tokens As Token() = Language.ParseScript(script)

                Return CheckTokenSequence(tokens)
            End Get
        End Property

        Sub New()
        End Sub

        <RDefaultFunction>
        Public Function Append(line As String) As IncompleteExpression
            Call lines.Add(line)
            Return Me
        End Function

        Public Sub Clear()
            Call lines.Clear()
        End Sub

        Public Function GetRScriptText() As String
            Return lines.JoinBy(vbCrLf)
        End Function

        Public Function PopRScriptText() As String
            Dim script As String = GetRScriptText()
            Call Clear()
            Return script
        End Function

        Public Function GetRScript() As Rscript
            Return Rscript.AutoHandleScript(lines.JoinBy(vbCrLf))
        End Function

        Public Function eval(env As Environment) As Object
            If Check() Then
                Return RInternal.debug.stop("in-complete expression to evaluate.", env)
            End If

            Return Program.CreateProgram(GetRScript,,).Execute(env)
        End Function

        ''' <summary>
        ''' test the given line tokens is in-complete expression or not?
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' in-complete expression:
        ''' 
        ''' 1. ends with operator token
        ''' 2. bracket stack not closed
        ''' </remarks>
        Public Shared Function CheckTokenSequence(tokens As Token()) As Boolean
            If tokens.IsNullOrEmpty Then
                ' special case:
                ' empty expression is completed
                Return False
            End If

            ' ends with open:  xxx(
            ' ends with operator: xxx *
            ' ends with comma: xxx(aaa,
            If tokens.Last.name = TokenType.open OrElse
                tokens.Last.name = TokenType.operator OrElse
                tokens.Last.name = TokenType.comma Then

                Return True
            End If

            ' check of the stack closed?
            Dim parts = Splitter.SplitByTopLevelDelimiter(tokens, TokenType.operator, includeKeyword:=True)

            For Each part As Token() In parts
                If scanStackOpen(part) Then
                    Return True
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <returns>
        ''' returns true means the stack is opened
        ''' </returns>
        Private Shared Function scanStackOpen(tokens As Token()) As Boolean
            Dim stack As New Stack(Of Token)

            For Each ti As Token In tokens
                If ti.name = TokenType.open Then
                    stack.Push(ti)
                ElseIf ti.name = TokenType.close Then
                    If stack.Count = 0 Then
                        ' syntax error
                        Return False
                    End If

                    If Splitter.CheckOfStackOpenClosePair(stack.Peek, ti) Then
                        Call stack.Pop()
                    End If
                End If
            Next

            Return Not stack.Empty
        End Function
    End Class
End Namespace
