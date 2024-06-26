﻿#Region "Microsoft.VisualBasic::6e75b2a331b305195cd0e1a98895a73d, R#\System\Package\PackageFile\Serialization\BlockReader.vb"

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

    '   Total Lines: 135
    '    Code Lines: 101 (74.81%)
    ' Comment Lines: 6 (4.44%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 28 (20.74%)
    '     File Size: 6.82 KB


    '     Class BlockReader
    ' 
    '         Properties: body, expression, type
    ' 
    '         Function: CheckMagic, Parse, (+2 Overloads) ParseBlock, (+2 Overloads) Read, ToString
    ' 
    '         Sub: Parse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Development.Package.File.Expressions
Imports System.Runtime.CompilerServices

Namespace Development.Package.File

    Public Class BlockReader

        Public Property expression As ExpressionTypes
        Public Property type As TypeCodes
        Public Property body As Byte()

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"[{expression.Description}] {type.Description}  &H{body.Length.ToHexString} bytes"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Parse(desc As DESCRIPTION, ByRef expr As Expression)
            expr = Parse(desc)
        End Sub

        Public Function Parse(desc As DESCRIPTION) As Expression
            Using buffer As New MemoryStream(body)
                Select Case expression
                    Case ExpressionTypes.Binary : Return New RBinary(Nothing).GetExpression(buffer, Me, desc)
                    Case ExpressionTypes.UnaryNot, ExpressionTypes.UnaryNumeric
                        Return New RUnary(Nothing).GetExpression(buffer, Me, desc)

                    Case ExpressionTypes.Break, ExpressionTypes.Continute
                        Return New RBreakControls(Nothing).GetExpression(buffer, Me, desc)

                    Case ExpressionTypes.FunctionDeclare,
                         ExpressionTypes.FormulaDeclare,
                         ExpressionTypes.LambdaDeclare,
                         ExpressionTypes.Using

                        Return New RFunction(Nothing).GetExpression(buffer, Me, desc)

                    Case ExpressionTypes.FunctionCall,
                         ExpressionTypes.FunctionByRef,
                         ExpressionTypes.IIf,
                         ExpressionTypes.Constructor,
                         ExpressionTypes.SequenceLiteral

                        Return New RCallFunction(Nothing).GetExpression(buffer, Me, desc)

                    Case ExpressionTypes.Annotation : Return New RAnnotation(Nothing).GetExpression(buffer, Me, desc)

                    Case ExpressionTypes.If, ExpressionTypes.ElseIf : Return New RIf(Nothing).GetExpression(buffer, Me, desc)
                    Case ExpressionTypes.For : Return New RFor(Nothing).GetExpression(buffer, Me, desc)
                    Case ExpressionTypes.Else : Return New RElse(Nothing).GetExpression(buffer, Me, desc)
                    Case ExpressionTypes.ClosureDeclare, ExpressionTypes.AcceptorDeclare

                        Return New RClosure(Nothing).GetExpression(buffer, Me, desc)

                    Case ExpressionTypes.TryCatch : Return New RTryCatch(Nothing).GetExpression(buffer, Me, desc)
                    Case ExpressionTypes.While : Return New RWhileLoop(Nothing).GetExpression(buffer, Me, desc)

                    Case ExpressionTypes.StringInterpolation : Return New RStringInterpolation(Nothing).GetExpression(buffer, Me, desc)
                    Case ExpressionTypes.Literal,
                         ExpressionTypes.SymbolRegexp

                        Return New RLiteral(Nothing).GetExpression(buffer, Me, desc)
                    Case ExpressionTypes.ExpressionLiteral : Return New RExprLiteral(Nothing).GetExpression(buffer, Me, desc)
                    Case ExpressionTypes.JSONLiteral : Return New RJSON(Nothing).GetExpression(buffer, Me, desc)
                    Case ExpressionTypes.TypeOf : Return New RTypeOf(Nothing).GetExpression(buffer, Me, desc)

                    Case ExpressionTypes.VectorLiteral : Return New RVector(Nothing).GetExpression(buffer, Me, desc)
                    Case ExpressionTypes.Shell : Return New RCommandLine(Nothing).GetExpression(buffer, Me, desc)

                    Case ExpressionTypes.Require, ExpressionTypes.Imports : Return New RRequire(Nothing).GetExpression(buffer, Me, desc)
                    Case ExpressionTypes.SymbolDeclare : Return New RSymbol(Nothing).GetExpression(buffer, Me, desc)
                    Case ExpressionTypes.Return : Return New RReturns(Nothing).GetExpression(buffer, Me, desc)
                    Case ExpressionTypes.SymbolIndex, ExpressionTypes.DotNetMemberReference, ExpressionTypes.VectorLoop
                        Return New RSymbolIndex(Nothing).GetExpression(buffer, Me, desc)
                    Case ExpressionTypes.SymbolReference, ExpressionTypes.SymbolNamespaceReference
                        Return New RSymbolReference(Nothing).GetExpression(buffer, Me, desc)
                    Case ExpressionTypes.SymbolAssign, ExpressionTypes.SymbolMemberAssign
                        Return New RSymbolAssign(Nothing).GetExpression(buffer, Me, desc)

                    Case Else
                        Throw New NotImplementedException(expression.Description)
                End Select
            End Using
        End Function

        ''' <summary>
        ''' 从头开始读文件使用的函数
        ''' </summary>
        ''' <param name="reader"></param>
        ''' <returns></returns>
        Public Shared Function Read(reader As BinaryReader) As BlockReader
            If Not CheckMagic(reader) Then
                Throw New InvalidDataException("magic header is not correct!")
            End If

            Return Read(reader, Writer.Magic.Length)
        End Function

        Public Shared Function CheckMagic(reader As BinaryReader) As Boolean
            ' check magic
            Dim magic As String = Encoding.ASCII.GetString(reader.ReadBytes(Writer.Magic.Length))
            Return magic = Writer.Magic
        End Function

        Public Shared Function ParseBlock(data As Byte()) As BlockReader
            Using buffer As New MemoryStream(data), reader As New BinaryReader(buffer)
                Return ParseBlock(reader)
            End Using
        End Function

        Public Shared Function ParseBlock(reader As BinaryReader) As BlockReader
            Dim type As ExpressionTypes = reader.ReadInt32
            Dim size As Integer = reader.ReadInt32
            Dim typecode As TypeCodes = reader.ReadByte
            Dim body As Byte() = reader.ReadBytes(size)

            Return New BlockReader With {
                .body = body,
                .expression = type,
                .type = typecode
            }
        End Function

        Public Shared Function Read(reader As BinaryReader, i As Long) As BlockReader
            Call reader.BaseStream.Seek(i, SeekOrigin.Begin)
            Return ParseBlock(reader)
        End Function

    End Class
End Namespace
