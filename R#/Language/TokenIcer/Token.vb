#Region "Microsoft.VisualBasic::6304332aa88ea1f62875190084b197f7, G:/GCModeller/src/R-sharp/R#//Language/TokenIcer/Token.vb"

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

    '   Total Lines: 74
    '    Code Lines: 53
    ' Comment Lines: 11
    '   Blank Lines: 10
    '     File Size: 2.69 KB


    '     Class Token
    ' 
    '         Properties: isLiteral, literal
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Language.TokenIcer

    ''' <summary>
    ''' the R# language token word
    ''' </summary>
    Public Class Token : Inherits CodeToken(Of TokenType)

        ''' <summary>
        ''' get the literal value of current token text represented.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property literal As Object
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Select Case name
                    Case TokenType.stringLiteral
                        Return text
                    Case TokenType.booleanLiteral
                        Return text.ParseBoolean
                    Case TokenType.numberLiteral
                        Return text.ParseDouble
                    Case TokenType.integerLiteral
                        Return text.ParseLong

                    Case Else
                        Return Nothing
                End Select
            End Get
        End Property

        ''' <summary>
        ''' is value literal of current token type?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isLiteral As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return name = TokenType.booleanLiteral OrElse
                       name = TokenType.integerLiteral OrElse
                       name = TokenType.missingLiteral OrElse
                       name = TokenType.numberLiteral OrElse
                       name = TokenType.stringLiteral
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(name As TokenType, Optional value$ = Nothing)
            Call MyBase.New(name, value)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(type As TokenType) As Token
            Return New Token(type)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator =(token As Token, type As TokenType) As Boolean
            Return token.name = type
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator <>(token As Token, type As TokenType) As Boolean
            Return Not token = type
        End Operator
    End Class
End Namespace
