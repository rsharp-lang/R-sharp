imports "VisualStudio" from "devkit";
imports "./banner.py";

# banner data for update onto the source files
[@info "Load code banner data"]
const banner_xml as string = ?"--banner-xml" || stop("no banner xml file!");
const banner = read.banner(banner_xml);
# folder that contains the source projects
[@info "Open the folder which contains the project sources to write banner data."]
const proj_folder = ?"--proj-folder" || stop("A folder path that contains vbproj file must be provided!");

[@info "A folder name list for ignores in the project source file scanning.
        Multiple folder name could be combines in this parameter string, 
		each folder name should be seperated via a comma(,) or semicolon(;) 
		symbol."]
let ignores as string = ?"--ignores" || "";

if (ignores != "") {
	ignores = unlist(strsplit(ignores, "[;,]+", fixed = FALSE));
}

print(`banner data from '${banner_xml}'!`);
print(banner);

# only apply of the banner to the project source file?
let projects = list.files(proj_folder, pattern = "*.vbproj", recursive = TRUE);

print(`get ${length(projects)} target source projects!`);
print(projects);

for (refer in projects) {
	# ignores project files that appears in the ignores list
	const tokens = unlist(strsplit(refer, "[\\/]+", fixed = FALSE));
	const test = any(tokens in ignores);
	
	# no matches, do banner updates
	if (!test) {
		processSingle(refer);
	} else {
		print("Skip of the target project updates due to its path value matched with the ignores list:");
		print("[ignores] " & refer);
	}   
}

code_stats(save = `${proj_folder}/proj_stats.csv`);