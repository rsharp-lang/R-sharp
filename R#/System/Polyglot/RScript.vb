#Region "Microsoft.VisualBasic::4ad1dc2c5e5bf95b5d417715ad1a73c8, R#\System\Polyglot\RScript.vb"

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

    '   Total Lines: 69
    '    Code Lines: 54 (78.26%)
    ' Comment Lines: 5 (7.25%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (14.49%)
    '     File Size: 2.84 KB


    '     Class RScriptLoader
    ' 
    '         Function: LoadScript, ParseScript
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Development.Polyglot

    Public Class RScriptLoader : Inherits ScriptLoader

        Public Overrides ReadOnly Iterator Property SuffixNames As IEnumerable(Of String)
            Get
                Yield "R"
            End Get
        End Property

        Public Overrides Function LoadScript(scriptfile As String, env As Environment) As Object
            Dim R As RInterpreter = env.globalEnvironment.Rscript
            ' 20200213 因为source函数是创建了一个新的环境容器
            ' 所以函数无法被导入到全局环境之中
            ' 在这里imports关键词操作则是使用全局环境
            Dim script As MagicScriptSymbol = CreateMagicScriptSymbol(scriptfile, R)
            Dim Rscript As Rscript = Rscript.FromFile(scriptfile)
            Dim stackframe As New StackFrame With {
                .File = Rscript.fileName,
                .Line = 0,
                .Method = New Method With {
                    .Method = MethodBase.GetCurrentMethod.Name,
                    .[Module] = "n/a",
                    .[Namespace] = "SMRUCC/R#"
                }
            }

            Call env.setStackInfo(stackframe)

            If env.FindSymbol("!script") Is Nothing Then
                env.Push("!script", New vbObject(script), [readonly]:=False)
            Else
                env.FindSymbol("!script").setValue(New vbObject(script), env)
            End If

            Dim parsed = ParseScript(scriptfile, env)

            If parsed Like GetType(Message) Then
                Return parsed.TryCast(Of Message)
            Else
                Return parsed.TryCast(Of Program).Execute(env)
            End If
        End Function

        Public Overrides Function ParseScript(scriptfile As String, env As Environment) As [Variant](Of Message, Program)
            Dim Rscript As Rscript = Rscript.FromFile(scriptfile)
            Dim R As RInterpreter = env.globalEnvironment.Rscript
            Dim error$ = Nothing
            Dim program As Program = Program.CreateProgram(Rscript, R.debug, [error]:=[error])

            If program Is Nothing Then
                ' there are syntax error in the external script
                ' for current imports action
                Return Internal.debug.stop([error].Trim(ASCII.CR, ASCII.LF, " "c, ASCII.TAB), env)
            Else
                Return program
            End If
        End Function
    End Class
End Namespace
