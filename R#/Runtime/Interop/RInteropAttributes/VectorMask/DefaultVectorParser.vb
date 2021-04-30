Namespace Runtime.Interop

    ''' <summary>
    ''' 1. 字符串类型默认使用``|``作为分隔符
    ''' 2. 数值类型默认使用``,``作为分隔符
    ''' </summary>
    Public Structure DefaultVectorParser : Implements IVectorExpressionLiteral

        Public Function ParseVector(default$, schema As Type) As Array Implements IVectorExpressionLiteral.ParseVector
            Select Case schema
                Case GetType(String)
                    Return [default]?.Split("|"c)
                Case GetType(Double)
                    Return [default]?.Split(","c) _
                        .Select(AddressOf Val) _
                        .ToArray
                Case GetType(Integer)
                    Return [default]?.Split(","c) _
                        .Select(AddressOf Integer.Parse) _
                        .ToArray
                Case Else
                    Throw New NotImplementedException(schema.FullName)
            End Select
        End Function
    End Structure
End Namespace