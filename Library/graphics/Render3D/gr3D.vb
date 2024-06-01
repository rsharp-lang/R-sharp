#Region "Microsoft.VisualBasic::4a40cb68d919a9f50c488eaf41d74c44, Library\graphics\Render3D\gr3D.vb"

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

    '   Total Lines: 69
    '    Code Lines: 47 (68.12%)
    ' Comment Lines: 15 (21.74%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (10.14%)
    '     File Size: 2.57 KB


    ' Module gr3D
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: camera, line3D, vector3D
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime.Internal

<Package("grDevices3D", Category:=APICategories.UtilityTools)>
Module gr3D

    Sub New()
        REnv.ConsolePrinter.AttachConsoleFormatter(Of Camera)(Function(c) c.ToString)
    End Sub

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

    <ExportAPI("line3D")>
    Public Function line3D(<RRawVectorArgument> a As Object, <RRawVectorArgument> b As Object, Optional pen As Object = Stroke.AxisStroke) As Line3D
        Dim stroke_css As String = InteropArgumentHelper.getStrokePenCSS(pen)
        Dim env As CSSEnvirnment = CSSEnvirnment.Empty()

        Return New Line3D With {
            .a = InteropArgumentHelper.getVector3D(a),
            .b = InteropArgumentHelper.getVector3D(b),
            .pen = env.GetPen(Stroke.TryParse(stroke_css))
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
    Public Function camera(<RRawVectorArgument>
                           Optional viewAngle As Object = Nothing,
                           Optional viewDistance# = -40,
                           Optional fov# = 256,
                           <RRawVectorArgument>
                           Optional size As Object = "2560,1440",
                           Optional env As Environment = Nothing) As Camera

        Return New Camera(InteropArgumentHelper.getVector3D(viewAngle)) With {
            .viewDistance = viewDistance,
            .fov = fov,
            .screen = InteropArgumentHelper.getSize(size, env).SizeParser
        }
    End Function
End Module
