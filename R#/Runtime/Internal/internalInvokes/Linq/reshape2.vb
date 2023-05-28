#Region "Microsoft.VisualBasic::e43f9ab95ff60497d81256f13839ffed, F:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/Linq/reshape2.vb"

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

    '   Total Lines: 113
    '    Code Lines: 79
    ' Comment Lines: 19
    '   Blank Lines: 15
    '     File Size: 5.05 KB


    '     Module reshape2
    ' 
    '         Function: melt, melt_array, melt_dataframe, melt_list
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Invokes.LinqPipeline

    Public Module reshape2

        ''' <summary>
        ''' melt: Convert an object into a molten data frame.
        ''' 
        ''' This the generic melt function. See the following functions 
        ''' for the details about different data structures
        ''' </summary>
        ''' <param name="data">Data set to melt</param>
        ''' <param name="na_rm">Should NA values be removed from the data set? 
        ''' This will convert explicit missings to implicit missings.</param>
        ''' <param name="value_name">name of variable used to store values</param>
        ''' <param name="args">
        ''' further arguments passed To Or from other methods.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' 1. melt.data.frame for data.frames
        ''' 2. melt.array for arrays, matrices And tables
        ''' 3. melt.list for lists
        ''' </returns>
        <ExportAPI("melt")>
        Public Function melt(<RRawVectorArgument> data As Object,
                             Optional na_rm As Boolean = False,
                             Optional value_name As String = "value",
                             <RListObjectArgument>
                             Optional args As list = Nothing,
                             Optional env As Environment = Nothing) As Object

            If TypeOf data Is dataframe Then
                Return melt_dataframe(data, na_rm, value_name, args, env)
            ElseIf TypeOf data Is list Then
                Return melt_list(data, na_rm, value_name, args, env)
            Else
                Return melt_array(REnv.asVector(Of Object)(data), na_rm, value_name, args, env)
            End If
        End Function

        <ExportAPI("melt.data.frame")>
        Public Function melt_dataframe(<RRawVectorArgument> data As dataframe,
                                       Optional na_rm As Boolean = False,
                                       Optional value_name As String = "value",
                                       <RListObjectArgument>
                                       Optional args As list = Nothing,
                                       Optional env As Environment = Nothing) As Object

            Dim df As New dataframe With {.columns = New Dictionary(Of String, Array)}
            Dim Name As New List(Of String)
            Dim variable As New List(Of String)
            Dim value As New List(Of Double)
            Dim idVars As String = CLRVector.asCharacter(args.getByName("id.vars")).FirstOrDefault
            Dim vname As String()

            If idVars Is Nothing Then
                vname = data.getRowNames

                For Each var As String In data.colnames
                    Call Name.AddRange(vname)
                    Call variable.AddRange(var.Replicate(data.nrows))
                    Call value.AddRange(CLRVector.asNumeric(data(var)))
                Next

                idVars = "X"
            Else
                vname = CLRVector.asCharacter(data(idVars))

                For Each var As String In data.colnames
                    If var = idVars Then
                        Continue For
                    End If

                    Call Name.AddRange(vname)
                    Call variable.AddRange(var.Replicate(data.nrows))
                    Call value.AddRange(CLRVector.asNumeric(data(var)))
                Next
            End If

            Call df.add(idVars, Name)
            Call df.add("variable", variable)
            Call df.add(value_name, value)

            Return df
        End Function

        <ExportAPI("melt.array")>
        Public Function melt_array(<RRawVectorArgument> data As Object,
                                   Optional na_rm As Boolean = False,
                                   Optional value_name As String = "value",
                                   <RListObjectArgument>
                                   Optional args As list = Nothing,
                                   Optional env As Environment = Nothing) As Object
            Throw New NotImplementedException
        End Function

        <ExportAPI("melt.list")>
        Public Function melt_list(<RRawVectorArgument> data As list,
                                  Optional na_rm As Boolean = False,
                                  Optional value_name As String = "value",
                                  <RListObjectArgument>
                                  Optional args As list = Nothing,
                                  Optional env As Environment = Nothing) As Object
            Throw New NotImplementedException
        End Function
    End Module
End Namespace
