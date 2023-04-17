#Region "Microsoft.VisualBasic::3da1b13741ed8603e744f2206933c721, D:/GCModeller/src/R-sharp/Library/base//utils/jsonlite.vb"

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

    '   Total Lines: 221
    '    Code Lines: 65
    ' Comment Lines: 147
    '   Blank Lines: 9
    '     File Size: 12.27 KB


    ' Module jsonlitePackage
    ' 
    '     Function: base64_dec, base64_enc, flatten, fromJSONEx, toJSONEx
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe

''' <summary>
''' ## A Simple and Robust JSON Parser and Generator for R
''' 
''' A reasonably fast JSON parser and generator, optimized for statistical 
''' data and the web. Offers simple, flexible tools for working with JSON in R, and
''' is particularly powerful for building pipelines and interacting with a web API. 
''' The implementation is based on the mapping described in the vignette (Ooms, 2014).
''' In addition to converting JSON data from/to R objects, 'jsonlite' contains 
''' functions to stream, validate, and prettify JSON data. The unit tests included 
''' with the package verify that all edge cases are encoded and decoded consistently 
''' for use with dynamic data in systems and applications.
''' </summary>
<Package("jsonlite",
         Category:=APICategories.UtilityTools,
         Cites:="https://arxiv.org/abs/1403.2805",
         Description:="A reasonably fast JSON parser and generator, optimized for statistical 
    data and the web. Offers simple, flexible tools for working with JSON in R, and
    is particularly powerful for building pipelines and interacting with a web API. 
    The implementation is based on the mapping described in the vignette (Ooms, 2014).
    In addition to converting JSON data from/to R objects, 'jsonlite' contains 
    functions to stream, validate, and prettify JSON data. The unit tests included 
    with the package verify that all edge cases are encoded and decoded consistently 
    for use with dynamic data in systems and applications.",
         Publisher:="Jeroen Ooms <jeroen@berkeley.edu>",
         Url:="https://github.com/jeroen/jsonlite/")>
