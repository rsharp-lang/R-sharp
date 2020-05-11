#Region "Microsoft.VisualBasic::ccba6b2794eb160da150a75735d0b723, R#\Interpreter\Syntax\SyntaxResult.vb"

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

    '     Class SyntaxResult
    ' 
    '         Properties: isException
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Interpreter.SyntaxParser

    ''' <summary>
    ''' The R# expression syntax build result, the result of this 
    ''' model could be an error or resulted expression model 
    ''' object. 
    ''' </summary>
    Friend Class SyntaxResult

        Public ReadOnly [error] As Exception
        Public ReadOnly expression As Expression

        ''' <summary>
        ''' 
        ''' </summary>
        Public ReadOnly stackTrace As String

        Public ReadOnly Property isException As Boolean
            Get
                Return Not [error] Is Nothing
            End Get
        End Property

        Public Sub New(syntax As Expression)
            Me.expression = syntax
        End Sub

        Sub New(err As Exception, debug As Boolean)
            If debug Then
                Throw err
            Else
                Me.stackTrace = Environment.StackTrace
                Me.error = err
            End If
        End Sub

        Sub New(err$, debug As Boolean)
            Call Me.New(New SyntaxErrorException(err), debug)
        End Sub

        Public Overrides Function ToString() As String
            If isException Then
                Return stackTrace
            Else
                Return expression.ToString
            End If
        End Function

        Public Shared Widening Operator CType(syntax As Expression) As SyntaxResult
            Return New SyntaxResult(syntax)
        End Operator

    End Class
End Namespace
