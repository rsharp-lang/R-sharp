#Region "Microsoft.VisualBasic::32c654aa8b42cc8d97c866904f1a0cea, F:/GCModeller/src/R-sharp/studio/Rsharp_IL/nts//Models/ExpressionCollection.vb"

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

    '   Total Lines: 30
    '    Code Lines: 21
    ' Comment Lines: 3
    '   Blank Lines: 6
    '     File Size: 1.05 KB


    ' Class ExpressionCollection
    ' 
    '     Properties: expressionName, expressions, type
    ' 
    '     Function: Evaluate, GetExpressions, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

''' <summary>
''' A temp expression collection for the function invoke parameters
''' </summary>
Public Class ExpressionCollection : Inherits Expression

    Public Overrides ReadOnly Property type As TypeCodes
    Public Overrides ReadOnly Property expressionName As Rsharp.Development.Package.File.ExpressionTypes

    Public Property expressions As Expression()

    Public Shared Function GetExpressions(exp As Expression) As Expression()
        If TypeOf exp Is ExpressionCollection Then
            Return DirectCast(exp, ExpressionCollection).expressions
        Else
            Return {exp}
        End If
    End Function

    Public Overrides Function Evaluate(envir As Environment) As Object
        Throw New NotImplementedException()
    End Function

    Public Overrides Function ToString() As String
        Return "( " & expressions.JoinBy(", ") & " )"
    End Function
End Class
