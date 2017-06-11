''' <summary>
''' The R# types
''' </summary>
Public Enum TypeCodes As Byte

    ''' <summary>
    ''' Object type in R#
    ''' </summary>
    [generic] = 0

    ''' <summary>
    ''' Class type in R#
    ''' </summary>
    [list] = 10

    ''' <summary>
    ''' <see cref="Integer"/> vector
    ''' </summary>
    [integer] = 100
    ''' <summary>
    ''' <see cref="ULong"/> vector
    ''' </summary>
    [uinteger]
    ''' <summary>
    ''' <see cref="Double"/> numeric vector
    ''' </summary>
    [double]
    ''' <summary>
    ''' <see cref="String"/> vector
    ''' </summary>
    [string]
    ''' <summary>
    ''' <see cref="Char"/> vector
    ''' </summary>
    [char]
    ''' <summary>
    ''' <see cref="Boolean"/> vector
    ''' </summary>
    [boolean]

End Enum
