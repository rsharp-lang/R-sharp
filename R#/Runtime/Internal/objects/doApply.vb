#Region "Microsoft.VisualBasic::d3520dc67cae28bf07470959156dc142, R#\Runtime\Internal\objects\doApply.vb"

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
    '    Code Lines: 65 (73.86%)
    ' Comment Lines: 6 (6.82%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 17 (19.32%)
    '     File Size: 2.94 KB


    '     Enum margins
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module doApply
    ' 
    '         Function: apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]
Imports SMRUCC.Rsharp.Runtime.Internal.[Object].Converts
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' row or column?
    ''' </summary>
    Public Enum margins
        row = 1
        column = 2
    End Enum

    Module doApply

        Public Function apply(df As dataframe, margin As margins, FUN As RFunction, env As Environment) As Object
            Dim data As Double()()
            Dim names As String()

            If margin = margins.row Then
                data = df.forEachRow _
                    .Select(Function(v) CLRVector.asNumeric(v.value)) _
                    .ToArray
                names = df.getRowNames
            Else
                names = df.colnames
                data = names _
                    .Select(Function(c) CLRVector.asNumeric(df(c))) _
                    .ToArray

            End If

            Dim vResult As Object() = New Object(data.Length - 1) {}
            Dim args As Object() = New Object(FUN.getArguments.Count - 1) {}
            Dim offset As Integer = 0

            For Each arg In FUN.getArguments
                If offset = 0 Then
                    Continue For
                Else
                    offset += 1
                End If

                If Not arg.Value Is Nothing Then
                    args(offset) = arg.Value.Evaluate(env)
                End If
            Next

            For i As Integer = 0 To vResult.Length - 1
                args(0) = data(i)
                vResult(i) = FUN.Invoke(args, env)

                If Program.isException(vResult(i)) Then
                    Return vResult(i)
                End If
            Next

            Dim gvResult As Array = TryCastGenericArray(vResult, env)
            Dim resultType As Type = gvResult.GetType.GetElementType

            If resultType.IsArray OrElse resultType Is GetType(vector) Then
                ' create dataframe
                df = New dataframe With {.columns = New Dictionary(Of String, Array)}

                For i As Integer = 0 To names.Length - 1
                    Call df.add(names(i), gvResult.GetValue(i))
                Next

                Return df
            ElseIf DataFramework.IsPrimitive(resultType) Then
                ' create vector
                Return gvResult
            Else
                ' create list
                Dim list As New list With {.slots = New Dictionary(Of String, Object)}

                For i As Integer = 0 To vResult.Length - 1
                    Call list.add(names(i), gvResult.GetValue(i))
                Next

                Return list
            End If
        End Function
    End Module
End Namespace
