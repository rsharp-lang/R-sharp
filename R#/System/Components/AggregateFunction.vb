#Region "Microsoft.VisualBasic::559f2673b087ce7a467c3716854e71b6, R#\System\Components\AggregateFunction.vb"

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

    '   Total Lines: 40
    '    Code Lines: 22 (55.00%)
    ' Comment Lines: 10 (25.00%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 8 (20.00%)
    '     File Size: 1.16 KB


    '     Class AggregateFunction
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: eval
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Development.Components

    ''' <summary>
    ''' create via the ``aggregate`` function, example as: 
    ''' 
    ''' ```r
    ''' let fx = aggregate(FUN = "max");
    ''' 
    ''' print(fx([1,2,3,4,5]));
    ''' # [1] 5
    ''' ```
    ''' </summary>
    Public Class AggregateFunction : Inherits RDefaultFunction

        Public ReadOnly aggregate As Func(Of IEnumerable(Of Double), Double)

        Default Public ReadOnly Property eval_vec(x As IEnumerable(Of Double)) As Double
            Get
                Return aggregate(x)
            End Get
        End Property

        Sub New(fx As Func(Of IEnumerable(Of Double), Double))
            aggregate = fx
        End Sub

        <RDefaultFunction>
        Public Function eval(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            Dim vec As Double() = CLRVector.asNumeric(x)
            Dim agg As Double = aggregate(vec)

            Return agg
        End Function

    End Class
End Namespace
