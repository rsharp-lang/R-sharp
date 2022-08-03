imports "devkit" from "VisualStudio";
imports "./banner.py";

# banner data for update onto the source files
[@info "Load code banner data"]
const banner_xml as string = ?"--banner-xml" || stop("no banner xml file!");
const banner = read.banner(banner_xml);
# folder that contains the source projects
[@info "Open the folder which contains the project sources to write banner data."]
const proj_folder = ?"--proj-folder" || stop("A folder path that contains vbproj file must be provided!");

print(`banner data from '${banner_xml}'!`);
print(banner);

# only apply of the banner to the project source file?
projects = list.files(proj_folder, pattern = "*.vbproj", recursive = TRUE);

print(`get ${length(projects)} target source projects!`);
print(projects);

for (refer in projects) {
    processSingle(refer);
}

code_stats(save = `${proj_folder}/proj_stats.csv`);