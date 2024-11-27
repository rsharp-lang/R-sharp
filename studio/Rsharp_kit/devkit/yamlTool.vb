#Region "Microsoft.VisualBasic::966445a215a70ecfee679a20cb5761f5, studio\Rsharp_kit\devkit\yamlTool.vb"

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

    '   Total Lines: 26
    '    Code Lines: 22 (84.62%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (15.38%)
    '     File Size: 914 B


    ' Module yamlTool
    ' 
    '     Function: parse
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MIME.text.yaml.Grammar
Imports Microsoft.VisualBasic.MIME.text.yaml.Syntax
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

<Package("yaml")>
Module yamlTool

    <ExportAPI("parse")>
    <RApiReturn(GetType(YamlStream))>
    Public Function parse(yaml As String, Optional env As Environment = Nothing) As Object
        Dim input As New TextInput(yaml)
        Dim success As Boolean = Nothing
        Dim parser As New YamlParser()
        Dim yamlStream As YamlStream = parser.ParseYamlStream(input, success)

        If success Then
            Return yamlStream
        End If

        Return RInternal.debug.stop(parser.BuildErrorMessages.ToArray, env)
    End Function
End Module
