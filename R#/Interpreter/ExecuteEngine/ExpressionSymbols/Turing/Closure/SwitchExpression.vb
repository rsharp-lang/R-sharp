#Region "Microsoft.VisualBasic::99b4991f75e3b1c11f9f3c661c71a69b, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\Closure\SwitchExpression.vb"

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

    '   Total Lines: 58
    '    Code Lines: 48
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 1.97 KB


    '     Class SwitchExpression
    ' 
    '         Properties: expressionName, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Evaluate
    ' 
    '         Sub: Add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    Public Class SwitchExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Switch
            End Get
        End Property

        Dim switch As Expression
        Dim [default] As Expression
        Dim sourceMap As StackFrame
        Dim hash As New Dictionary(Of String, Expression)

        Sub New(switch As Expression, [default] As Expression, sourceMap As StackFrame)
            Me.switch = switch
            Me.default = [default]
            Me.sourceMap = sourceMap
        End Sub

        Public Sub Add(key As String, exp As Expression)
            hash(key) = exp
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim switchVal As Object = switch.Evaluate(envir)
            Dim hasKey As String

            If Program.isException(switchVal) Then
                Return switchVal
            Else
                hasKey = any.ToString(REnv.single(switchVal))
            End If

            If hash.ContainsKey(hasKey) Then
                Return hash(hasKey).Evaluate(envir)
            ElseIf [default] Is Nothing Then
                Return Internal.debug.stop($"no switch for '{hasKey}'!", envir)
            Else
                Return [default].Evaluate(envir)
            End If
        End Function
    End Class
End Namespace
