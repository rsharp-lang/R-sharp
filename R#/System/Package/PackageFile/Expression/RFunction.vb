#Region "Microsoft.VisualBasic::fcfc0a325e0556f6adc70a63b1124d8e, R#\System\Package\PackageFile\Expression\RFunction.vb"

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

    '     Class RFunction
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
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace System.Package.File.Expressions

    Public Class RFunction : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, DeclareNewFunction))
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As DeclareNewFunction)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.FunctionDeclare))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(Writer.GetBuffer(sourceMap:=x.stackFrame))
                Call outfile.Write(Encoding.ASCII.GetBytes(x.funcName))
                Call outfile.Write(CByte(0))

                Call outfile.Write(CByte(x.params.Length))

                For Each arg As DeclareNewSymbol In x.params
                    Call outfile.Write(context.GetBuffer(arg))
                Next

                Call outfile.Write(x.body.bodySize)

                For Each exec As Expression In x.body.EnumerateCodeLines
                    Call outfile.Write(context.GetBuffer(exec))
                Next

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, desc As DESCRIPTION) As Expression
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
