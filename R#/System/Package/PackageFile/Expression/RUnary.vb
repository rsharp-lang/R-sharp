#Region "Microsoft.VisualBasic::1973b4184bf441ed3076e14b0c61c7ff, R-sharp\R#\System\Package\PackageFile\Expression\RUnary.vb"

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

    '   Total Lines: 72
    '    Code Lines: 59
    ' Comment Lines: 0
    '   Blank Lines: 13
    '     File Size: 2.85 KB


    '     Class RUnary
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetExpression
    ' 
    '         Sub: (+3 Overloads) WriteBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Development.Package.File.Expressions

    Public Class RUnary : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            If TypeOf x Is UnaryNot Then
                Call WriteBuffer(ms, DirectCast(x, UnaryNot))
            ElseIf TypeOf x Is UnaryNumeric Then
                Call WriteBuffer(ms, DirectCast(x, UnaryNumeric))
            Else
                Throw New NotImplementedException
            End If
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As UnaryNumeric)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.UnaryNumeric))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(Encoding.ASCII.GetBytes(x.operator))
                Call outfile.Write(CByte(0))
                Call outfile.Write(context.GetBuffer(x.numeric))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As UnaryNot)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.UnaryNot))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(context.GetBuffer(x.logical))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim unary As Expression

                Select Case raw.expression
                    Case ExpressionTypes.UnaryNot
                        Dim logical As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                        unary = New UnaryNot(logical)
                    Case ExpressionTypes.UnaryNumeric
                        Dim op As String = Encoding.ASCII.GetString(Writer.readZEROBlock(bin).ToArray)
                        Dim num As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                        unary = New UnaryNumeric(op, num)
                    Case Else
                        Throw New NotImplementedException
                End Select

                Return unary
            End Using
        End Function
    End Class
End Namespace
