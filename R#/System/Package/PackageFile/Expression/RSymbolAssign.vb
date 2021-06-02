#Region "Microsoft.VisualBasic::d07a6c1057250f47c13cedd30848c5b3, R#\System\Package\PackageFile\Expression\RSymbolAssign.vb"

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

    '     Class RSymbolAssign
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
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Development.Package.File.Expressions

    Public Class RSymbolAssign : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, ValueAssignExpression))
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As ValueAssignExpression)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.SymbolAssign))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(CByte(If(x.isByRef, 1, 0)))
                Call outfile.Write(CByte(x.targetSymbols.Length))

                For Each symbol As Expression In x.targetSymbols
                    Call outfile.Write(context.GetBuffer(symbol))
                Next

                Call outfile.Write(context.GetBuffer(x.value))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim isByRef As Boolean = If(bin.ReadByte = 0, False, True)
                Dim symbolSize As Integer = bin.ReadByte
                Dim symbols As New List(Of Expression)

                For i As Integer = 0 To symbolSize - 1
                    Call BlockReader.ParseBlock(bin).Parse(desc).DoCall(AddressOf symbols.Add)
                Next

                Dim value As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                Dim assign As New ValueAssignExpression(symbols.ToArray, value)

                Return assign
            End Using
        End Function
    End Class
End Namespace
