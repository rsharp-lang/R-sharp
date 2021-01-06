#Region "Microsoft.VisualBasic::1279339c01e9e4d8ac88d0e9f7b964f6, R#\Interpreter\ExecuteEngine\ExpressionSymbols\CLI\ArgumentValue.vb"

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

    '     Class ArgumentValue
    ' 
    '         Properties: expressionName, name, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Development.Package.File
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols

    ''' <summary>
    ''' Get commandline argument or safely get environment symbol value by name
    ''' 
    ''' ```
    ''' ?'name'
    ''' ```
    ''' </summary>
    Public Class ArgumentValue : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.string
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.GetArgument
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
            Dim arg As String = REnv.getFirst(name.Evaluate(envir))
            Dim value As Object
            Dim arguments = App.CommandLine

            If arguments.ContainsParameter(arg) OrElse arguments.HavebFlag(arg) Then
                value = CType(arguments(arg), String)
            Else
                Dim symbol As Symbol = envir.FindSymbol(arg)

                If symbol Is Nothing Then
                    ' 20200428
                    ' 在这里为了保持和之前的行为一致
                    ' 之前的版本之中，查找失败的命令行参数返回false
                    ' 在这里将无法查找到的都设置为false 
                    value = CType(arguments(arg), String)
                Else
                    value = symbol.value
                End If
            End If

            Return value
        End Function
    End Class
End Namespace
