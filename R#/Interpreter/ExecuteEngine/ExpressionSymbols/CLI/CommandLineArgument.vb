#Region "Microsoft.VisualBasic::f893612f2db17fe73ef941bec6fad2a6, R#\Interpreter\ExecuteEngine\ExpressionSymbols\CLI\CommandLineArgument.vb"

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

    '     Class CommandLineArgument
    ' 
    '         Properties: name, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Default
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' Get commandline argument value by name
    ''' 
    ''' ```
    ''' ?'name'
    ''' ```
    ''' </summary>
    Public Class CommandLineArgument : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.string
            End Get
        End Property

        ''' <summary>
        ''' The argument name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property name As Expression

        Sub New(name As Expression)
            Me.name = name
        End Sub

        Public Overrides Function ToString() As String
            Return $"$ARGV[[{name}]]"
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim arg As String = Runtime.getFirst(name.Evaluate(envir))
            Dim value As DefaultString = App.CommandLine(arg)

            Return CType(value, String)
        End Function
    End Class
End Namespace
