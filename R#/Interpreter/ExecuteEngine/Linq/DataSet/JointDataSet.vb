#Region "Microsoft.VisualBasic::6af9515e4cdb80238df8557b288cbe61, G:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/Linq/DataSet/JointDataSet.vb"

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

    '   Total Lines: 61
    '    Code Lines: 45
    ' Comment Lines: 4
    '   Blank Lines: 12
    '     File Size: 2.30 KB


    '     Class JointDataSet
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Join, PopulatesData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.My.JavaScript
Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' data set result of left join
    ''' </summary>
    Public Class JointDataSet : Inherits DataSet

        Dim main As DataSet
        Dim mainSymbol As String

        Sub New(symbol As String, main As DataSet)
            Me.main = main
            Me.mainSymbol = symbol
        End Sub

        Public Function Join(data As DataLeftJoin, context As ExecutableContext) As ErrorDataSet
            Dim right As DataSet = DataSet.CreateRawDataSet(data.Exec(context), context)

            If TypeOf right Is ErrorDataSet Then
                Return right
            End If

            Dim mainKey As String = data.FindKeySymbol(mainSymbol)
            Dim rightKey As String = data.FindKeySymbol(data.anotherData.symbolName)
            Dim raw As Object() = main.PopulatesData.ToArray
            Dim joinSeq As JavaScriptObject() = New JavaScriptObject(raw.Length - 1) {}
            Dim rightSeq As Dictionary(Of String, JavaScriptObject) = right _
                .PopulatesData _
                .Select(Function(a)
                            Return DirectCast(a, JavaScriptObject)
                        End Function) _
                .GroupBy(Function(a) any.ToString(a(rightKey))) _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return a.First
                              End Function)
            Dim leftQuery As String

            For i As Integer = 0 To raw.Length - 1
                joinSeq(i) = DirectCast(raw(i), JavaScriptObject)
                leftQuery = any.ToString(joinSeq(i)(mainKey))

                If rightSeq.ContainsKey(leftQuery) Then
                    ' join two data
                    joinSeq(i) = JavaScriptObject.Join(joinSeq(i), rightSeq(leftQuery))
                End If
            Next

            main = New RuntimeVectorDataSet(joinSeq)

            Return Nothing
        End Function

        Friend Overrides Function PopulatesData() As IEnumerable(Of Object)
            Return main.PopulatesData
        End Function
    End Class
End Namespace
