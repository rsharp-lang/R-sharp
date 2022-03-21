#Region "Microsoft.VisualBasic::679ce37ce382a005acf36c723bbb5039, R-sharp\R#\Interpreter\ExecuteEngine\Linq\Expression\Expressions\MemberReference.vb"

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

    '   Total Lines: 51
    '    Code Lines: 40
    ' Comment Lines: 0
    '   Blank Lines: 11
    '     File Size: 1.84 KB


    '     Class MemberReference
    ' 
    '         Properties: name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Exec, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.My.JavaScript
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class MemberReference : Inherits Expression

        Friend ReadOnly symbol As Expression
        Friend ReadOnly memberName As String

        Sub New(symbol As Expression, memberName As Expression)
            Me.symbol = symbol

            If TypeOf memberName Is SymbolReference Then
                Me.memberName = DirectCast(memberName, SymbolReference).symbolName
            ElseIf TypeOf memberName Is Literal Then
                Me.memberName = any.ToString(DirectCast(memberName, Literal).value)
            Else
                Throw New InvalidExpressionException(memberName.name)
            End If
        End Sub

        Public Overrides ReadOnly Property name As String
            Get
                Return "x$ref"
            End Get
        End Property

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Dim symbol As Object = Me.symbol.Exec(context)

            If symbol Is Nothing Then
                Return Internal.debug.stop({"target symbol is nothing!", "symbol: " & Me.symbol.ToString}, context)
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
