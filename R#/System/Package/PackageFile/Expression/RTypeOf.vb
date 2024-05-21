#Region "Microsoft.VisualBasic::38b72604ac28cd720416e611ad6895e1, R#\System\Package\PackageFile\Expression\RTypeOf.vb"

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

    '   Total Lines: 46
    '    Code Lines: 37 (80.43%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: NaN%
    ' 
    '   Blank Lines: 9 (19.57%)
    '     File Size: 1.72 KB


    '     Class RTypeOf
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
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

Namespace Development.Package.File.Expressions

    Public Class RTypeOf : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Dim modeOf As ModeOf = x

            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.TypeOf))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(Encoding.ASCII.GetBytes(modeOf.keyword))
                Call outfile.Write(CByte(0))
                Call outfile.Write(context.GetBuffer(modeOf.target))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim key As String = StreamHelper.ReadZEROBlock(bin) _
                    .DoCall(Function(bytes)
                                Return Encoding.ASCII.GetString(bytes.ToArray)
                            End Function)
                Dim target As Expression = BlockReader _
                    .ParseBlock(bin) _
                    .Parse(desc)

                Return New ModeOf(key, target)
            End Using
        End Function
    End Class
End Namespace
