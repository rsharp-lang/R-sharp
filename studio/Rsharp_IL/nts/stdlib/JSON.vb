#Region "Microsoft.VisualBasic::9ca88a8710b21b188b26032c7b40c56c, F:/GCModeller/src/R-sharp/studio/Rsharp_IL/nts//stdlib/JSON.vb"

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

    '   Total Lines: 25
    '    Code Lines: 21
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 965 B


    '     Module JSON
    ' 
    '         Function: parse, stringify
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace jsstd

    <Package("JSON")>
    Public Module JSON

        <ExportAPI("parse")>
        Public Function parse(json As String, Optional env As Environment = Nothing) As Object
            Dim rawElement As JsonElement = New JsonParser().OpenJSON(json)
            Dim obj = rawElement.createRObj(env)
            Return obj
        End Function

        <ExportAPI("stringify")>
        Public Function stringify(<RRawVectorArgument> obj As Object, Optional env As Environment = Nothing) As Object
            Return jsonlite.toJSON(obj, env, False, False, enumToStr:=True, unixTimestamp:=True)
        End Function
    End Module
End Namespace
