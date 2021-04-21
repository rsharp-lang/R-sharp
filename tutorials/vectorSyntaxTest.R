let getAllFiles as function(save_dir, QC_folder) {
	[gsub(save_dir, "file:", ""), QC_folder] 
	:> lapply(dir -> list.files(trim(dir, '"'), pattern = "*.raw", recursive = TRUE)) 
	:> unlist 
	:> basename
	;
}