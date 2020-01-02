#Region "Microsoft.VisualBasic::c7790f9eb6eaa53a393c94d25cbecf78, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Package\Require.vb"

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

    '     Class Require
    ' 
    '         Properties: packages, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class Require : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Public Property packages As Expression()

        Sub New(names As Token())
            packages = names _
                .SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(b) Not b.isComma) _
                .Select(AddressOf Expression.CreateExpression) _
                .ToArray
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim names As New List(Of String)
            Dim [global] As GlobalEnvironment = envir.globalEnvironment
            Dim pkgName As String
            Dim message As Message

            For Each name As Expression In packages
                pkgName = ValueAssign.GetSymbol(name)
                message = [global].LoadLibrary(pkgName)

                If Not message Is Nothing Then
                    Call Internal.debug.PrintMessageInternal(message)
                End If
            Next

            Return names.ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"require({packages.JoinBy(", ")})"
        End Function
    End Class
End Namespace
