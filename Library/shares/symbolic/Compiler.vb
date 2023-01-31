Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.MIME.application.xml.MathML
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components
Imports MathExp = Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl.Expression
Imports MathSymbol = Microsoft.VisualBasic.MIME.application.xml.MathML.SymbolExpression

Friend Module Compiler

    Public Function GetLambda(exp As FormulaExpression) As LambdaExpression
        Dim symbol As String = exp.var
        Dim formula As MathExpression = Compiler.BuildExpression(exp.formula)

        Return New LambdaExpression With {
            .parameters = New String() {symbol},
            .lambda = formula
        }
    End Function

    Public Function GetLambda(exp As DeclareNewFunction) As LambdaExpression
        Dim codes = exp.body.EnumerateCodeLines.ToArray

        If codes.IsNullOrEmpty Then
        ElseIf codes.Length > 1 Then

        End If

        Dim formula As MathExpression = Compiler.BuildExpression(codes(Scan0))
        Dim args As String() = exp.parameters _
            .Select(Function(x) x.names) _
            .IteratesALL _
            .Distinct _
            .ToArray

        Return New LambdaExpression With {
            .parameters = args,
            .lambda = formula
        }
    End Function

    Private Function BuildExpression(exp As Expression) As MathExpression
        If TypeOf exp Is Operators.BinaryExpression Then
            Dim rawBin = DirectCast(exp, Operators.BinaryExpression)
            Dim left = Compiler.BuildExpression(rawBin.left)
            Dim right = Compiler.BuildExpression(rawBin.right)

            Return New BinaryExpression() With {
                .[operator] = rawBin.operator,
                .applyleft = left,
                .applyright = right
            }
        ElseIf TypeOf exp Is SymbolReference Then
            Return New MathSymbol(DirectCast(exp, SymbolReference).symbol) With {.isNumericLiteral = False}
        ElseIf TypeOf exp Is Literal Then
            Return New MathSymbol(DirectCast(exp, Literal).ValueStr) With {.isNumericLiteral = True}
        ElseIf TypeOf exp Is FunctionInvoke Then
            Dim doCall As FunctionInvoke = DirectCast(exp, FunctionInvoke)
            Dim name As String = InvokeParameter.GetSymbolName(doCall.funcName)

            Return New MathFunctionExpression With {
                .name = name,
                .parameters = doCall.parameters _
                    .Select(AddressOf Compiler.BuildExpression) _
                    .ToArray
            }
        Else
            Throw New NotImplementedException(exp.GetType.FullName)
        End If
    End Function

    ''' <summary>
    ''' just allows math expression
    ''' </summary>
    ''' <param name="exp"></param>
    ''' <returns></returns>
    Public Function GetLambda(exp As DeclareLambdaFunction) As LambdaExpression
        Dim formula As MathExpression = Compiler.BuildExpression(exp.closure)

        Return New LambdaExpression With {
            .parameters = exp.parameterNames,
            .lambda = formula
        }
    End Function

    Public Function GetLambda(raw As String, ParamArray args As String()) As LambdaExpression
        Dim exp As MathExp = ExpressionEngine.Parse(DirectCast(raw, String))

        Return New LambdaExpression With {
            .parameters = args
        }
    End Function
End Module