#Region "Microsoft.VisualBasic::afbc844713fad9b78cd30a3c45c14c28, R#\System\Package\PackageFile\Expression\RCallFunction.vb"

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

    '   Total Lines: 187
    '    Code Lines: 156 (83.42%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 31 (16.58%)
    '     File Size: 8.82 KB


    '     Class RCallFunction
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetExpression, getFunctionName, getParameters, getTypeCode, parseByRef
    '                   parseIif, parseInvoke, parseNew, parseSeq
    ' 
    '         Sub: WriteBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Development.Package.File.Expressions

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
            ElseIf TypeOf x Is CreateObject Then
                Return DirectCast(x, CreateObject).constructor
            ElseIf TypeOf x Is SequenceLiteral Then
                Dim seq As SequenceLiteral = x

                Return {seq.from, seq.to, seq.steps}
            Else
                Return DirectCast(x, ByRefFunctionCall).GetUnionParameters.ToArray
            End If
        End Function

        Private Shared Function getFunctionName(x As Expression) As Expression
            If TypeOf x Is FunctionInvoke Then
                Return DirectCast(x, FunctionInvoke).funcName
            ElseIf TypeOf x Is IIfExpression Then
                Return New Literal("iif")
            ElseIf TypeOf x Is CreateObject Then
                Return New Literal(DirectCast(x, CreateObject).name)
            ElseIf TypeOf x Is SequenceLiteral Then
                Return New Literal("sequence")
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

                Call outfile.Write(context.GetBuffer(sourceMap:=CType(x, IRuntimeTrace).stackFrame))
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

        Private Shared Function parseSeq(bin As BinaryReader, desc As DESCRIPTION) As SequenceLiteral
            Dim sourceMap As StackFrame = Writer.ReadSourceMap(bin, desc)
            Dim ns$ = StreamHelper.ReadZEROBlock(bin).DoCall(Function(bytes) Encoding.ASCII.GetString(bytes.ToArray))
            Dim name As Literal = BlockReader.ParseBlock(bin).Parse(desc)
            Dim paramSize As Integer = bin.ReadByte
            Dim args As New List(Of Expression)

            For i As Integer = 0 To paramSize - 1
                Call BlockReader.ParseBlock(bin).Parse(desc).DoCall(AddressOf args.Add)
            Next

            Return New SequenceLiteral(args(Scan0), args(1), args(2), sourceMap)
        End Function

        Private Shared Function parseNew(bin As BinaryReader, desc As DESCRIPTION) As CreateObject
            Dim sourceMap As StackFrame = Writer.ReadSourceMap(bin, desc)
            Dim ns$ = bin.ReadZEROBlock().DoCall(Function(bytes) Encoding.ASCII.GetString(bytes.ToArray))
            Dim name As Literal = BlockReader.ParseBlock(bin).Parse(desc)
            Dim paramSize As Integer = bin.ReadByte
            Dim args As New List(Of Expression)

            For i As Integer = 0 To paramSize - 1
                Call BlockReader.ParseBlock(bin).Parse(desc).DoCall(AddressOf args.Add)
            Next

            Return New CreateObject(name.value, args.ToArray, sourceMap)
        End Function

        Private Shared Function parseIif(bin As BinaryReader, desc As DESCRIPTION) As IIfExpression
            Dim sourceMap As StackFrame = Writer.ReadSourceMap(bin, desc)
            Dim ns$ = bin.ReadZEROBlock().DoCall(Function(bytes) Encoding.ASCII.GetString(bytes.ToArray))
            Dim func As Expression = BlockReader.ParseBlock(bin).Parse(desc)
            Dim paramSize As Integer = bin.ReadByte
            Dim args As New List(Of Expression)

            For i As Integer = 0 To paramSize - 1
                Call BlockReader.ParseBlock(bin).Parse(desc).DoCall(AddressOf args.Add)
            Next

            Return New IIfExpression(args(0), args(1), args(2), sourceMap)
        End Function

        Private Shared Function parseByRef(bin As BinaryReader, desc As DESCRIPTION) As ByRefFunctionCall
            Dim sourceMap As StackFrame = Writer.ReadSourceMap(bin, desc)
            Dim ns$ = bin.ReadZEROBlock().DoCall(Function(bytes) Encoding.ASCII.GetString(bytes.ToArray))
            Dim func As Expression = BlockReader.ParseBlock(bin).Parse(desc)
            Dim paramSize As Integer = bin.ReadByte
            Dim args As New List(Of Expression)

            For i As Integer = 0 To paramSize - 1
                Call BlockReader.ParseBlock(bin) _
                    .Parse(desc) _
                    .DoCall(AddressOf args.Add)
            Next

            Dim leftArguments = args.Take(args.Count - 1).ToArray
            Dim invoke As New FunctionInvoke(func, sourceMap, leftArguments)

            Return New ByRefFunctionCall(invoke, args.Last, sourceMap)
        End Function

        Private Shared Function getTypeCode(x As Expression) As ExpressionTypes
            If TypeOf x Is FunctionInvoke Then
                Return ExpressionTypes.FunctionCall
            ElseIf TypeOf x Is IIfExpression Then
                Return ExpressionTypes.IIf
            ElseIf TypeOf x Is ByRefFunctionCall Then
                Return ExpressionTypes.FunctionByRef
            ElseIf TypeOf x Is CreateObject Then
                Return ExpressionTypes.Constructor
            ElseIf TypeOf x Is SequenceLiteral Then
                Return ExpressionTypes.SequenceLiteral
            Else
                Throw New NotImplementedException(x.GetType.FullName)
            End If
        End Function

        Private Shared Function parseInvoke(bin As BinaryReader, desc As DESCRIPTION) As FunctionInvoke
            Dim sourceMap As StackFrame = Writer.ReadSourceMap(bin, desc)
            Dim ns$ = StreamHelper.ReadZEROBlock(bin).DoCall(Function(bytes) Encoding.ASCII.GetString(bytes.ToArray))
            Dim func As Expression = BlockReader.ParseBlock(bin).Parse(desc)
            Dim parmSize As Integer = bin.ReadByte
            Dim args As New List(Of Expression)

            For i As Integer = 0 To parmSize - 1
                Call BlockReader.ParseBlock(bin).Parse(desc).DoCall(AddressOf args.Add)
            Next

            Return New FunctionInvoke(func, sourceMap, args.ToArray) With {.[namespace] = ns}
        End Function

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Select Case raw.expression
                    Case ExpressionTypes.FunctionCall : Return parseInvoke(bin, desc)
                    Case ExpressionTypes.IIf : Return parseIif(bin, desc)
                    Case ExpressionTypes.FunctionByRef : Return parseByRef(bin, desc)
                    Case ExpressionTypes.Constructor : Return parseNew(bin, desc)
                    Case ExpressionTypes.SequenceLiteral : Return parseSeq(bin, desc)
                    Case Else
                        Throw New InvalidCastException(raw.expression.ToString)
                End Select
            End Using
        End Function
    End Class
End Namespace
