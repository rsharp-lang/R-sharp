

const processSingle = function(refer, banner, proj_folder, stat = list()) {
    let vbproj = read.vbproj(file = refer, legacy = FALSE);

    if (is.null(vbproj)) {
        print(`skip of the .NET Framework project: ${refer}...`);
    } else {
        print(`processing vbproj: ${refer}`);

        # run project processing...
        let v = process_project(vbproj, refer, banner, proj_folder, stat);

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
        stat$projList = append(stat$projList, relative_path(refer, proj_folder));
    }

    print("~done!");

    return(stat);
}