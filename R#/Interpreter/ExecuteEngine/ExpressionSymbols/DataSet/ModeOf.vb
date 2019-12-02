#Region "Microsoft.VisualBasic::2499055bd94ca3877ea4924ed4fa46b2, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\ModeOf.vb"

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

    '     Class ModeOf
    ' 
    '         Properties: keyword, target, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class ModeOf : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
        Public ReadOnly Property keyword As String
        Public ReadOnly Property target As Expression

        Sub New(keyword$, target As Token())
            Me.keyword = keyword
            Me.target = Expression.CreateExpression(target)
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim value As Object = target.Evaluate(envir)

            If keyword = "modeof" Then
                If value Is Nothing Then
                    Return TypeCodes.NA.Description
                Else
                    Return value.GetType.GetRTypeCode.Description
                End If
            Else
                If value Is Nothing Then
                    Return GetType(Void)
                Else
                    Return value.GetType
                End If
            End If
        End Function
    End Class
End Namespace
