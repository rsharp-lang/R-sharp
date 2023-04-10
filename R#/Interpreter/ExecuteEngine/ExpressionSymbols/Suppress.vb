#Region "Microsoft.VisualBasic::e35f2fecb6522519037a47215bc3cd8c, D:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/ExpressionSymbols/Suppress.vb"

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

    '   Total Lines: 36
    '    Code Lines: 29
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 1.24 KB


    '     Class Suppress
    ' 
    '         Properties: expression, expressionName, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Development.Package.File

Namespace Interpreter.ExecuteEngine.ExpressionSymbols

    Public Class Suppress : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
        Public ReadOnly Property expression As Expression

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Suppress
            End Get
        End Property

        Sub New(evaluate As Expression)
            expression = evaluate
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim result As Object = expression.Evaluate(envir)

            If Not result Is Nothing AndAlso result.GetType Is GetType(Message) Then
                Dim message As Message = result
                message.level = MSG_TYPES.WRN
                envir.globalEnvironment.messages.Add(message)
                Return Nothing
            Else
                Return result
            End If
        End Function
    End Class
End Namespace
