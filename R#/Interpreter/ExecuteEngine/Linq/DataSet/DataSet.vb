#Region "Microsoft.VisualBasic::4810b5c673138a55d4fa42635b77699f, D:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/Linq/DataSet/DataSet.vb"

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
    '    Code Lines: 37
    ' Comment Lines: 5
    '   Blank Lines: 7
    '     File Size: 2.07 KB


    '     Class DataSet
    ' 
    '         Function: CreateDataSet, CreateRawDataSet
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports Microsoft.VisualBasic.Language

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' the linq dataset object, a helper module for 
    ''' iterates through the data sequence that produced
    ''' by the <see cref="QueryExpression"/>
    ''' </summary>
    Public MustInherit Class DataSet

        Friend MustOverride Function PopulatesData() As IEnumerable(Of Object)

        Friend Shared Function CreateDataSet(queryExpression As QueryExpression, context As ExecutableContext) As DataSet
            If queryExpression.joins > 0 Then
                Dim data As New JointDataSet(
                    symbol:=queryExpression.source.symbolName,
                    main:=CreateRawDataSet(queryExpression.GetSeqValue(context), context)
                )
                Dim err As New Value(Of ErrorDataSet)

                For Each joint As DataLeftJoin In queryExpression.joins
                    If Not err = data.Join(joint, context) Is Nothing Then
                        Return err.GetValueOrDefault
                    End If
                Next

                Return data
            Else
                Return CreateRawDataSet(queryExpression.GetSeqValue(context), context)
            End If
        End Function

        Friend Shared Function CreateRawDataSet(result As Object, context As ExecutableContext) As DataSet
            If result Is Nothing Then
                Return New ErrorDataSet With {.message = Internal.debug.stop("null query sequence data!", context)}
            ElseIf TypeOf result Is Message Then
                Return New ErrorDataSet With {.message = result}
            ElseIf TypeOf result Is dataframe Then
                Return New DataFrameDataSet With {.dataframe = result}
            Else
                Return New SequenceDataSet With {.sequence = result}
            End If
        End Function
    End Class
End Namespace
