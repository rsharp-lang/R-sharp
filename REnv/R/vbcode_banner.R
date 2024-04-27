#' process a single vb.net source file
#' 
const walkFiles = function(vbproj, refer, banner, proj_folder) {
    let v = list();
    let root = dirname(refer);

    for(file in tqdm(sourceFiles(vbproj))) {
        let stat = write.code_banner(file, banner, rootDir = proj_folder);

        v$totalLines   = append(v$totalLines, [stat]::totalLines);
        v$commentLines = append(v$commentLines, [stat]::commentLines);
        v$blankLines   = append(v$blankLines, [stat]::blankLines);
        v$size         = append(v$size, [stat]::size);
        v$lineOfCodes  = append(v$lineOfCodes, [stat]::lineOfCodes);

        v$Class    = append(v$Class, [stat]::classes);
        v$method   = append(v$method, [stat]::method);
        v$operator = append(v$operator, [stat]::operator);
        v$function = append(v$function, [stat]::function);
        v$property = append(v$property, [stat]::properties);

        v$files    = append(v$files, getRelativePath(file, root));
        v$projList = append(v$projList, getRelativePath(refer, proj_folder));
    }

    print("-----------------------------");

    return(v);
}

const process_project = function(vbproj, refer, stat = list(
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

    let v = walkFiles(vbproj, refer);

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

const processSingle(refer, stat = list()) {
    let vbproj = read.vbproj(file = refer, legacy = FALSE);

    if (is.null(vbproj)) {
        print(`skip of the .NET Framework project: ${refer}...`);
    } else {
        print(`processing vbproj: ${refer}`);

        # run project processing...
        let v = process_project(vbproj, refer);

        # append project summary
        stat$totalLines   = append(stat$totalLines, sum(v$totalLines));
        stat$commentLines = append(stat$commentLines, sum(v$commentLines));
        stat$blankLines   = append(stat$blankLines, sum(v$blankLines));
        stat$size         = append(stat$size, sum(v$size));
        stat$lineOfCodes  = append(stat$lineOfCodes, sum(v$lineOfCodes));
        
        stat$classes   = append(stat$classes, sum(v$Class));
        stat$method    = append(stat$method, sum(v$method));
        stat$operator  = append(stat$operator, sum(v$operator));
        stat$functions = append(stat$functions, sum(v$function));
        stat$property  = append(stat$property, sum(v$property));

        stat$files    = append(stat$files, "<project>");
        stat$projList = append(stat$projList, getRelativePath(refer, proj_folder));
    }

    print("~done!");

    return(stat);
}

#' export the result as csv file
#' 
const code_stats = function(stats, proj_folder, save) {
    let v = [
        proj_folder, 
        "<root>", 
        sum(stats$totalLines), 
        sum(stats$commentLines), 
        sum(stats$blankLines), 
        sum(stats$lineOfCodes), 
        sum(stats$classes), 
        sum(stats$property), 
        sum(stats$method), 
        sum(stats$functions), 
        sum(stats$operator), 
        sum(stats$size)
    ];
    let stat = data.frame(
        proj          = stats$projList, 
        files         = stats$files, 
        totalLines    = stats$totalLines, 
        commentLines  = stats$commentLines, 
        blankLines    = stats$blankLines, 
        lineOfCodes   = stats$lineOfCodes, 
        "class"       = stats$classes, 
        property      = stats$property, 
        method        = stats$method, 
        functions     = stats$functions, 
        operator      = stats$operator, 
        "size(bytes)" = stats$size
    );
    
    stat = rbind(stat, v);
    stat[, "size"] = byte_size(stat[, "size(bytes)"]);
    
    print(stat, max.print = 13);

    write.csv(stat, file = save, row.names = FALSE);
}