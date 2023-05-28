#Region "Microsoft.VisualBasic::15fd8d30de9ef16aa78ca1fe598db319, F:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/graphics/graphicsDevice.vb"

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

    '   Total Lines: 88
    '    Code Lines: 45
    ' Comment Lines: 33
    '   Blank Lines: 10
    '     File Size: 2.97 KB


    '     Structure graphicsDevice
    ' 
    '         Properties: Background, isEmpty
    ' 
    '         Function: getArgumentValue, GetCurrentDevice, ToString
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

    ''' <summary>
    ''' the internal graphics device handle of the R# environment
    ''' </summary>
    Public Structure graphicsDevice

        ''' <summary>
        ''' the graphics device for plot rendering
        ''' </summary>
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
                Return args.slots(ref)
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
End Namespace
