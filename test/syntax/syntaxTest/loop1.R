	for(i in 1:length(name)) {
		plotRow(list(
			anno    = strsplit(anno[i], ";\s+", fixed = FALSE),
			mz      = as.numeric(strsplit(mz[i],";\s+")),
			into    = as.numeric(strsplit(into[i], ";\s+")),
			name    = name[i],
			xcms_id = xcms_id[i],
			image   = list("*[Agly]+" = images[[family[i]]])
		), outputdir);
	}