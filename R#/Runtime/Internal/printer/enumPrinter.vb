Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.ConsolePrinter

    Public Module enumPrinter

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="obj">
        ''' This object should be an enum value.
        ''' </param>
        ''' <returns></returns>
        Public Function printClass(obj As Object) As String
            Dim type As REnum = REnum.GetEnumList(obj.GetType)
            Dim base As Type = type.baseType
            Dim describ$ = DirectCast(obj, [Enum]).Description
            Dim print$ = $"{{{base.Name.ToLower} {type.IntValue(obj)}}} {obj.ToString}"

            If describ = obj.ToString Then
                Return print
            Else
                Return $"{print} #{describ}"
            End If
        End Function

        Public Function defaultValueToString([default] As Object, type As Type) As String
            Dim s As String = [default].ToString

            If s.IsPattern("\d+") Then
                ' is flag combinations
                s = GetAllEnumFlags([default], type) _
                    .Select(Function(flag) flag.ToString) _
                    .JoinBy("|")
            End If

            Return $"[{s}]"
        End Function
    End Module
End Namespace