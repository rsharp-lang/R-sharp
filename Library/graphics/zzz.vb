#Region "Microsoft.VisualBasic::e0d33867653e0a00c03cd4bbeb76e1ce, F:/GCModeller/src/R-sharp/Library/graphics//zzz.vb"

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

    '   Total Lines: 35
    '    Code Lines: 27
    ' Comment Lines: 1
    '   Blank Lines: 7
    '     File Size: 1.20 KB


    ' Class zzz
    ' 
    '     Function: loadColors
    ' 
    '     Sub: onLoad, RegisterDriver
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports PdfImage = Microsoft.VisualBasic.Imaging.PDF

Public Class zzz

    Public Shared Sub onLoad()
        Call plots.Main()
        Call geometry2D.Main()

        Call RegisterDriver()
    End Sub

    Private Shared Function loadColors(res As String) As Color()
        Dim lines As String() = res.LineTokens.Skip(2).ToArray
        Dim sample = lines.Split(8).Select(Function(t) t(0)).ToArray
        Dim colors = sample _
            .Select(Function(line)
                        Dim t As Integer() = (From tr As String
                                              In line.Split
                                              Let p As Double = Val(tr)
                                              Select CInt(255 * tr)).ToArray
                        Dim color As Color = Color.FromArgb(t(0), t(1), t(2))

                        Return color
                    End Function) _
            .ToArray

        Return colors
    End Function

    Private Shared Sub RegisterDriver()
        Call PDFimage.Driver.Init()
        ' Call Designer.Register("MPL_gist_ncar", loadColors(My.Resources.MPL_gist_ncar))
    End Sub
End Class
