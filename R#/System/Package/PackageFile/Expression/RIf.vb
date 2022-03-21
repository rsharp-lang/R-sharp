#Region "Microsoft.VisualBasic::2fd53c8cb3abf4c348bbb8c6381c8885, R-sharp\R#\System\Package\PackageFile\Expression\RIf.vb"

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

    '   Total Lines: 67
    '    Code Lines: 55
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 2.79 KB


    '     Class RIf
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
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace Development.Package.File.Expressions

    Public Class RIf : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            If TypeOf x Is IfBranch Then
                Call WriteBuffer(ms, DirectCast(x, IfBranch))
            Else
                Call WriteBuffer(ms, DirectCast(x, ElseIfBranch))
            End If
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As ElseIfBranch)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.ElseIf))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(context.GetBuffer(x.stackFrame))
                Call outfile.Write(context.GetBuffer(x.ifTest))
                Call outfile.Write(context.GetBuffer(x.trueClosure))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As IfBranch)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.If))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(context.GetBuffer(x.stackFrame))
                Call outfile.Write(context.GetBuffer(x.ifTest))
                Call outfile.Write(context.GetBuffer(x.trueClosure))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim sourceMap As StackFrame = Writer.ReadSourceMap(bin, desc)
                Dim test As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                Dim trueExpr As DeclareNewFunction = BlockReader.ParseBlock(bin).Parse(desc)

                If raw.expression = ExpressionTypes.If Then
                    Return New IfBranch(test, trueExpr, sourceMap)
                Else
                    Return New ElseIfBranch(test, trueExpr.body, stackframe:=sourceMap)
                End If
            End Using
        End Function
    End Class
End Namespace
