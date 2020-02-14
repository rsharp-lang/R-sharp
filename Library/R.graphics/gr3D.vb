
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime

<Package("grDevices.gr3D")>
Module gr3D

    <ExportAPI("vector3D")>
    Public Function vector3D(x#, y#, z#) As Point3D
        Return New Point3D With {
            .X = x,
            .Y = y,
            .Z = z
        }
    End Function

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
