#Region "Microsoft.VisualBasic::ef0d94de7b9677f42392b71f8dd9d100, studio\R.exec\Compiler.vb"

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

    ' Module Compiler
    ' 
    '     Function: Build, DeclareNewVariable
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports RExpression = SMRUCC.Rsharp.Interpreter.ExecuteEngine.Expression

''' <summary>
''' R# executable file format and script compiler
''' </summary>
Public Module Compiler

    Public Iterator Function Build(script As String) As IEnumerable(Of variable)
        Dim program As Program = Program.BuildProgram(script)
        Dim buffer As variable()
        Dim i32ptr As Integer = Scan0

        For Each line As RExpression In program
            Select Case line.GetType
                Case GetType(DeclareNewSymbol)
                    buffer = DirectCast(line, DeclareNewSymbol).DeclareNewVariable
            End Select
        Next
    End Function

    <Extension>
    Private Function DeclareNewVariable(declares As DeclareNewSymbol) As variable()
        Dim [let] As New variable
        ' Dim names = declares.
    End Function
End Module
