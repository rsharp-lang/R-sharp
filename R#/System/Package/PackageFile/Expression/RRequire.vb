Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace System.Package.File.Expressions

    ''' <summary>
    ''' imports语句在脚本模式下可以出现在任意语句块之中，但是
    ''' imports语句在R#程序包之中只允许出现在脚本的最顶层，不允许出现在其他的语句块中
    ''' require语句可以出现在程序包之中的任意语句块之中
    ''' </summary>
    Public Class RRequire : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, Require))
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As Require)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.Require))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(CByte(x.packages.Length))

                For Each pkgName As String In x.packages.Select(AddressOf ValueAssign.GetSymbol)
                    Call outfile.Write(Encoding.ASCII.GetBytes(pkgName))
                    Call outfile.Write(CByte(0))
                Next

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, desc As DESCRIPTION) As Expression
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace