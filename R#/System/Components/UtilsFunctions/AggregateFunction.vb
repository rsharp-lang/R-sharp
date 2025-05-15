#Region "Microsoft.VisualBasic::8fe82caddc7f707dd2aab8d03c1df7f5, R#\System\Components\UtilsFunctions\AggregateFunction.vb"

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

    '   Total Lines: 49
    '    Code Lines: 29 (59.18%)
    ' Comment Lines: 10 (20.41%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 10 (20.41%)
    '     File Size: 1.44 KB


    '     Class AggregateFunction
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: eval
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports Microsoft.VisualBasic.Scripting.Expressions

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

        Sub New(desc As Aggregates)
            Call Me.New(fx:=desc.GetAggregateFunction)
        End Sub

        Sub New(desc As String)
            Call Me.New(fx:=desc.GetAggregateFunction)
        End Sub

        <RDefaultFunction>
        Public Function eval(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            Dim vec As Double() = CLRVector.asNumeric(x)
            Dim agg As Double = aggregate(vec)

            Return agg
        End Function

    End Class
End Namespace
