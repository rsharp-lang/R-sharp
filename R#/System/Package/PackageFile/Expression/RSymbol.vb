#Region "Microsoft.VisualBasic::d54c530b6be666c5602ff1cfcb05ed2a, R#\System\Package\PackageFile\Expression\RSymbol.vb"

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

    '   Total Lines: 79
    '    Code Lines: 65 (82.28%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (17.72%)
    '     File Size: 3.17 KB


    '     Class RSymbol
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
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

Namespace Development.Package.File.Expressions

    Public Class RSymbol : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, DeclareNewSymbol))
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As DeclareNewSymbol)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(DirectCast(ExpressionTypes.SymbolDeclare, Integer))
                Call outfile.Write(0)
                Call outfile.Write(DirectCast(x.type, Byte))

                Call outfile.Write(context.GetBuffer(x.stackFrame))
                Call outfile.Write(CType(If(x.is_readonly, 1, 0), Byte))
                Call outfile.Write(CType(x.symbolSize, Byte))

                For Each name As String In x.names
                    Call outfile.Write(Encoding.ASCII.GetBytes(name))
                    Call outfile.Write(CType(0, Byte))
                Next

                If Not x.value Is Nothing Then
                    Dim bytes As Byte() = context.GetBuffer(x.value)

                    Call outfile.Write(bytes.Length)
                    Call outfile.Write(bytes)
                End If

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim trace As StackFrame = Writer.ReadSourceMap(bin, desc)
                Dim [readonly] As Boolean = If(bin.ReadByte = 0, False, True)
                Dim sizeOf As Integer = bin.ReadByte
                Dim nameList As New List(Of String)
                Dim value As Expression

                For i As Integer = 0 To sizeOf - 1
                    Call bin.ReadZEROBlock() _
                        .DoCall(Function(bytes) Encoding.ASCII.GetString(bytes.ToArray)) _
                        .DoCall(AddressOf nameList.Add)
                Next

                If buffer.Position >= buffer.Length Then
                    value = Nothing
                Else
                    sizeOf = bin.ReadInt32
                    value = BlockReader.ParseBlock(bin).Parse(desc)
                End If

                Return New DeclareNewSymbol(
                    names:=nameList.ToArray,
                    value:=value,
                    type:=raw.type,
                    [readonly]:=[readonly],
                    stackFrame:=trace
                )
            End Using
        End Function
    End Class
End Namespace
