#Region "Microsoft.VisualBasic::28e95691d9f271275ab6c38975670f1f, R#\Runtime\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: MeasureArrayElementType, MeasureRealElementType, TryCatch
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime

    <HideModuleName> Public Module Extensions

        ''' <summary>
        ''' If exception happens, then this function will catch 
        ''' the exceptin object and then returns the error.
        ''' </summary>
        ''' <param name="runScript"></param>
        ''' <returns></returns>
        Public Function TryCatch(runScript As Func(Of Object)) As Object
            Try
                Return runScript()
            Catch ex As Exception
                Return ex
            End Try
        End Function

        ''' <summary>
        ''' 这个函数只会尝试第一个不为空的元素的类型
        ''' </summary>
        ''' <param name="array"></param>
        ''' <returns></returns>
        Public Function MeasureArrayElementType(array As Array) As Type
            Dim x As Object

            For i As Integer = 0 To array.Length - 1
                x = array.GetValue(i)

                If Not x Is Nothing Then
                    Return x.GetType
                End If
            Next

            Return GetType(Void)
        End Function

        Public Function MeasureRealElementType(array As Array) As Type
            Dim arrayType As Type = array.GetType
            Dim x As Object
            Dim types As New List(Of Type)

            If arrayType.HasElementType AndAlso Not arrayType.GetElementType Is GetType(Object) Then
                Return arrayType.GetElementType
            End If

            For i As Integer = 0 To array.Length - 1
                x = array.GetValue(i)

                If Not x Is Nothing Then
                    types.Add(x.GetType)
                End If
            Next

            If types.Count = 0 Then
                Return GetType(Void)
            Else
                Dim tg = types _
                    .GroupBy(Function(t) t.FullName) _
                    .OrderByDescending(Function(k) k.Count) _
                    .ToArray

                If tg(Scan0).Count < array.Length / 2 Then
                    Return GetType(Object)
                Else
                    Return tg(Scan0).First
                End If
            End If
        End Function
    End Module
End Namespace
