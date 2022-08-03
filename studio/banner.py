totalLines   = []
commentLines = []
blankLines   = []
size         = []
lineOfCodes  = []

classes   = []
method    = []
operator  = []
functions = []
property  = []

files    = []
projList = []

def walkFiles(vbproj, refer):
    v = list()
    root = dirname(refer)

    for file in sourceFiles(vbproj):
        print(file)

        stat = write.code_banner(file, banner, rootDir = proj_folder)

        v$totalLines   = append(v$totalLines, [stat]::totalLines)
        v$commentLines = append(v$commentLines, [stat]::commentLines)
        v$blankLines   = append(v$blankLines, [stat]::blankLines)
        v$size         = append(v$size, [stat]::size)
        v$lineOfCodes  = append(v$lineOfCodes, [stat]::lineOfCodes)

        v$Class    = append(v$Class, [stat]::classes)
        v$method   = append(v$method, [stat]::method)
        v$operator = append(v$operator, [stat]::operator)
        v$function = append(v$function, [stat]::function)
        v$property = append(v$property, [stat]::properties)

        v$files    = append(v$files, getRelativePath(file, root))
        v$projList = append(v$projList, getRelativePath(refer, proj_folder))

    print("-----------------------------")

    return v

def process_project(vbproj, refer):
    print(vbproj)

    v = walkFiles(vbproj, refer) 

    # append project details
    totalLines   = append(totalLines, v$totalLines)
    commentLines = append(commentLines, v$commentLines)
    blankLines   = append(blankLines, v$blankLines)
    size         = append(size, v$size)
    lineOfCodes  = append(lineOfCodes, v$lineOfCodes)

    classes   = append(classes, v$Class)
    method    = append(method, v$method)
    operator  = append(operator, v$operator)
    functions = append(functions, v$function)
    property  = append(property, v$property)

    files    = append(files, v$files)
    projList = append(projList, v$projList)

    return v
    
def processSingle(refer):
    vbproj = read.vbproj(file = refer, legacy = True)
    
    if is.null(vbproj):
        print(`skip of the .NET Framework project: ${refer}...`)
    else        
        print(`processing vbproj: ${refer}`)

        # run project processing...
        v = process_project(vbproj, refer)

        # append project summary
        totalLines   = append(totalLines, sum(v$totalLines))
        commentLines = append(commentLines, sum(v$commentLines))
        blankLines   = append(blankLines, sum(v$blankLines))
        size         = append(size, sum(v$size))
        lineOfCodes  = append(lineOfCodes, sum(v$lineOfCodes))
        
        classes   = append(classes, sum(v$Class))
        method    = append(method, sum(v$method))
        operator  = append(operator, sum(v$operator))
        functions = append(functions, sum(v$function))
        property  = append(property, sum(v$property))

        files    = append(files, "<project>")
        projList = append(projList, getRelativePath(refer, proj_folder))
    
    print("~done!")

def code_stats(save):
    v = [proj_folder, "<root>", sum(totalLines), sum(commentLines), sum(blankLines), sum(lineOfCodes), sum(classes), sum(property), sum(method), sum(functions), sum(operator), sum(size)]

    stat = data.frame(proj = projList, files, totalLines, commentLines, blankLines, lineOfCodes, "class" = classes, property, method, functions, operator, "size(bytes)" = size)
    stat = rbind(stat, v)
    stat[, "size"] = byte_size(stat[, "size(bytes)"])
    
    print(stat, max.print = 13)

    write.csv(stat, file = save, row.names = False)