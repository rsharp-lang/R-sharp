#Region "Microsoft.VisualBasic::2dca05321d45fc33e874747c7a76e99d, R-sharp\Library\Rlapack\RMatrix.vb"

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

    '   Total Lines: 45
    '    Code Lines: 28
    ' Comment Lines: 10
    '   Blank Lines: 7
    '     File Size: 1.39 KB


    ' Module RMatrix
    ' 
    '     Function: Matrix
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("Matrix")>
Module RMatrix

    ''' <summary>
    ''' ## 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="nrow"></param>
    ''' <param name="ncol"></param>
    ''' <param name="byrow"></param>
    ''' <param name="dimnames"></param>
    ''' <param name="sparse"></param>
    ''' <returns></returns>
    <ExportAPI("Matrix")>
    Public Function Matrix(<RRawVectorArgument>
                           Optional data As Object = Nothing,
                           Optional nrow As Integer = 1,
                           Optional ncol As Integer = 1,
                           Optional byrow As Boolean = False,
                           Optional dimnames As String() = Nothing,
                           Optional sparse As Boolean = False) As GeneralMatrix

        If TypeOf data Is vector Then
            data = DirectCast(data, vector).data
        End If

        If TypeOf data Is Double() Then
            If byrow Then
            Else

            End If
        End If

        If sparse Then
        Else
            Throw New NotImplementedException
        End If

        Throw New NotImplementedException
    End Function
End Module
