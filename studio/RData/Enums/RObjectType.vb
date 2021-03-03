
''' <summary>
''' Type of a R object.
''' </summary>
Public Enum RObjectType
    ''' <summary>
    ''' NULL
    ''' </summary>
    NIL = 0
    ''' <summary>
    ''' symbols
    ''' </summary>
    SYM = 1
    ''' <summary>
    ''' pairlists
    ''' </summary>
    LIST = 2
    ''' <summary>
    ''' closures
    ''' </summary>
    CLO = 3
    ''' <summary>
    ''' environments
    ''' </summary>
    ENV = 4
    ''' <summary>
    ''' promises
    ''' </summary>
    PROM = 5
    ''' <summary>
    ''' language objects
    ''' </summary>
    LANG = 6
    ''' <summary>
    ''' special functions
    ''' </summary>
    SPECIAL = 7
    ''' <summary>
    ''' builtin functions
    ''' </summary>
    BUILTIN = 8
    ''' <summary>
    ''' internal character strings
    ''' </summary>
    [Char] = 9
    ''' <summary>
    ''' logical vectors
    ''' </summary>
    LGL = 10
    ''' <summary>
    ''' Integer vectors
    ''' </summary>
    INT = 13
    ''' <summary>
    ''' numeric vectors
    ''' </summary>
    REAL = 14
    ''' <summary>
    ''' complex vectors
    ''' </summary>
    CPLX = 15
    ''' <summary>
    ''' character vectors
    ''' </summary>
    STR = 16
    ''' <summary>
    ''' dot-dot-dot Object
    ''' </summary>
    DOT = 17
    ''' <summary>
    ''' make “any” args work
    ''' </summary>
    ANY = 18
    ''' <summary>
    ''' list (generic vector)
    ''' </summary>
    VEC = 19
    ''' <summary>
    ''' expression vector
    ''' </summary>
    EXPR = 20
    ''' <summary>
    ''' Byte code
    ''' </summary>
    BCODE = 21
    ''' <summary>
    ''' external pointer
    ''' </summary>
    EXTPTR = 22
    ''' <summary>
    ''' weak reference
    ''' </summary>
    WEAKREF = 23
    ''' <summary>
    ''' raw vector
    ''' </summary>
    RAW = 24
    ''' <summary>
    ''' S4 classes Not Of simple type
    ''' </summary>
    S4 = 25
    ''' <summary>
    ''' Empty environment
    ''' </summary>
    EMPTYENV = 242
    ''' <summary>
    ''' Global environment
    ''' </summary>
    GLOBALENV = 253
    ''' <summary>
    ''' NIL value
    ''' </summary>
    NILVALUE = 254
    ''' <summary>
    ''' Reference
    ''' </summary>
    REF = 255
End Enum

