Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Text
Imports njl.Language
Imports SMRUCC.Rsharp.Development.Hybrids
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Public Class JuliaScriptLoader : Inherits ScriptLoader

    Public Overrides ReadOnly Property SuffixName As String
        Get
            Return "jl"
        End Get
    End Property

    Public Overrides Function LoadScript(scriptfile As String, env As Environment) As Object
        Dim R As RInterpreter = env.globalEnvironment.Rscript
        Dim program As Program
        Dim error$ = Nothing

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
                .[Namespace] = "SMRUCC/njl"
            }
        }

        Call env.setStackInfo(stackframe)

        If env.FindSymbol("!script") Is Nothing Then
            env.Push("!script", New vbObject(script), [readonly]:=False)
        Else
            env.FindSymbol("!script").SetValue(New vbObject(script), env)
        End If

        program = Rscript.ParseJlScript(R.debug)

        If program Is Nothing Then
            ' there are syntax error in the external script
            ' for current imports action
            Return Internal.debug.stop([error].Trim(ASCII.CR, ASCII.LF, " "c, ASCII.TAB), env)
        Else
            Return program.Execute(env)
        End If
    End Function
End Class
