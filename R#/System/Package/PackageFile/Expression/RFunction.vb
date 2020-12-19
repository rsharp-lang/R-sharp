#Region "Microsoft.VisualBasic::fcfc0a325e0556f6adc70a63b1124d8e, R#\System\Package\PackageFile\Expression\RFunction.vb"

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

'     Class RFunction
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
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace System.Package.File.Expressions

    Public Class RFunction : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Private Shared Function getTypeCode(x As Expression) As ExpressionTypes
            If TypeOf x Is DeclareNewFunction Then
                Return ExpressionTypes.FunctionDeclare
            ElseIf TypeOf x Is DeclareLambdaFunction Then
                Return ExpressionTypes.LambdaDeclare
            ElseIf TypeOf x Is FormulaExpression Then
                Return ExpressionTypes.FormulaDeclare
            Else
                Throw New NotImplementedException(x.GetType.FullName)
            End If
        End Function

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Dim params As Expression()
            Dim body As Expression()

            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(getTypeCode(x)))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                If x.GetType.ImplementInterface(Of IRuntimeTrace) Then
                    Call outfile.Write(Writer.GetBuffer(sourceMap:=CType(x, IRuntimeTrace).stackFrame))
                End If

                If TypeOf x Is DeclareNewFunction Then
                    params = DirectCast(x, DeclareNewFunction).params
                    body = DirectCast(x, DeclareNewFunction).body _
                        .EnumerateCodeLines _
                        .ToArray

                    Call outfile.Write(Encoding.ASCII.GetBytes(DirectCast(x, DeclareNewFunction).funcName))
                    Call outfile.Write(CByte(0))
                ElseIf TypeOf x Is FormulaExpression Then
                    params = {New SymbolReference(DirectCast(x, FormulaExpression).var)}
                    body = {DirectCast(x, FormulaExpression).formula}
                Else
                    body = {DirectCast(x, DeclareLambdaFunction).closure}
                    params = DirectCast(x, DeclareLambdaFunction).parameterNames _
                        .Select(Function(a) New Literal(a)) _
                        .ToArray
                End If

                Call outfile.Write(CByte(params.Length))

                For Each arg As Expression In params
                    Call outfile.Write(context.GetBuffer(arg))
                Next

                Call outfile.Write(body.Length)

                For Each exec As Expression In body
                    Call outfile.Write(context.GetBuffer(exec))
                Next

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Private Shared Function ParseFunction(reader As BinaryReader, desc As DESCRIPTION) As DeclareNewFunction
            Dim sourceMap As StackFrame = Writer.ReadSourceMap(reader)
        End Function

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using io As New BinaryReader(buffer)
                Select Case raw.type
                    Case ExpressionTypes.FunctionDeclare : Return ParseFunction(io, desc)

                End Select
            End Using

            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
