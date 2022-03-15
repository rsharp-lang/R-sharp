#Region "Microsoft.VisualBasic::cca8bfaf0a49504929bf308bb0dc899e, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\ObjectWith.vb"

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

    '   Total Lines: 62
    '    Code Lines: 48
    ' Comment Lines: 3
    '   Blank Lines: 11
    '     File Size: 2.25 KB


    '     Class ObjectWith
    ' 
    '         Properties: expressionName, stackFrame, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' syntax implementation for ``with`` keyword
    ''' </summary>
    Public Class ObjectWith : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.With
            End Get
        End Property

        Dim closure As ClosureExpression
        Dim target As Expression
        Dim isModifyWith As Boolean

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                If isModifyWith Then
                    Return target.type
                Else
                    Return closure.type
                End If
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Sub New(target As Expression, closure As ClosureExpression, isModifyWith As Boolean, stacktrace As StackFrame)
            Me.target = target
            Me.closure = closure
            Me.isModifyWith = isModifyWith
            Me.stackFrame = stacktrace
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim obj As Object = target.Evaluate(envir)

            If Program.isException(obj) Then
                Return obj
            ElseIf obj Is Nothing Then
                envir.AddMessage($"the expression evaluation result of '{target}' is nothing!", MSG_TYPES.WRN)
                Return Nothing
            End If

            Dim objEnv As New ObjectEnvironment(obj, envir, stackFrame)
            Dim result As Object = closure.Evaluate(envir:=objEnv)

            Return result
        End Function
    End Class
End Namespace
