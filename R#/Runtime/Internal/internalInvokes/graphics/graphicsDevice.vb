#Region "Microsoft.VisualBasic::f84da26056268a3d4d8a5ed53fc228c7, R-sharp\R#\Runtime\Internal\internalInvokes\graphics\graphicsDevice.vb"

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

    '   Total Lines: 39
    '    Code Lines: 31
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 1.02 KB


    '     Structure graphicsDevice
    ' 
    '         Properties: Background, isEmpty
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Internal.Invokes

    Public Structure graphicsDevice

        Dim g As IGraphics
        Dim file As Stream
        Dim args As list
        Dim index As Integer

        Public ReadOnly Property isEmpty As Boolean
            Get
                Return g Is Nothing
            End Get
        End Property

        Public ReadOnly Property Background As Color
            Get
                Return g.Background
            End Get
        End Property

        Default Public ReadOnly Property Item(ref As String) As Object
            Get
                Return args.slots(ref)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{g.ToString}] {args.getNames.GetJson}"
        End Function

    End Structure
End Namespace
