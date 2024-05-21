#Region "Microsoft.VisualBasic::deb67e75cf4abddc84bb283c9804bf03, R#\System\Package\PackageFile\Expression\RFor.vb"

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

    '   Total Lines: 66
    '    Code Lines: 52 (78.79%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: NaN%
    ' 
    '   Blank Lines: 14 (21.21%)
    '     File Size: 2.69 KB


    '     Class RFor
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
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks

Namespace Development.Package.File.Expressions

    Public Class RFor : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, ForLoop))
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As ForLoop)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.For))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(context.GetBuffer(x.stackFrame))
                Call outfile.Write(CByte(If(x.parallel, 1, 0)))

                Call outfile.Write(x.variables.Length)

                For Each item As String In x.variables
                    Call outfile.Write(Encoding.ASCII.GetBytes(item))
                    Call outfile.Write(CByte(0))
                Next

                Call outfile.Write(context.GetBuffer(x.sequence))
                Call outfile.Write(context.GetBuffer(x.body))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim sourceMap As StackFrame = Writer.ReadSourceMap(bin, desc)
                Dim parallel As Boolean = If(bin.ReadByte = 0, False, True)
                Dim varSize As Integer = bin.ReadInt32
                Dim vars As New List(Of String)

                For i As Integer = 0 To varSize - 1
                    Call StreamHelper.ReadZEROBlock(bin) _
                        .DoCall(Function(bytes)
                                    Return Encoding.ASCII.GetString(bytes.ToArray)
                                End Function) _
                        .DoCall(AddressOf vars.Add)
                Next

                Dim seq As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                Dim [loop] As Expression = BlockReader.ParseBlock(bin).Parse(desc)

                Return New ForLoop(vars.ToArray, seq, [loop], parallel, sourceMap)
            End Using
        End Function
    End Class
End Namespace
