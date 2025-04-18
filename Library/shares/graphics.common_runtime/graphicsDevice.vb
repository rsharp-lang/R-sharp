﻿#Region "Microsoft.VisualBasic::ef4a09c8b7c3fca5564f394a853e7b01, Library\shares\graphics.common_runtime\graphicsDevice.vb"

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

    '   Total Lines: 126
    '    Code Lines: 79 (62.70%)
    ' Comment Lines: 33 (26.19%)
    '    - Xml Docs: 96.97%
    ' 
    '   Blank Lines: 14 (11.11%)
    '     File Size: 4.36 KB


    ' Structure graphicsDevice
    ' 
    '     Properties: Background, isEmpty, TryMeasureFormatEncoder
    ' 
    '     Function: getArgumentValue, GetCurrentDevice, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language.[Default]
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Internal.Object

''' <summary>
''' the internal graphics device handle of the R# environment
''' </summary>
Public Structure graphicsDevice : Implements IsEmpty

    ''' <summary>
    ''' the graphics device for plot rendering
    ''' </summary>
    Dim g As IGraphics
    Dim file As Stream
    Dim args As list
    Dim index As Integer
    Dim leaveOpen As Boolean
    Dim dev As String

    Public ReadOnly Property isEmpty As Boolean Implements IsEmpty.IsEmpty
        Get
            Return g Is Nothing
        End Get
    End Property

    Public ReadOnly Property Background As Color
        Get
            Return g.Background
        End Get
    End Property

    Public ReadOnly Property TryMeasureFormatEncoder As ImageFormats
        Get
            Dim driver As Drivers = g.Driver

            If driver = Drivers.Default Then
                driver = DriverLoad.DefaultGraphicsDevice
            End If

            Select Case g.Driver
                Case Drivers.WMF : Return ImageFormats.Wmf
                Case Drivers.PDF : Return ImageFormats.Pdf
                Case Drivers.PostScript : Return ImageFormats.Unknown
                Case Drivers.SVG : Return ImageFormats.Svg
                Case Drivers.GDI

                    Select Case dev
                        Case "bitmap" : Return ImageFormats.Bmp
                        Case "png" : Return ImageFormats.Png
                        Case "jpeg" : Return ImageFormats.Jpeg
                        Case "win.metafile" : Return ImageFormats.Wmf
                        Case "postscript" : Throw New NotImplementedException("postscript")
                        Case "tiff" : Return ImageFormats.Tiff
                        Case Else
                            Throw New NotImplementedException
                    End Select
            End Select

            Throw New NotImplementedException
        End Get
    End Property

    ''' <summary>
    ''' get argument value by name
    ''' </summary>
    ''' <param name="ref"></param>
    ''' <returns>
    ''' this method get value directly from the <see cref="args"/> data 
    ''' list, an exception will be throw if the argument name key is 
    ''' missing.
    ''' 
    ''' for safely get the argument value from this object, use the 
    ''' <see cref="getArgumentValue(String, list)"/> method.
    ''' </returns>
    Default Public ReadOnly Property Item(ref As String) As Object
        Get
            If args Is Nothing OrElse args.slots Is Nothing Then
                Return Nothing
            ElseIf ref Is Nothing OrElse Not args.slots.ContainsKey(ref) Then
                Return Nothing
            Else
                Return args.slots(ref)
            End If
        End Get
    End Property

    ''' <summary>
    ''' try <see cref="args"/> at first, then this function will 
    ''' try to find the argument value in <paramref name="args2"/> 
    ''' if the argument name is missing from the <see cref="args"/>
    ''' collection.
    ''' </summary>
    ''' <param name="ref">the argument name to find</param>
    ''' <param name="args2">
    ''' this argument value can be nothing
    ''' </param>
    ''' <returns>
    ''' this function is a safe function, value nothing will 
    ''' be returns if all argument pack is missing the required
    ''' argument name key.
    ''' </returns>
    Public Function getArgumentValue(ref As String, args2 As list) As Object
        If args.slots.ContainsKey(ref) Then
            Return args.slots(ref)
        ElseIf args2 Is Nothing Then
            Return Nothing
        ElseIf Not args2.slots.ContainsKey(ref) Then
            Return Nothing
        Else
            Return args2.slots(ref)
        End If
    End Function

    Public Shared Function GetCurrentDevice() As graphicsDevice
        Return graphics.curDev
    End Function

    Public Overrides Function ToString() As String
        Return $"[{g.ToString}] {args.getNames.GetJson}"
    End Function

End Structure
