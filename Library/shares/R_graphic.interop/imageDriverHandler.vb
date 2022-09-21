#Region "Microsoft.VisualBasic::90e1799ef23fb245a0a762477568acdf, R-sharp\Library\R_graphic.interop\imageDriverHandler.vb"

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

'   Total Lines: 41
'    Code Lines: 33
' Comment Lines: 2
'   Blank Lines: 6
'     File Size: 1.33 KB


' Module imageDriverHandler
' 
'     Function: GetDevice, getDriver
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Module imageDriverHandler

    ReadOnly grDevices As Index(Of String) = {"grDevices", "graphics"}

    <Extension>
    Public Function getDriver(env As Environment) As Drivers
        Dim frames = env.stackTrace

        For Each stack In frames
            If stack.Method.Namespace Like grDevices Then
                Select Case Microsoft.VisualBasic.Strings.LCase(stack.Method.Method)
                    Case "wmf" : Return Drivers.WMF
                    Case "bitmap" : Return Drivers.GDI
                    Case "svg" : Return Drivers.SVG
                    Case "pdf" : Return Drivers.PDF
                    Case Else
                        ' do nothing, and then test
                        ' next frame data
                End Select
            End If
        Next

        Return Drivers.Default
    End Function

    ''' <summary>
    ''' Checking the graphics context
    ''' 
    ''' check for the argument name 'grDevices' is exists in the given
    ''' argument list or not and also the argument value should be the 
    ''' object type of <see cref="graphicsDevice"/>.
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CheckGraphicsDeviceExists(args As list) As Boolean
        Return args.hasName("grDevices") AndAlso TypeOf args("grDevices") Is graphicsDevice
    End Function

    <Extension>
    Public Function GetDevice(args As list) As IGraphics
        If args.hasName("grDevices") Then
            Return args!grDevices
        Else
            Return Nothing
        End If
    End Function
End Module
