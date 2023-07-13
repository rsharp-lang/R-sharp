#Region "Microsoft.VisualBasic::547f38485f08f707298e72ec076e91f5, G:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/Expression/RFunction.vb"

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

    '   Total Lines: 201
    '    Code Lines: 162
    ' Comment Lines: 0
    '   Blank Lines: 39
    '     File Size: 8.82 KB


    '     Class RFunction
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetExpression, getTypeCode, parseFormula, ParseFunction, parseLambda
    '                   parseUsingClosure
    ' 
    '         Sub: WriteBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Development.Package.File.Expressions

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
            ElseIf TypeOf x Is UsingClosure Then
                Return ExpressionTypes.Using
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
                    Call outfile.Write(context.GetBuffer(sourceMap:=CType(x, IRuntimeTrace).stackFrame))
                End If

                If TypeOf x Is DeclareNewFunction Then
                    params = DirectCast(x, DeclareNewFunction).parameters
                    body = DirectCast(x, DeclareNewFunction).body _
                        .EnumerateCodeLines _
                        .ToArray

                    Call outfile.Write(Encoding.ASCII.GetBytes(DirectCast(x, DeclareNewFunction).funcName))
                    Call outfile.Write(CByte(0))
                ElseIf TypeOf x Is FormulaExpression Then
                    params = {New SymbolReference(DirectCast(x, FormulaExpression).var)}
                    body = {DirectCast(x, FormulaExpression).formula}
                ElseIf TypeOf x Is UsingClosure Then
                    params = {DirectCast(x, UsingClosure).params}
                    body = DirectCast(x, UsingClosure).closure.EnumerateCodeLines.ToArray
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

        Private Shared Function parseUsingClosure(reader As BinaryReader, desc As DESCRIPTION) As UsingClosure
            Dim sourceMap As StackFrame = Writer.ReadSourceMap(reader, desc)
            Dim parmSize As Integer = reader.ReadByte
            Dim args As New List(Of Expression)

            For i As Integer = 0 To parmSize - 1
                Call BlockReader.ParseBlock(reader).Parse(desc).DoCall(AddressOf args.Add)
            Next

            Dim bodySize As Integer = reader.ReadInt32
            Dim body As New List(Of Expression)

            For i As Integer = 0 To bodySize - 1
                Call BlockReader.ParseBlock(reader).Parse(desc).DoCall(AddressOf body.Add)
            Next

            Return New UsingClosure(args(Scan0), New ClosureExpression(body.ToArray), sourceMap)
        End Function

        Private Shared Function parseFormula(reader As BinaryReader, desc As DESCRIPTION) As FormulaExpression
            Dim parmSize As Integer = reader.ReadByte
            Dim args As New List(Of Expression)

            For i As Integer = 0 To parmSize - 1
                Call BlockReader.ParseBlock(reader).Parse(desc).DoCall(AddressOf args.Add)
            Next

            Dim bodySize As Integer = reader.ReadInt32
            Dim body As New List(Of Expression)

            If bodySize <> 1 Then
                Throw New InvalidProgramException($"formula expression can not be more than one line!")
            End If

            For i As Integer = 0 To bodySize - 1
                Call BlockReader.ParseBlock(reader).Parse(desc).DoCall(AddressOf body.Add)
            Next

            Return New FormulaExpression(DirectCast(args(Scan0), SymbolReference).symbol, body(Scan0))
        End Function

        Private Shared Function parseLambda(reader As BinaryReader, desc As DESCRIPTION) As DeclareLambdaFunction
            Dim sourceMap As StackFrame = Writer.ReadSourceMap(reader, desc)
            Dim parmSize As Integer = reader.ReadByte
            Dim args As New List(Of Expression)

            For i As Integer = 0 To parmSize - 1
                Call BlockReader.ParseBlock(reader).Parse(desc).DoCall(AddressOf args.Add)
            Next

            Dim bodySize As Integer = reader.ReadInt32
            Dim body As New List(Of Expression)

            If bodySize <> 1 Then
                Throw New InvalidProgramException($"lambda function can not be more than one line!")
            End If

            For i As Integer = 0 To bodySize - 1
                Call BlockReader.ParseBlock(reader).Parse(desc).DoCall(AddressOf body.Add)
            Next

            Dim names As String() = args.Select(Function(a) DirectCast(a, Literal).value.ToString).ToArray
            Dim name$ = $"[{names.JoinBy(", ")}] -> {body(Scan0).ToString}"
            Dim target As New DeclareNewSymbol(names, Nothing, TypeCodes.generic, False, sourceMap)

            Return New DeclareLambdaFunction(name, target, body(Scan0), sourceMap)
        End Function

        Private Shared Function ParseFunction(reader As BinaryReader, desc As DESCRIPTION) As DeclareNewFunction
            Dim sourceMap As StackFrame = Writer.ReadSourceMap(reader, desc)
            Dim funcName As String = StreamHelper _
                .ReadZEROBlock(reader) _
                .DoCall(Function(bytes)
                            Return Encoding.ASCII.GetString(bytes.ToArray)
                        End Function)
            Dim parms As Integer = reader.ReadByte
            Dim args As New List(Of DeclareNewSymbol)
            Dim symbol As Expression

            For i As Integer = 0 To parms - 1
                symbol = BlockReader.ParseBlock(reader).Parse(desc)
                args.Add(symbol)
            Next

            Dim lines As Integer = reader.ReadInt32
            Dim body As New List(Of Expression)

            For i As Integer = 0 To lines - 1
                body.Add(BlockReader.ParseBlock(reader).Parse(desc))
            Next

            Return New DeclareNewFunction(
                funcName:=funcName,
                parameters:=args.ToArray,
                body:=New ClosureExpression(body.ToArray),
                stackframe:=sourceMap
            ) With {
                .[Namespace] = desc.Package
            }
        End Function

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using io As New BinaryReader(buffer)
                Select Case raw.expression
                    Case ExpressionTypes.FunctionDeclare : Return ParseFunction(io, desc)
                    Case ExpressionTypes.LambdaDeclare : Return parseLambda(io, desc)
                    Case ExpressionTypes.FormulaDeclare : Return parseFormula(io, desc)
                    Case ExpressionTypes.Using : Return parseUsingClosure(io, desc)
                    Case Else
                        Throw New NotImplementedException(raw.ToString)
                End Select
            End Using

            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
