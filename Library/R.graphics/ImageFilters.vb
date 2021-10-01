#Region "Microsoft.VisualBasic::79fb98802f9c8219d627369222261390, Library\R.graphics\ImageFilters.vb"

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

    ' Module ImageFilters
    ' 
    '     Function: Diffusion, Emboss, Pencil, Sharp, Soften
    '               WoodCarving
    ' 
    ' /********************************************************************************/

#End Region


Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Filters
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("filter")>
Module ImageFilters

    <ExportAPI("pencil")>
    Public Function Pencil(image As Image, Optional sensitivity As Single = 25) As Image
        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(image)
            Return bitmap.Pencil(sensitivity).GetImage(flush:=True)
        End Using
    End Function

    <ExportAPI("wood_carving")>
    Public Function WoodCarving(image As Image, Optional sensitivity As Single = 25) As Image
        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(image)
            Return bitmap.Pencil(sensitivity, woodCarving:=True).GetImage(flush:=True)
        End Using
    End Function

    <ExportAPI("emboss")>
    Public Function Emboss(image As Image,
                           <RRawVectorArgument(GetType(Integer))>
                           Optional direction As Object = "1,1",
                           Optional lighteness As Integer = 127,
                           Optional env As Environment = Nothing) As Image

        Dim dir As Size = InteropArgumentHelper _
            .getSize(direction, env, "1,1") _
            .SizeParser

        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(image)
            Return bitmap.Emboss(dir.Width, dir.Height, lighteness).GetImage(flush:=True)
        End Using
    End Function

    <ExportAPI("diffusion")>
    Public Function Diffusion(image As Image) As Image
        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(image)
            Return bitmap.Diffusion.GetImage(flush:=True)
        End Using
    End Function

    <ExportAPI("soft")>
    Public Function Soften(image As Image, Optional max As Double = 255) As Image
        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(image)
            Return bitmap.Soften(max).GetImage(flush:=True)
        End Using
    End Function

    <ExportAPI("sharp")>
    Public Function Sharp(image As Image, Optional ByVal sharpDgree As Single = 0.3, Optional max As Double = 255) As Image
        Using bitmap As BitmapBuffer = BitmapBuffer.FromImage(image)
            Return bitmap.Sharp(sharpDgree, max).GetImage(flush:=True)
        End Using
    End Function

End Module

