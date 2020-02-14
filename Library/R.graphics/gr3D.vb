
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime

<Package("grDevices.gr3D")>
Module gr3D

    ''' <summary>
    ''' Create a new 3D point
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <param name="z"></param>
    ''' <returns></returns>
    <ExportAPI("vector3D")>
    Public Function vector3D(x#, y#, Optional z# = 0) As Point3D
        Return New Point3D With {
            .X = x,
            .Y = y,
            .Z = z
        }
    End Function

    ''' <summary>
    ''' Create a new 3D camera object.
    ''' </summary>
    ''' <param name="viewAngle"></param>
    ''' <param name="viewDistance"></param>
    ''' <param name="fov"></param>
    ''' <param name="size"></param>
    ''' <returns></returns>
    <ExportAPI("camera")>
    Public Function camera(Optional viewAngle As Object = Nothing,
                           Optional viewDistance# = -40,
                           Optional fov# = 256,
                           Optional size As Object = "2560,1440") As Camera

        Return New Camera(InteropArgumentHelper.getVector3D(viewAngle)) With {
            .ViewDistance = viewDistance,
            .fov = fov,
            .screen = InteropArgumentHelper.getSize(size).SizeParser
        }
    End Function
End Module
