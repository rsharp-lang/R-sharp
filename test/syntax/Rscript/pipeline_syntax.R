let peakstable = peaks_export 
|> list.files(pattern = "*.peakdata")
|> lapply(path -> readBin(path, what = "gcms_peak"), names = path -> basename(path))
|> peak_alignment()
;