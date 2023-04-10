#Region "Microsoft.VisualBasic::7b27a121dddb9da7678f1acfa7caacc5, E:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/Expression/RVector.vb"

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

    '   Total Lines: 48
    '    Code Lines: 37
    ' Comment Lines: 0
    '   Blank Lines: 11
    '     File Size: 1.75 KB


    '     Class RVector
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
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

Namespace Development.Package.File.Expressions

    Public Class RVector : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, VectorLiteral))
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As VectorLiteral)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.VectorLiteral))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(x.values.Length)

                For Each item As Expression In x.values
                    Call outfile.Write(context.GetBuffer(item))
                Next

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim elements As New List(Of Expression)
                Dim sizeOf As Integer = bin.ReadInt32

                For i As Integer = 0 To sizeOf - 1
                    Call BlockReader.ParseBlock(bin).Parse(desc).DoCall(AddressOf elements.Add)
                Next

                Return New VectorLiteral(elements)
            End Using
        End Function
    End Class
End Namespace