Module jsonlitePackage

    ''' <summary>
    ''' Simple in-memory base64 encoder and decoder. Used 
    ''' internally for converting raw vectors to text. 
    ''' Interchangeable with encoder from base64enc or 
    ''' openssl package.
    ''' </summary>
    ''' <param name="input">
    ''' string or raw vector to be encoded/decoded
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI>
    <RApiReturn(GetType(String))>
    Public Function base64_enc(<RRawVectorArgument> input As Object, Optional env As Environment = Nothing) As Object

        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' Simple in-memory base64 encoder and decoder. Used 
    ''' internally for converting raw vectors to text. 
    ''' Interchangeable with encoder from base64enc or 
    ''' openssl package.
    ''' </summary>
    ''' <param name="input">
    ''' string or raw vector to be encoded/decoded
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI>
    <RApiReturn(GetType(String))>
    Public Function base64_dec(<RRawVectorArgument> input As Object, Optional env As Environment = Nothing) As Object
        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' ## Flatten nested data frames
    ''' 
    ''' In a nested data frame, one or more of the columns 
    ''' consist of another data frame. These structures 
    ''' frequently appear when parsing JSON data from the web. 
    ''' We can flatten such data frames into a regular 2 
    ''' dimensional tabular structure.
    ''' </summary>
    ''' <param name="x">a data frame</param>
    ''' <param name="recursive">flatten recursively</param>
    ''' <returns></returns>
    <ExportAPI>
    Public Function flatten(x As Rdataframe, Optional recursive As Boolean = True) As Object
        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' ## Convert R objects to/from JSON
    ''' 
    ''' These functions are used to convert between JSON data 
    ''' and R objects. The toJSON and fromJSON functions use a 
    ''' class based mapping, which follows conventions outlined 
    ''' in this paper: https://arxiv.org/abs/1403.2805 (also
    ''' available as vignette).
    ''' </summary>
    ''' <param name="txt">a JSON string, URL or file</param>
    ''' <param name="simplifyVector">coerce JSON arrays containing only primitives into an atomic vector</param>
    ''' <param name="simplifyDataFrame">coerce JSON arrays containing only records (JSON objects) into a data frame</param>
    ''' <param name="simplifyMatrix">coerce JSON arrays containing vectors of equal mode and dimension into matrix or array</param>
    ''' <param name="flatten">automatically flatten nested data frames into a single non-nested data frame</param>
    ''' <param name="args">arguments passed on to class specific print methods</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' The toJSON and fromJSON functions are drop-in replacements 
    ''' for the identically named functions in packages rjson and 
    ''' RJSONIO. Our implementation uses an alternative, somewhat 
    ''' more consistent mapping between R objects and JSON strings.
    ''' 
    ''' The serializeJSON and unserializeJSON functions in this 
    ''' package use an alternative system to convert between R objects 
    ''' and JSON, which supports more classes but is much more verbose.
    ''' A JSON string is always unicode, using UTF-8 by default, 
    ''' hence there is usually no need to escape any characters. 
    ''' However, the JSON format does support escaping of unicode 
    ''' characters, which are encoded using a backslash followed 
    ''' by a lower case "u" and 4 hex characters, for example: 
    ''' "Z\u00FCrich". The fromJSON function will parse such escape 
    ''' sequences but it is usually preferable to encode unicode 
    ''' characters in JSON using native UTF-8 rather than escape 
    ''' sequences.
    ''' </remarks>
    <ExportAPI("fromJSON")>
    Public Function fromJSONEx(txt As String,
                               Optional simplifyVector As Boolean = True,
                               Optional simplifyDataFrame As Boolean = True,
                               Optional simplifyMatrix As Boolean = True,
                               Optional flatten As Boolean = False,
                               <RListObjectArgument>
                               Optional args As list = Nothing,
                               Optional env As Environment = Nothing) As Object

        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' ## Convert R objects to/from JSON
    ''' 
    ''' These functions are used to convert between JSON data and R 
    ''' objects. The toJSON and fromJSON functions use a class based
    ''' mapping, which follows conventions outlined in this paper: 
    ''' https://arxiv.org/abs/1403.2805 (also available as vignette).
    ''' </summary>
    ''' <param name="x">the object to be encoded</param>
    ''' <param name="dataframe">how to encode data.frame objects: must
    ''' be one of 'rows', 'columns' or 'values'</param>
    ''' <param name="matrix">how to encode matrices and higher dimensional 
    ''' arrays: must be one of 'rowmajor' or 'columnmajor'.</param>
    ''' <param name="Date">how to encode Date objects: must be one of 
    ''' 'ISO8601' or 'epoch'</param>
    ''' <param name="POSIXt">how to encode POSIXt (datetime) objects: 
    ''' must be one of 'string', 'ISO8601', 'epoch' or 'mongo'</param>
    ''' <param name="factor">how to encode factor objects: must be one 
    ''' of 'string' or 'integer'</param>
    ''' <param name="complex">how to encode complex numbers: must be 
    ''' one of 'string' or 'list'</param>
    ''' <param name="raw">how to encode raw objects: must be one of 
    ''' 'base64', 'hex' or 'mongo'</param>
    ''' <param name="null">how to encode NULL values within a list:
    ''' must be one of 'null' or 'list'</param>
    ''' <param name="na">how to print NA values: must be one of 'null' 
    ''' or 'string'. Defaults are class specific</param>
    ''' <param name="auto_unbox">automatically unbox all atomic vectors 
    ''' of length 1. It is usually safer to avoid this and instead use 
    ''' the unbox function to unbox individual elements. An exception is 
    ''' that objects of class AsIs (i.e. wrapped in I()) are not 
    ''' automatically unboxed. This is a way to mark single values as
    ''' length-1 arrays.
    ''' </param>
    ''' <param name="digits">max number of decimal digits to print for 
    ''' numeric values. Use I() to specify significant digits. Use NA 
    ''' for max precision.</param>
    ''' <param name="pretty">adds indentation whitespace to JSON output.
    ''' Can be TRUE/FALSE or a number specifying the number of spaces 
    ''' to indent. See prettify</param>
    ''' <param name="force">unclass/skip objects of classes with no 
    ''' defined JSON mapping</param>
    ''' <param name="args">arguments passed on to class specific print 
    ''' methods</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' The toJSON and fromJSON functions are drop-in replacements for 
    ''' the identically named functions in packages rjson and RJSONIO. 
    ''' Our implementation uses an alternative, somewhat more
    ''' consistent mapping between R objects and JSON strings.
    ''' The serializeJSON and unserializeJSON functions in this package
    ''' use an alternative system to convert between R objects and JSON,
    ''' which supports more classes but is much more verbose.
    ''' A JSON string is always unicode, using UTF-8 by default, hence 
    ''' there is usually no need to escape any characters. However, 
    ''' the JSON format does support escaping of unicode characters,
    ''' which are encoded using a backslash followed by a lower case 
    ''' "u" and 4 hex characters, for example: "Z\u00FCrich". The 
    ''' fromJSON function will parse such escape sequences but it is 
    ''' usually preferable to encode unicode characters in JSON using 
    ''' native UTF-8 rather than escape sequences.
    ''' </remarks>
    <ExportAPI("toJSON")>
    Public Function toJSONEx(<RRawVectorArgument> x As Object,
                             <RRawVectorArgument(GetType(String))> Optional dataframe As Object = "rows|columns|values",
                             <RRawVectorArgument(GetType(String))> Optional matrix As Object = "rowmajor|columnmajor",
                             <RRawVectorArgument(GetType(String))> Optional [Date] As Object = "ISO8601|epoch",
                             <RRawVectorArgument(GetType(String))> Optional POSIXt As Object = "string|ISO8601|epoch|mongo",
                             <RRawVectorArgument(GetType(String))> Optional factor As Object = "string|integer",
                             <RRawVectorArgument(GetType(String))> Optional complex As Object = "string|list",
                             <RRawVectorArgument(GetType(String))> Optional raw As Object = "base64|hex|mongo|int|js",
                             <RRawVectorArgument(GetType(String))> Optional null As Object = "list|null",
                             <RRawVectorArgument(GetType(String))> Optional na As Object = "null|string",
                             <RRawVectorArgument(GetType(String))> Optional auto_unbox As Boolean = False,
                             <RRawVectorArgument(GetType(String))> Optional digits As Integer = 4,
                             <RRawVectorArgument(GetType(String))> Optional pretty As Boolean = False,
                             <RRawVectorArgument(GetType(String))> Optional force As Boolean = False,
                             <RRawVectorArgument(GetType(String))> Optional args As list = Nothing,
                             <RRawVectorArgument(GetType(String))> Optional env As Environment = Nothing) As Object
        Throw New NotImplementedException
    End Function

End Module
