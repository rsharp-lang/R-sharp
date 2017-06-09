#Region "Microsoft.VisualBasic::0ca028d2ef97514b35389e6e09634d91, ..\sciBASIC#\Data_science\Mathematical\Math\Scripting\Arithmetic.Expression\FuncCaller.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Mathematical.Scripting.Types
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Abstract

''' <summary>
''' Function object model.(调用函数的方法)
''' </summary>
Public Class FuncCaller

    Public ReadOnly Property Name As String
    Public ReadOnly Property Params As New List(Of SimpleExpression)

    ReadOnly __calls As FunctionEvaluate

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Name">The function name</param>
    ''' <param name="evaluate">Engine handle</param>
    Sub New(name$, evaluate As FunctionEvaluate)
        Me.Name = name
        Me.__calls = evaluate
    End Sub

    Public Overrides Function ToString() As String
        Dim args As String() = Params.ToArray(Function(x) x.ToString)
        Return $"{Name}({args.JoinBy(", ")})"
    End Function

    Public Function Evaluate() As Double
        Return __calls(Name, Params.ToArray(Function(x) x.Evaluate))
    End Function
End Class