#Region "Microsoft.VisualBasic::e961ea4028b45517b79ad0fc9e3e2f77, R#\Runtime\Internal\objects\dataset\pipeline\CLRIterator.vb"

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

    '   Total Lines: 37
    '    Code Lines: 31 (83.78%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (16.22%)
    '     File Size: 1.38 KB


    '     Class CLRIterator
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Enumerates, populates, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' subclass of <see cref="pipeline"/> model
    ''' </summary>
    Public Class CLRIterator : Inherits pipeline

        Public Sub New(input As IEnumerable, type As Type)
            MyBase.New(input, type)
        End Sub

        Public Overrides Iterator Function populates(Of T)(env As Environment) As IEnumerable(Of T)
            For Each item As T In DirectCast(pipeline, IEnumerable(Of T))
                Yield item
            Next

            If Not pipeFinalize Is Nothing Then
                Call pipeFinalize()()
            End If
        End Function

        Public Overrides Function ToString() As String
            If pipeline.GetType.IsArray Then
                Return $"clr_array[{elementType.ToString} x {DirectCast(pipeline, Array).Length}]"
            Else
                Return $"clr_iterator[{elementType.ToString}]"
            End If
        End Function

        Public Shared Function Enumerates(Of T)(x As Object, env As Environment) As IEnumerable(Of T)
            If TypeOf x Is vector Then
                Return CLRIterator.fromVector(Of T)(x, env).populates(Of T)(env)
            ElseIf TypeOf x Is T() Then
                Return DirectCast(x, T())
            Else
                Return CLRIterator.TryCreatePipeline(Of T)(x, env).populates(Of T)(env)
            End If
        End Function
    End Class
End Namespace
