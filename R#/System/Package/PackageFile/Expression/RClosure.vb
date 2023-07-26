#Region "Microsoft.VisualBasic::41a210a96d64f1185219e688938acd17, D:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/Expression/RClosure.vb"

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

    '   Total Lines: 61
    '    Code Lines: 46
    ' Comment Lines: 3
    '   Blank Lines: 12
    '     File Size: 2.38 KB


    '     Class RClosure
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
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace Development.Package.File.Expressions

    Public Class RClosure : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Using outfile As New BinaryWriter(ms)
                ' 20210618
                ' the data structure of the acceptor closure and the normal closure epxression
                ' is identical

                If TypeOf x Is AcceptorClosure Then
                    Call WriteBuffer(outfile, DirectCast(x, AcceptorClosure))
                Else
                    Call WriteBuffer(outfile, DirectCast(x, ClosureExpression))
                End If
            End Using
        End Sub

        Private Overloads Sub WriteBuffer(outfile As BinaryWriter, x As ClosureExpression)
            Call outfile.Write(CInt(x.expressionName))
            Call outfile.Write(0)
            Call outfile.Write(CByte(x.type))

            Call outfile.Write(x.bodySize)

            For Each line As Expression In x.EnumerateCodeLines
                Call outfile.Write(context.GetBuffer(line))
            Next

            Call outfile.Flush()
            Call saveSize(outfile)
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim numOfLines As Integer = bin.ReadInt32
                Dim lines As Expression() = New Expression(numOfLines - 1) {}

                For i As Integer = 0 To numOfLines - 1
                    lines(i) = BlockReader.ParseBlock(bin).Parse(desc)
                Next

                If raw.expression = ExpressionTypes.ClosureDeclare Then
                    Return New ClosureExpression(lines)
                ElseIf raw.expression = ExpressionTypes.AcceptorDeclare Then
                    Return New AcceptorClosure(lines)
                Else
                    Throw New InvalidCastException(raw.expression.Description)
                End If
            End Using
        End Function
    End Class
End Namespace
