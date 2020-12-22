#Region "Microsoft.VisualBasic::2002ae563c5b8d10578b31848a10db32, R#\Runtime\Internal\internalInvokes\graphics.vb"

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

'     Module graphics
' 
'         Function: bitmap, plot
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    Module graphics

        ''' <summary>
        ''' Generic function for plotting of R objects. 
        ''' </summary>
        ''' <param name="[object]"></param>
        ''' <param name="args"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("plot")>
        Public Function plot(<RRawVectorArgument> [object] As Object,
                             <RRawVectorArgument, RListObjectArgument> args As Object,
                             Optional env As Environment = Nothing) As Object

            Dim argumentsVal As Object = base.Rlist(args, env)

            If Program.isException(argumentsVal) Then
                Return argumentsVal
            Else
                Return DirectCast(argumentsVal, list).invokeGeneric([object], env)
            End If
        End Function

        ''' <summary>
        ''' save image data as bitmap image file
        ''' </summary>
        ''' <param name="image"></param>
        ''' <param name="file"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("bitmap")>
        Public Function bitmap(image As Object, Optional file As Object = Nothing, Optional format As ImageFormats = ImageFormats.Png, Optional env As Environment = Nothing) As Object
            If image Is Nothing Then
                Return debug.stop("the source bitmap image can not be nothing!", env)
            End If

            Dim stream As Stream
            Dim is_file As Boolean = False

            If file Is Nothing Then
                stream = Console.OpenStandardOutput
            ElseIf TypeOf file Is String Then
                stream = DirectCast(file, String).Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
                is_file = True
            ElseIf TypeOf file Is Stream Then
                stream = file
            Else
                Return Message.InCompatibleType(GetType(Stream), file.GetType, env)
            End If

            If image.GetType.ImplementInterface(Of SaveGdiBitmap) Then
                Call DirectCast(image, SaveGdiBitmap).Save(stream, format.GetFormat)
            Else
                Call DirectCast(image, Image).Save(stream, format.GetFormat)
            End If

            Call stream.Flush()

            If is_file Then
                Call stream.Close()
                Call stream.Dispose()
            End If

            Return True
        End Function
    End Module
End Namespace
