#Region "Microsoft.VisualBasic::a52c256595c35bca8d15cdb98089ef40, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\Closure\TryCatchExpression.vb"

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


     Code Statistics:

        Total Lines:   67
        Code Lines:    55
        Comment Lines: 2
        Blank Lines:   10
        File Size:     2.39 KB


    '     Class TryCatchExpression
    ' 
    '         Properties: expressionName, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    Public Class TryCatchExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.generic
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.TryCatch
            End Get
        End Property

        Dim sourceMap As StackFrame
        Dim [try] As Expression
        Dim [catch] As Expression
        Dim exception As SymbolReference

        Sub New([try] As Expression, [catch] As Expression, sourceMap As StackFrame)
            Me.try = [try]
            Me.catch = [catch]
            Me.sourceMap = sourceMap

            If TypeOf [try] Is DeclareLambdaFunction Then
                With DirectCast([try], DeclareLambdaFunction)
                    Me.exception = New SymbolReference(.parameterNames(Scan0))
                    Me.try = .closure
                End With
            End If
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim tryVal As Object = [try].Evaluate(envir)

            If Program.isException(tryVal) Then
                If [catch] Is Nothing Then
                    ' returns a try-error
                    Return New TryError(tryVal, sourceMap)
                Else
                    Using closureEnv As New Environment(envir, sourceMap, isInherits:=False)
                        If Not exception Is Nothing Then
                            Call closureEnv.Push(
                                name:=exception.symbol,
                                value:=New TryError(tryVal, sourceMap),
                                [readonly]:=False
                            )
                        End If

                        Return [catch].Evaluate(closureEnv)
                    End Using
                End If
            Else
                ' no error
                Return tryVal
            End If
        End Function
    End Class
End Namespace
