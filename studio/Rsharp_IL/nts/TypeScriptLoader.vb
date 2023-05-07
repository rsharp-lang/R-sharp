#Region "Microsoft.VisualBasic::965bd7e5c87cc2f3821659aaf83496a7, D:/GCModeller/src/R-sharp/studio/Rsharp_IL/nts//TypeScriptLoader.vb"

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
'     File Size: 651 B


' Class TypeScriptLoader
' 
'     Properties: SuffixName
' 
'     Function: LoadScript, ParseScript
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Development.Polyglot
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Public Class TypeScriptLoader : Inherits ScriptLoader

    Public Overrides ReadOnly Iterator Property SuffixNames As IEnumerable(Of String)
        Get
            Yield "ts"
            Yield "js"
        End Get
    End Property

    Shared Sub New()
        Call Internal.invoke.pushEnvir(GetType(jsstd.system))
    End Sub

    Public Overrides Function ParseScript(scriptfile As String, env As Environment) As [Variant](Of Message, Program)
        Dim Rscript As Rscript = Rscript.AutoHandleScript(scriptfile)
        Dim program As Program = Rscript.ParseTsScript(debug:=env.globalEnvironment.debugMode)

        Return program
    End Function

    Public Overrides Function LoadScript(scriptfile As String, env As Environment) As Object
        Dim R As RInterpreter = env.globalEnvironment.Rscript
        ' 20200213 因为source函数是创建了一个新的环境容器
        ' 所以函数无法被导入到全局环境之中
        ' 在这里imports关键词操作则是使用全局环境
        Dim script As MagicScriptSymbol = CreateMagicScriptSymbol(scriptfile, R)
        Dim stackframe As New StackFrame With {
            .File = Rscript.GetFileNameDisplay(scriptfile),
            .Line = 0,
            .Method = New Method With {
                .Method = MethodBase.GetCurrentMethod.Name,
                .[Module] = "n/a",
                .[Namespace] = "SMRUCC/nts"
            }
        }

        Call env.setStackInfo(stackframe)
        Call setup_jsEnv(env.globalEnvironment)

        If env.FindSymbol("!script") Is Nothing Then
            env.Push("!script", New vbObject(script), [readonly]:=False)
        Else
            env.FindSymbol("!script").setValue(New vbObject(script), env)
        End If

        Dim program As Program = ParseScript(scriptfile, env)

        If program Is Nothing Then
            ' there are syntax error in the external script
            ' for current imports action
            Return Internal.debug.stop("".Trim(ASCII.CR, ASCII.LF, " "c, ASCII.TAB), env)
        Else
            Return program.Execute(env)
        End If
    End Function

    Public Shared Sub setup_jsEnv(env As GlobalEnvironment)
        Call [Imports].hook_jsEnv(env, "localStorage", GetType(jsstd.isolationFs.localStorage))
        Call [Imports].hook_jsEnv(env, "sessionStorage", GetType(jsstd.isolationFs.sessionStorage))
        Call [Imports].hook_jsEnv(env, "Math", GetType(Invokes.math), GetType(jsstd.Math))
        Call [Imports].hook_jsEnv(env, "console", GetType(jsstd.console))
        Call [Imports].hook_jsEnv(env, "JSON", GetType(jsstd.JSON))
    End Sub
End Class
