#Region "Microsoft.VisualBasic::587472520bc63dd17c561d406449a96b, D:/GCModeller/src/R-sharp/studio/Rsharp_IL/nts//TypeScriptLoader.vb"

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

    '   Total Lines: 22
    '    Code Lines: 16
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 650 B


    ' Class TypeScriptLoader
    ' 
    '     Properties: SuffixName
    ' 
    '     Function: LoadScript, ParseScript
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Development.Hybrids
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Public Class TypeScriptLoader : Inherits ScriptLoader

    Public Overrides ReadOnly Property SuffixName As String
        Get
            Return "ts"
        End Get
    End Property

    Public Overrides Function ParseScript(scriptfile As String, env As Environment) As [Variant](Of Message, Program)

    End Function

    Public Overrides Function LoadScript(scriptfile As String, env As Environment) As Object

    End Function
End Class
