#Region "Microsoft.VisualBasic::b93da7c5de4e0d12747ce99cdbbb53cb, R#\Runtime\Internal\objects\pipeline.vb"

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

    '     Class pipeline
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: createVector, populates, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' The R# pipeline
    ''' </summary>
    Public Class pipeline

        ReadOnly pipeline As IEnumerable
        ReadOnly elementType As RType

        Sub New(input As IEnumerable, type As Type)
            pipeline = input
            elementType = RType.GetRSharpType(type)
        End Sub

        Public Function createVector(env As Environment) As vector
            Return New vector(elementType, pipeline, env)
        End Function

        Public Iterator Function populates(Of T)() As IEnumerable(Of T)
            For Each obj As Object In pipeline
                Yield DirectCast(obj, T)
            Next
        End Function

        Public Overrides Function ToString() As String
            Return $"pipeline[{elementType.ToString}]"
        End Function
    End Class
End Namespace
