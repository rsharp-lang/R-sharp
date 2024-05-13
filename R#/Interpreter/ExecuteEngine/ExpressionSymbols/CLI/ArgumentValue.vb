#Region "Microsoft.VisualBasic::b3b98e201536d62e72b4a999a6a7087e, R#\Interpreter\ExecuteEngine\ExpressionSymbols\CLI\ArgumentValue.vb"

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

    '   Total Lines: 86
    '    Code Lines: 54
    ' Comment Lines: 15
    '   Blank Lines: 17
    '     File Size: 2.84 KB


    '     Delegate Function
    ' 
    ' 
    '     Class ArgumentValue
    ' 
    '         Properties: expressionName, name, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Evaluate, ToString
    ' 
    '         Sub: SetArgumentHandler, SetConfigName
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols

    Public Delegate Function GetArgumentValueHandler(configKey As String) As Object

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

        Dim configName As String

        ''' <summary>
        ''' The argument name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property name As Expression

        Shared getArgumentValue As GetArgumentValueHandler

        Sub New(name As Expression)
            Me.name = name
        End Sub

        Public Overrides Function ToString() As String
            Return $"$ARGV[[{name}]]"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Sub SetArgumentHandler(handler As GetArgumentValueHandler)
            getArgumentValue = handler
        End Sub

        Friend Sub SetConfigName(name As String)
            configName = name
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim arg As String = REnv.getFirst(name.Evaluate(envir))
            Dim value As Object
            Dim arguments As CommandLine = App.CommandLine

            If arguments.ContainsParameter(arg) OrElse arguments.HavebFlag(arg) Then
                Return CType(arguments(arg), String)
            ElseIf getArgumentValue IsNot Nothing AndAlso Not configName.StringEmpty Then
                Return getArgumentValue(configName)
            End If

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

            Return value
        End Function
    End Class
End Namespace
