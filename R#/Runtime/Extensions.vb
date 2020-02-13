#Region "Microsoft.VisualBasic::68f702ee3e7befd95e944f80ea5974c2, R#\Runtime\Extensions.vb"

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
    '         Function: MeasureArrayElementType, TryCatch
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime

    <HideModuleName> Public Module Extensions

        Public Function TryCatch(runScript As Func(Of Object)) As Object
            Try
                Return runScript()
            Catch ex As Exception
                Return ex
            End Try
        End Function

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
    End Module
End Namespace
