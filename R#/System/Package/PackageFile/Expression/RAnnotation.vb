#Region "Microsoft.VisualBasic::4056c4d4d318aa71b17d2c8f00de2822, E:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/Expression/RAnnotation.vb"

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

    '   Total Lines: 43
    '    Code Lines: 34
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.72 KB


    '     Class RAnnotation
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
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Annotation

Namespace Development.Package.File.Expressions

    Public Class RAnnotation : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            If TypeOf x Is Profiler Then
                Dim profiler As Profiler = DirectCast(x, Profiler)

                Using outfile As New BinaryWriter(ms)
                    Call outfile.Write(CInt(ExpressionTypes.Annotation))
                    Call outfile.Write(0)
                    Call outfile.Write(CByte(x.type))

                    Call outfile.Write(context.GetBuffer(sourceMap:=profiler.stackFrame))
                    Call outfile.Write(context.GetBuffer(x:=profiler.target))

                    Call outfile.Flush()
                    Call saveSize(outfile)
                End Using
            Else
                Throw New NotImplementedException
            End If
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, raw As BlockReader, desc As DESCRIPTION) As Expression
            Using reader As New BinaryReader(buffer)
                Dim sourceMap As StackFrame = Writer.ReadSourceMap(reader, desc)
                Dim target As Expression = BlockReader.ParseBlock(reader).Parse(desc)

                Return New Profiler(target, sourceMap)
            End Using
        End Function
    End Class
End Namespace
