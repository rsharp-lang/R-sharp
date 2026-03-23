#Region "Microsoft.VisualBasic::013e71b87929c2f4ec4b7ec6fcc6bb13, R#\System\Document\CodeAnalysis\RlangFunctionCall.vb"

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

    '   Total Lines: 123
    '    Code Lines: 94 (76.42%)
    ' Comment Lines: 3 (2.44%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 26 (21.14%)
    '     File Size: 5.21 KB


    '     Class RlangFunctionCall
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetScript, ListScript, TableScript, ValueScript, VectorScript
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Unit
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Components
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]

Namespace Development.CodeAnalysis

    Public Class RlangFunctionCall

        ReadOnly func As DeclareNewFunction

        Sub New(f As DeclareNewFunction)
            func = f
        End Sub

        Public Function GetScript(args As Dictionary(Of String, Object), env As Environment) As String
            Dim caller As New StringBuilder
            Dim pass As New List(Of String)

            For Each arg As DeclareNewSymbol In func.parameters
                If args.ContainsKey(arg.getName(0)) Then
                    Dim val As Object = args(arg.getName(0))
                    Dim script As String = ValueScript(val, env)

                    Call pass.Add(script)
                ElseIf arg.value IsNot Nothing Then
                    ' use optional default value
                    Dim defaultVal As New RlangTranslator(New ClosureExpression(arg.value))

                    Call pass.Add(defaultVal.GetScript(env).Trim(";"c))
                Else
                    Throw New MissingPrimaryKeyException($"missing parameter value of '{arg.getName(0)}'")
                End If
            Next

            Call caller.AppendLine("# call")
            Call caller.AppendLine($"{func.funcName}({pass.JoinBy(", " & vbLf)});")

            Return caller.ToString
        End Function

        Private Function ValueScript(val As Object, env As Environment) As String
            If val Is Nothing Then
                Return "NULL"
            ElseIf TypeOf val Is vector OrElse val.GetType.IsArray Then
                Dim raw_vec As Array = If(TypeOf val Is vector, DirectCast(val, vector).data, DirectCast(val, Array))
                Dim vec As Array = UnsafeTryCastGenericArray(raw_vec)
                Dim script As String = VectorScript(vec, env)

                Return script
            ElseIf TypeOf val Is list Then
                Return ListScript(DirectCast(val, list).slots, env)
            ElseIf TypeOf val Is dataframe Then
                Return TableScript(DirectCast(val, dataframe), env)
            Else
                Dim literal As Literal = Literal.FromAnyValue(val)
                Dim script As String = literal.GetNativeRScript
                Return script
            End If
        End Function

        Private Function TableScript(df As dataframe, env As Environment) As String
            Dim size As Long = ProfileRecord.HeapSize(df)

            df.rownames = df.rownames.UniqueNames

            If size > 4 * ByteSize.KB Then
                ' write to temp file and pass the file path
                Dim temp As String = App.GetNextUniqueName("df_")
                Dim tempfile As String = TempFileSystem.GetAppSysTempFile(".csv", sessionID:=App.PID, prefix:="df_")
                Dim expr As Expression = Expression.Parse($"write.csv({temp}, file = ""{tempfile}"", row.names = TRUE);")

                Call env.Push(temp, df, [readonly]:=True, TypeCodes.dataframe)
                Call env.Evaluate({expr})

                Return $"read.csv(""{tempfile}"", row.names = 1, check.names = FALSE)"
            Else
                Dim rownames As String = VectorScript(df.rownames, env)
                Dim fields As New List(Of String)

                For Each field As KeyValuePair(Of String, Array) In df.columns
                    Call fields.Add($"""{field.Key}"" = {VectorScript(field.Value, env)}")
                Next

                ' construct of the script
                Return $"data.frame(row.names = {rownames}, {fields.JoinBy(", " & vbLf)}, check.names = FALSE)"
            End If
        End Function

        Private Function ListScript(list As Dictionary(Of String, Object), env As Environment) As String
            Dim slots As New List(Of String)

            For Each name As String In list.Keys
                Dim val As Object = list(name)
                Dim script As String = ValueScript(val, env)

                Call slots.Add($"""{name}"" = {script}")
            Next

            Return $"list({slots.JoinBy(", " & vbLf)})"
        End Function

        Private Function VectorScript(vec As Array, env As Environment) As String
            Dim vals As New List(Of String)

            For Each xi As Object In vec.AsObjectEnumerator
                Call vals.Add(ValueScript(xi, env))
            Next

            Return $"c({vals.JoinBy(", ")})"
        End Function
    End Class
End Namespace
