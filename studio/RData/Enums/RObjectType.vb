#Region "Microsoft.VisualBasic::60fccf2140a0080bfe1d5706f616a7f5, studio\RData\Enums\RObjectType.vb"

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

    ' Enum RObjectType
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
    [CHAR] = 9
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
    ''' Alternative representations
    ''' </summary>
    ALTREP = 238

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
