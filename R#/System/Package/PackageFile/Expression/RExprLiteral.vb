#Region "Microsoft.VisualBasic::c92c4a5b88280970665a2167f0517e8e, F:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/Expression/RExprLiteral.vb"

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

    '   Total Lines: 39
    '    Code Lines: 30
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.56 KB


    '     Class RExprLiteral
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetExpression
    ' 
    '         Sub: WriteBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

Namespace Development.Package.File.Expressions

    Public Class RExprLiteral : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Dim literal As ExpressionLiteral = DirectCast(x, ExpressionLiteral)

            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.ExpressionLiteral))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(context.GetBuffer(literal.stackFrame))
                Call outfile.Write(context.GetBuffer(literal.expression))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim sourceMap As StackFrame = Writer.ReadSourceMap(bin, desc)
                Dim literal As Expression = BlockReader.ParseBlock(bin).Parse(desc)

                Return New ExpressionLiteral(literal, sourceMap)
            End Using
        End Function
    End Class
End Namespace
