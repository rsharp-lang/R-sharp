#Region "Microsoft.VisualBasic::2d7081abc587d951342bdd1c51076cd3, R#\System\Package\PackageFile\Expression\RJSON.vb"

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

    '   Total Lines: 60
    '    Code Lines: 45 (75.00%)
    ' Comment Lines: 3 (5.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (20.00%)
    '     File Size: 2.34 KB


    '     Class RJSON
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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Development.Package.File.Expressions

    ''' <summary>
    ''' <see cref="JSONLiteral"/>
    ''' </summary>
    Public Class RJSON : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Dim json As JSONLiteral = x

            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.JSONLiteral))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(json.size)

                For Each member As NamedValue(Of Expression) In json.members
                    Call outfile.Write(Encoding.UTF8.GetBytes(member.Name))
                    Call outfile.Write(CByte(0))
                    Call outfile.Write(context.GetBuffer(member.Value))
                Next

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim nsize As Integer = bin.ReadInt32
                Dim members As New List(Of NamedValue(Of Expression))

                For i As Integer = 0 To nsize - 1
                    Dim name As String = StreamHelper.ReadZEROBlock(bin) _
                        .DoCall(Function(bytes)
                                    Return Encoding.UTF8.GetString(bytes.ToArray)
                                End Function)
                    Dim expr As Expression = BlockReader.ParseBlock(bin).Parse(desc)

                    Call members.Add(New NamedValue(Of Expression)(name, expr))
                Next

                Return New JSONLiteral(members)
            End Using
        End Function
    End Class
End Namespace
