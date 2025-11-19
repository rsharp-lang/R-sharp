Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Development.CommandLine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports renv = SMRUCC.Rsharp.Runtime

''' <summary>
''' Command Line Option Parser
''' </summary>
<Package("optparse")>
Module optparse

    Const NULL As Object = Nothing

    ''' <summary>
    ''' Functions to enable our OptionParser to recognize specific command line options.
    ''' 
    ''' add_option adds a option to a prexisting OptionParser instance whereas make_option is used to 
    ''' create a list of OptionParserOption instances that will be used in the option_list argument 
    ''' of the OptionParser function to create a new OptionParser instance.
    ''' </summary>
    ''' <param name="opt_str">A character vector containing the string of the desired long flag comprised of “–” followed by a letter and then a sequence of alphanumeric characters and optionally a string of the desired short flag comprised of the “-” followed by a letter.</param>
    ''' <param name="action">A character string that describes the action optparse should take when it encounters an option, either “store”, “store_true”, “store_false”, or “callback”. An action of “store” signifies that optparse should store the specified following value if the option is found on the command string. “store_true” stores TRUE if the option is found and “store_false” stores FALSE if the option is found. “callback” stores the return value produced by the function specified in the callback argument. If callback is not NULL then the default is “callback” else “store”.</param>
    ''' <param name="type">A character string that describes specifies which data type should be stored, either “logical”, “integer”, “double”, “complex”, or “character”. Default is “logical” if action %in% c("store_true", store_false), typeof(default) if action == "store" and default is not NULL and “character” if action == "store" and default is NULL. “numeric” will be converted to “double”.</param>
    ''' <param name="dest">A character string that specifies what field in the list returned by parse_args should optparse store option values. Default is derived from the long flag in opt_str.</param>
    ''' <param name="default">The default value optparse should use if it does not find the option on the command line.</param>
    ''' <param name="help">A character string describing the option to be used by print_help in generating a usage message. %default will be substituted by the value of default.</param>
    ''' <param name="metavar">A character string that stands in for the option argument when printing help text. Default is the value of dest.</param>
    ''' <param name="callback">A function that executes after the each option value is fully parsed. It's value is assigned to the option and its arguments are the option S4 object, the long flag string, the value of the option, the parser S4 object, and ....</param>
    ''' <param name="callback_args">A list of additional arguments passed to callback function (via do.call).</param>
    ''' <returns>Both make_option and add_option return instances of class <see cref="OptionParserOption"/>.</returns>
    <ExportAPI("make_option")>
    Public Function make_option(opt_str$(),
                                Optional action As String = NULL,
                                Optional type As String = NULL,
                                Optional dest As String = NULL,
                                <RRawVectorArgument>
                                Optional [default] As Object = NULL,
                                Optional help As String = "",
                                Optional metavar As String = NULL,
                                Optional callback As Object = NULL,
                                Optional callback_args As Object = NULL) As OptionParserOption

        Return New OptionParserOption With {
            .[default] = [default],
            .help = help,
            .opt_str = opt_str,
            .type = type,
            .action = action
        }
    End Function

    ''' <summary>
    ''' A function to create an instance of a parser object
    ''' 
    ''' This function is used to create an instance of a parser object which when combined with the parse_args, 
    ''' make_option, and add_option methods is very useful for parsing options from the command line.
    ''' </summary>
    ''' <param name="usage">The program usage message that will printed out if parse_args finds a help option, %prog is substituted with the value of the prog argument.</param>
    ''' <param name="option_list">A list of of OptionParserOption instances that will define how parse_args reacts to command line options. OptionParserOption instances are usually created by make_option and can also be added to an existing OptionParser instance via the add_option function.</param>
    ''' <param name="add_help_option">Whether a standard help option should be automatically added to the OptionParser instance.</param>
    ''' <param name="prog">Program name to be substituted for %prog in the usage message (including description and epilogue if present), the default is to use the actual Rscript file name if called by an Rscript file and otherwise keep %prog.</param>
    ''' <param name="description">Additional text for print_help to print out between usage statement and options statement</param>
    ''' <param name="epilogue">Additional text for print_help to print out after the options statement</param>
    ''' <param name="formatter">A function that formats usage text. The function should take only one argument (an OptionParser() object). Default is IndentedHelpFormatter(). The other builtin formatter provided by this package is TitledHelpFormatter().</param>
    ''' <returns>An instance of the OptionParser class.</returns>
    <ExportAPI("OptionParser")>
    <RApiReturn(GetType(OptionParser))>
    Public Function OptionParser(Optional usage$ = "usage: %prog [options]",
                                 Optional option_list As list = Nothing,
                                 Optional add_help_option As Boolean = True,
                                 Optional prog As Object = NULL,
                                 Optional description$ = "",
                                 Optional epilogue$ = "",
                                 Optional formatter As Object = "IndentedHelpFormatter",
                                 Optional title As String = Nothing,
                                 Optional dependency As String() = Nothing,
                                 Optional env As Environment = Nothing) As Object

        Return New OptionParser With {
            .add_help_option = add_help_option,
            .description = description,
            .epilogue = epilogue,
            .option_list = If(option_list Is Nothing OrElse option_list.is_empty,
                New OptionParserOption() {},
                renv.TryCastGenericArray(option_list.data.ToArray, env)),
            .usage = usage,
            .title = title,
            .dependency = dependency
        }
    End Function

    ''' <summary>
    ''' ### Parse command line options.
    ''' 
    ''' parse_args parses command line options using an OptionParser instance for guidance. parse_args2 is a wrapper to 
    ''' parse_args setting the options positional_arguments and convert_hyphens_to_underscores to TRUE.
    ''' </summary>
    ''' <param name="object">An OptionParser instance.</param>
    ''' <param name="args">A character vector containing command line options to be parsed. Default is everything after the Rscript program in the command line. If positional_arguments is not FALSE then parse_args will look for positional arguments at the end of this vector.</param>
    ''' <param name="print_help_and_exit">Whether parse_args should call print_help to print out a usage message and exit the program. Default is TRUE.</param>
    ''' <param name="positional_arguments">Number of positional arguments. A numeric denoting the exact number of supported arguments, or a numeric vector of length two denoting the minimum and maximum number of arguments (Inf for no limit). The value TRUE is equivalent to c(0, Inf). The default FALSE is supported for backward compatibility only, as it alters the format of the return value.</param>
    ''' <param name="convert_hyphens_to_underscores">If the names in the returned list of options contains hyphens then convert them to underscores. The default FALSE is supported for backward compatibility reasons as it alters the format of the return value</param>
    ''' <returns>Returns a list with field options containing our option values as well as another field args which 
    ''' contains a vector of positional arguments. For backward compatibility, if and only if positional_arguments 
    ''' is FALSE, returns a list containing option values.</returns>
    ''' <remarks>
    ''' #### Acknowledgement
    ''' 
    ''' A big thanks To Steve Lianoglou For a bug report And patch; Juan Carlos BorrÃ¡s For a bug report; Jim Nikelski For a bug 
    ''' report And patch; Ino de Brujin And Benjamin Tyner For a bug report; Jonas Zimmermann For bug report; Miroslav Posta For
    ''' bug reports; Stefan Seemayer For bug report And patch; Kirill MÃ¼ller For patches; Steve Humburg For patch.
    ''' </remarks>
    <ExportAPI("parse_args")>
    <RApiReturn(TypeCodes.list)>
    Public Function parse_args([object] As OptionParser,
                               <RLazyExpression>
                               Optional args As Object = "~commandArgs(parse_args = 'clr')",
                               Optional print_help_and_exit As Boolean = True,
                               Optional positional_arguments As Boolean = False,
                               Optional convert_hyphens_to_underscores As Boolean = False) As Object

        If print_help_and_exit Then
            Dim doc As CommandLineDocument = [object].GetDocument
            Dim stdout As TextWriter = App.StdOut

            Call doc.PrintUsage(stdout)
            Call stdout.Flush()
        End If
    End Function
End Module



