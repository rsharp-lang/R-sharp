#Region "Microsoft.VisualBasic::7793e7119ebf2a9f7d9bba1fd5485ab0, F:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/Linq/Expression/Expressions/ValueAssign.vb"

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

    '   Total Lines: 25
    '    Code Lines: 19
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 733 B


    '     Class ValueAssign
    ' 
    '         Properties: name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Exec
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports RExpression = SMRUCC.Rsharp.Interpreter.ExecuteEngine.Expression

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class ValueAssign : Inherits Expression

        Public Overrides ReadOnly Property name As String
            Get
                Return "a <- x"
            End Get
        End Property

        Friend symbolName As String
        Friend value As RExpression

        Sub New(symbolName As String, value As RExpression)
            Me.value = value
            Me.symbolName = symbolName
        End Sub

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
