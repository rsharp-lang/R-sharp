#Region "Microsoft.VisualBasic::d7ea3d2a154923747006a5e64d1bdffb, F:/GCModeller/src/R-sharp/studio/Rsharp_IL/nts//stdlib/console.vb"

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

    '   Total Lines: 86
    '    Code Lines: 72
    ' Comment Lines: 0
    '   Blank Lines: 14
    '     File Size: 3.07 KB


    '     Module console
    ' 
    '         Function: df_from_list_array, log, table
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Interop.[CType]

Namespace jsstd

    <Package("console")>
    Public Module console

        <ExportAPI("log")>
        Public Function log(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing)
            If TypeOf x Is String Then
                Call base.cat(CStr(x) & vbCrLf, env:=env)
            Else
                Dim json As Object = jsonlite.toJSON(x, env)

                If Program.isException(json) Then
                    Return json
                Else
                    Call base.cat(CStr(json) & vbCrLf, env:=env)
                End If
            End If

            Return Nothing
        End Function

        <ExportAPI("table")>
        Public Function table(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing)
            Dim data = pipeline.TryCreatePipeline(Of list)(x, env)

            If data.isError Then
                If TypeOf x Is vector Then
                    x = DirectCast(x, vector).data
                End If

                If x.GetType.IsArray Then
                    Dim df As New dataframe With {
                        .columns = New Dictionary(Of String, Array) From {
                            {"x", x}
                        }
                    }

                    Return base.print(df, env:=env)
                ElseIf x.GetType.ImplementInterface(Of ICTypeDataframe) Then
                    Return base.print(DirectCast(x, ICTypeDataframe).toDataframe, env:=env)
                End If

                Return data.getError
            End If

            Return data.populates(Of list)(env) _
                .ToArray _
                .df_from_list_array(env)
        End Function

        <Extension>
        Private Function df_from_list_array(listArray As list(), env As Environment) As Object
            Dim union_names As String() = listArray _
                .Select(Function(l) l.getNames) _
                .IteratesALL _
                .Distinct _
                .ToArray
            Dim df As New dataframe With {
                .columns = New Dictionary(Of String, Array),
                .rownames = Enumerable _
                    .Range(1, listArray.Length) _
                    .Select(Function(i) i.ToString) _
                    .ToArray
            }

            For Each name As String In union_names
                Call df.add(name, listArray.Select(Function(l) l.getByName(name)))
            Next

            Return base.print(df, env:=env)
        End Function
    End Module
End Namespace
