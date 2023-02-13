#Region "Microsoft.VisualBasic::fbb76e26937aa9225053c560c7b05c9f, R-sharp\R#\System\Package\PackageFile\Expression\RCommandLine.vb"

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

    '   Total Lines: 40
    '    Code Lines: 31
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.48 KB


    '     Class RCommandLine
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
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols

Namespace Development.Package.File.Expressions

    Public Class RCommandLine : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As IO.MemoryStream, x As Expression)
            Dim shell As ExternalCommandLine = DirectCast(x, ExternalCommandLine)

            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.Shell))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(shell.ioRedirect)
                Call outfile.Write(shell.silent)
                Call outfile.Write(context.GetBuffer(shell.cli))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As IO.MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim flag1 As Boolean = bin.ReadBoolean
                Dim flag2 As Boolean = bin.ReadBoolean
                Dim literal As Expression = BlockReader.ParseBlock(bin).Parse(desc)

                Return New ExternalCommandLine(literal) With {
                    .ioRedirect = flag1,
                    .silent = flag2
                }
            End Using
        End Function
    End Class
End Namespace
