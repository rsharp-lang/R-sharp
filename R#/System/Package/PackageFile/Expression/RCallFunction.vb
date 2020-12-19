#Region "Microsoft.VisualBasic::afbe5d494cedcba02d8b9a587f430736, R#\System\Package\PackageFile\Expression\RCallFunction.vb"

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

'     Class RCallFunction
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: GetExpression
' 
'         Sub: (+2 Overloads) WriteBuffer
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace System.Package.File.Expressions

    Public Class RCallFunction : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Private Shared Function getParameters(x As Expression) As Expression()
            If TypeOf x Is FunctionInvoke Then
                Return DirectCast(x, FunctionInvoke).parameters
            ElseIf TypeOf x Is IIfExpression Then
                Dim iif As IIfExpression = x

                Return {
                    iif.ifTest,
                    iif.trueResult,
                    iif.falseResult
                }
            Else
                Return {DirectCast(x, ByRefFunctionCall).target, DirectCast(x, ByRefFunctionCall).value}
            End If
        End Function

        Private Shared Function getFunctionName(x As Expression) As Expression
            If TypeOf x Is FunctionInvoke Then
                Return DirectCast(x, FunctionInvoke).funcName
            ElseIf TypeOf x Is IIfExpression Then
                Return New Literal("iif")
            Else
                Return DirectCast(x, ByRefFunctionCall).funcRef
            End If
        End Function

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Dim namespace$ = If(TypeOf x Is FunctionInvoke, DirectCast(x, FunctionInvoke).namespace, "n/a")
            Dim funcName As Expression = getFunctionName(x)
            Dim parameters As Expression() = getParameters(x)

            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(getTypeCode(x)))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(Writer.GetBuffer(sourceMap:=CType(x, IRuntimeTrace).stackFrame))
                Call outfile.Write(Encoding.ASCII.GetBytes(namespace$ Or "n/a".AsDefault))
                Call outfile.Write(CByte(0))
                Call outfile.Write(context.GetBuffer(funcName))

                Call outfile.Write(CByte(parameters.Length))

                For Each arg As Expression In parameters
                    Call outfile.Write(context.GetBuffer(arg))
                Next

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Private Shared Function getTypeCode(x As Expression) As ExpressionTypes
            If TypeOf x Is FunctionInvoke Then
                Return ExpressionTypes.FunctionCall
            ElseIf TypeOf x Is IIfExpression Then
                Return ExpressionTypes.IIf
            ElseIf TypeOf x Is ByRefFunctionCall Then
                Return ExpressionTypes.FunctionByRef
            Else
                Throw New NotImplementedException(x.GetType.FullName)
            End If
        End Function

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
