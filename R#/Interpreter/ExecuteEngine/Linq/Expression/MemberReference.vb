#Region "Microsoft.VisualBasic::ee1acc0e646a584db6e68fa1f83b9abd, LINQ\LINQ\Interpreter\Expressions\MemberReference.vb"

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

'     Class MemberReference
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: Exec, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.My.JavaScript
Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class MemberReference : Inherits Expression

        Friend ReadOnly symbol As Expression
        Friend ReadOnly memberName As String

        Sub New(symbol As Expression, memberName As Expression)
            Me.symbol = symbol

            If TypeOf memberName Is SymbolReference Then
                Me.memberName = DirectCast(memberName, SymbolReference).symbolName
            ElseIf TypeOf memberName Is Literals Then
                Me.memberName = any.ToString(memberName.Exec(Nothing))
            Else
                Throw New InvalidExpressionException(memberName.name)
            End If
        End Sub

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Dim symbol As Object = Me.symbol.Exec(context)

            If symbol Is Nothing Then
                Throw New NullReferenceException
            End If

            If TypeOf symbol Is JavaScriptObject Then
                Return DirectCast(symbol, JavaScriptObject)(memberName)
            End If

            Throw New NotImplementedException()
        End Function

        Public Overrides Function ToString() As String
            Return $"{symbol}->{memberName}"
        End Function
    End Class
End Namespace
