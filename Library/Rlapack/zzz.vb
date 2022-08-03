#Region "Microsoft.VisualBasic::122758a67ec15b1a45d675e6ed9fe35a, R-sharp\Library\Rlapack\zzz.vb"

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

    '   Total Lines: 34
    '    Code Lines: 29
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 1.18 KB


    ' Class zzz
    ' 
    '     Function: printMatrix
    ' 
    '     Sub: onLoad
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports SMRUCC.Rsharp.Runtime.Internal

Public Class zzz

    Public Shared Sub onLoad()
        Call ConsolePrinter.AttachConsoleFormatter(Of GeneralMatrix)(AddressOf printMatrix)
    End Sub

    Private Shared Function printMatrix(m As GeneralMatrix) As String
        Dim strMat As String()() = m _
            .RowVectors _
            .Select(Function(r)
                        Return r.Select(Function(d) d.ToString("F4")).ToArray
                    End Function) _
            .ToArray
        Dim text As New StringBuilder
        Dim i As New Uid(0, Uid.AlphabetUCase)
        Dim titles As String() = strMat(Scan0) _
            .Select(Function(null) ++i) _
            .ToArray

        Using writer As New StringWriter(text)
            Call PrintAsTable.PrintTable(strMat, writer, " ", title:=titles, trilinearTable:=True)
            Call writer.Flush()
        End Using

        Return text.ToString
    End Function
End Class
