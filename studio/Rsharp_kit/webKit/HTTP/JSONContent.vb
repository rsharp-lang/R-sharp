#Region "Microsoft.VisualBasic::64575f4cf74c58c67b7766be20d9ad6b, E:/GCModeller/src/R-sharp/studio/Rsharp_kit/webKit//HTTP/JSONContent.vb"

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

    '   Total Lines: 27
    '    Code Lines: 23
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 1017 B


    ' Class JSONContent
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: json_encode
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net.Http
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting

Public Class JSONContent : Inherits StringContent

    Public Sub New(obj As Object, env As Environment)
        MyBase.New(json_encode(obj, env), Encodings.UTF8WithoutBOM.CodePage, "application/json")
    End Sub

    Private Shared Function json_encode(obj As Object, env As Environment) As String
        If TypeOf obj Is String Then
            Return obj
        ElseIf TypeOf obj Is vector AndAlso
            DirectCast(obj, vector).length = 1 AndAlso
            DirectCast(obj, vector).elementType Is RType.GetRSharpType(GetType(String)) Then

            Return any.ToString(DirectCast(obj, vector).data.GetValue(Scan0))
        Else
            Return jsonlite.toJSON(obj, env)
        End If
    End Function
End Class

