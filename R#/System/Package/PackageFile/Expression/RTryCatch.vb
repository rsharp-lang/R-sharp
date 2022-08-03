#Region "Microsoft.VisualBasic::ba28d0efeeb48eab8d9ea7bf1f62fb3e, R-sharp\R#\System\Package\PackageFile\Expression\RTryCatch.vb"

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

    '   Total Lines: 47
    '    Code Lines: 38
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 2.05 KB


    '     Class RTryCatch
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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace Development.Package.File.Expressions

    Public Class RTryCatch : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, TryCatchExpression))
        End Sub

        Private Overloads Sub WriteBuffer(ms As MemoryStream, x As TryCatchExpression)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.TryCatch))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(context.GetBuffer(sourceMap:=x.sourceMap))
                Call outfile.Write(context.GetBuffer(x.try))
                Call outfile.Write(context.GetBuffer(x.catch))
                Call outfile.Write(context.GetBuffer(x.exception))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using bin As New BinaryReader(buffer)
                Dim sourceMap As StackFrame = Writer.ReadSourceMap(bin, desc)
                Dim [try] As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                Dim [catch] As Expression = BlockReader.ParseBlock(bin).Parse(desc)
                Dim [exception] As Expression = BlockReader.ParseBlock(bin).Parse(desc)

                Return New TryCatchExpression([try], [catch], exception, sourceMap)
            End Using
        End Function
    End Class
End Namespace
