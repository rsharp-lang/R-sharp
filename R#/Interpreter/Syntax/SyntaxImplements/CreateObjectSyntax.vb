Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module CreateObjectSyntax

        Public Function CreateNewObject(keyword As Token, tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim type As String = tokens(Scan0).text
            Dim stackFrame As StackFrame = opts.GetStackTrace(keyword, $"[{type}].cor")
            Dim args = tokens.Skip(2).Take(tokens.Length - 3).ToArray
            Dim parameters As New List(Of Expression)

            For Each a As SyntaxResult In args.getInvokeParameters(opts)
                If a.isException Then
                    Return a
                Else
                    parameters.Add(a.expression)
                End If
            Next

            Return New CreateObject(type, parameters.ToArray, stackFrame)
        End Function
    End Module
End Namespace