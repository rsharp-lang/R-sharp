const process_project = function(vbproj, refer, banner, proj_folder, stat = list(
        totalLines   = [],
        commentLines = [],
        blankLines   = [],
        size         = [],
        lineOfCodes  = [],

        classes   = [],
        method    = [],
        operator  = [],
        functions = [],
        property  = [],

        files    = [],
        projList = []
    )) {

    let v = walkFiles(vbproj, refer, banner, proj_folder);

    print(vbproj);

    # append project details
    stat$totalLines   = append(stat$totalLines, v$totalLines);
    stat$commentLines = append(stat$commentLines, v$commentLines);
    stat$blankLines   = append(stat$blankLines, v$blankLines);
    stat$size         = append(stat$size, v$size);
    stat$lineOfCodes  = append(stat$lineOfCodes, v$lineOfCodes);

    stat$classes   = append(stat$classes, v$Class);
    stat$method    = append(stat$method, v$method);
    stat$operator  = append(stat$operator, v$operator);
    stat$functions = append(stat$functions, v$function);
    stat$property  = append(stat$property, v$property);

    stat$files    = append(stat$files, v$files);
    stat$projList = append(stat$projList, v$projList);

    return(v);
}