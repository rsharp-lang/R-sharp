#Region "Microsoft.VisualBasic::a9910b13625b7787d1c9bdc1d1924059, E:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/Linq/Expression/Expressions/VectorLiteral.vb"

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

    '   Total Lines: 21
    '    Code Lines: 16
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 613 B


    '     Class VectorLiteral
    ' 
    '         Properties: elements, name
    ' 
    '         Function: Exec, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class VectorLiteral : Inherits Expression

        Public Overrides ReadOnly Property name As String
            Get
                Return "[...]"
            End Get
        End Property

        Public Property elements As Expression()

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Throw New NotImplementedException()
        End Function

        Public Overrides Function ToString() As String
            Return $"[{elements.JoinBy(", ")}]"
        End Function
    End Class
End Namespace
