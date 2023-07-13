#Region "Microsoft.VisualBasic::527e619b610615bca81f281399b5ac19, G:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/ExpressionSymbols/Turing/Closure/TryCatchExpression.vb"

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

    '   Total Lines: 84
    '    Code Lines: 69
    ' Comment Lines: 3
    '   Blank Lines: 12
    '     File Size: 3.13 KB


    '     Class TryCatchExpression
    ' 
    '         Properties: expressionName, type
    ' 
    '         Constructor: (+2 Overloads) Sub New
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

        Friend ReadOnly sourceMap As StackFrame
        Friend ReadOnly [try] As Expression
        Friend ReadOnly [catch] As Expression
        Friend ReadOnly exception As SymbolReference

        Sub New([try] As Expression, [catch] As Expression, sourceMap As StackFrame)
            Me.try = [try]
            Me.catch = [catch]
            Me.sourceMap = sourceMap

            If TypeOf [try] Is DeclareLambdaFunction Then
                With DirectCast([try], DeclareLambdaFunction)
                    Me.exception = New SymbolReference(.parameterNames(Scan0))
                    Me.try = .closure
                End With
            Else
                Me.exception = New SymbolReference("ex")
                Me.catch = New ClosureExpression({})
            End If
        End Sub

        Friend Sub New([try] As Expression, [catch] As Expression, exception As SymbolReference, sourceMap As StackFrame)
            Me.try = [try]
            Me.catch = [catch]
            Me.exception = exception
            Me.sourceMap = sourceMap
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim tryVal As Object

            Try
                tryVal = [try].Evaluate(envir)
            Catch ex As Exception
                ' handle .NET internal error
                tryVal = Internal.debug.stop(ex, envir, suppress:=True)
            End Try

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
