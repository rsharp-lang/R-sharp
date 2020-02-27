#Region "Microsoft.VisualBasic::1efb12b8d8860d4874652149fb614213, Library\R.graphics\gr3D.vb"

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

    ' Module gr3D
    ' 
    '     Function: camera, vector3D
    ' 
    ' /********************************************************************************/

#End Region


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

