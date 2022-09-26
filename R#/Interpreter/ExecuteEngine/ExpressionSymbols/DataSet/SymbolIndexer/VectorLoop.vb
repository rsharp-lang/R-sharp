Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' create a new vector data which is introduced via 
    ''' the perl syntax:
    ''' 
    ''' ``list@name`` means ``sapply(list, x -> x$name);``
    ''' ``list@{name}`` means ``sapply(list, x -> x[[name]]);``
    ''' </summary>
    Public Class VectorLoop : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        ''' <summary>
        ''' the target member name
        ''' 
        ''' should be string literal or a symbol reference 
        ''' expression produce the variable name
        ''' </summary>
        Friend ReadOnly index As Expression
        ''' <summary>
        ''' target list symbol that will extract vector data
        ''' from it
        ''' </summary>
        Friend symbol As Expression

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.VectorLoop
            End Get
        End Property

        Sub New(symbol As Expression, index As Expression)
            Me.symbol = symbol
            Me.index = index
        End Sub

        Public Overrides Function ToString() As String
            If TypeOf index Is Literal Then
                Return $"{symbol}@{index}"
            Else
                Return $"{symbol}@{{{index}}}"
            End If
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim data As Object = symbol.Evaluate(envir)
            Dim member As Object = index.Evaluate(envir)

            If Program.isException(data) Then
                Return data
            ElseIf Program.isException(member) Then
                Return member
            End If

            Dim memberName As String = any.ToString(REnv.single(member))

            If Not TypeOf data Is list Then
                Return Message.InCompatibleType(GetType(list), data.GetType, envir)
            End If

            Dim datalist As list = DirectCast(data, list)
            Dim vec As Object() = New Object(datalist.length - 1) {}
            Dim source As Object() = datalist.data.ToArray

            For i As Integer = 0 To vec.Length - 1
                vec(i) = DirectCast(source(i), list).getByName(memberName)
            Next

            Return REnv.TryCastGenericArray(vec, env:=envir)
        End Function
    End Class
End Namespace