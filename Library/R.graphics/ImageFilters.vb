
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
