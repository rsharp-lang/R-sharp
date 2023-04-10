#Region "Microsoft.VisualBasic::9be57fd2935ce41481b3831b60cd85a2, D:/GCModeller/src/R-sharp/R#//Runtime/Serialize/bufferObjects/rscriptBuffer.vb"

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

    '   Total Lines: 51
    '    Code Lines: 41
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 1.52 KB


    '     Class rscriptBuffer
    ' 
    '         Properties: code, target
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: getValue
    ' 
    '         Sub: loadBuffer, Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Runtime.Serialize

    Public Class rscriptBuffer : Inherits BufferObject

        Public Property target As Expression

        Public Overrides ReadOnly Property code As BufferObjects
            Get
                Return BufferObjects.rscript
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(data As Stream)
            Call loadBuffer(data)
        End Sub

        Public Overrides Sub Serialize(taskPayload As Stream)
            Call New Writer(taskPayload).Write(target)
            Call taskPayload.Flush()
        End Sub

        Public Overrides Function getValue() As Object
            Return target
        End Function

        Protected Overrides Sub loadBuffer(stream As Stream)
            Dim fake As New DESCRIPTION With {
                .Author = "xieguigang",
                .[Date] = Now.ToString,
                .Maintainer = .Author,
                .License = "MIT",
                .Package = NameOf(rscriptBuffer),
                .Title = .Package,
                .Type = "runtime",
                .Version = App.Version,
                .Description = .Package
            }

            Using reader As New BinaryReader(stream)
                Call BlockReader.Read(reader).Parse(fake, expr:=_target)
            End Using
        End Sub
    End Class
End Namespace
