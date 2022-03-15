#Region "Microsoft.VisualBasic::c50dc25fdd4dabde9390602685a37c95, R-sharp\Library\JsonHelper\jsonlite.vb"

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

    '   Total Lines: 58
    '    Code Lines: 48
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 1.88 KB


    ' Module jsonlite
    ' 
    '     Function: toJSON
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports SMRUCC.Rsharp.Development.Components
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports REnv = SMRUCC.Rsharp.Runtime

Module jsonlite

    Public Function toJSON(x As Object, env As Environment,
                           Optional maskReadonly As Boolean = False,
                           Optional indent As Boolean = False,
                           Optional enumToStr As Boolean = True,
                           Optional unixTimestamp As Boolean = True) As Object

        Dim json As JsonElement
        Dim opts As New JSONSerializerOptions With {
            .indent = indent,
            .maskReadonly = maskReadonly,
            .enumToString = enumToStr,
            .unixTimestamp = unixTimestamp
        }

        If x Is Nothing Then
            Return "null"
        End If

        If TypeOf x Is vector Then
            x = DirectCast(x, vector).data
        End If

        If x.GetType.IsArray Then
            If DirectCast(x, Array).Length = 1 Then
                x = DirectCast(x, Array).GetValue(Scan0)
            ElseIf DirectCast(x, Array).Length = 0 Then
                Return "[]"
            Else
                x = REnv.TryCastGenericArray(DirectCast(x, Array), env)
            End If
        End If

        If Program.isException(x) Then
            Return x
        End If

        If Not TypeOf x Is JsonElement Then
            x = Encoder.GetObject(x)
            json = x.GetType.GetJsonElement(x, opts)
        Else
            json = DirectCast(x, JsonElement)
        End If

        Dim jsonStr As String = json.BuildJsonString(opts)

        Return jsonStr
    End Function
End Module
