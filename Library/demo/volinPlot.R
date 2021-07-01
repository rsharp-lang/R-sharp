imports "charts" from "graphics";
imports "validation" from "MLkit";

options(strict = FALSE);

data       = `${dirname(@script)}/data2/MSI_peaktable.csv`;
sampleinfo = read.csv(`${dirname(@script)}/data2/sampleInfo.csv`); 
dataTbl    = read.csv(data, row.names = 1);

str(dataTbl);

biomarker = "MZ_146.0617";
biodata   = dataTbl[biomarker, , drop = TRUE];

getVector = function(group) {
	sample = (sampleinfo[sampleinfo[, "sample_info"] == group, ])[, "ID"];
	print(sample);
	sample = unlist(biodata[sample]);
	print(sample);
	sample;
}

sample1 = getVector("X5.Contour");
sample2 = getVector("X4.Contour");

tag  = c(rep(FALSE, times = length(sample1)), rep(TRUE, times = length(sample2)));
data = c(sample1, sample2);
roc  = prediction(data / max(data), tag);

print(roc);

bitmap(file = `${dirname(@script)}/data2/MZ_146.0617_roc.png`) {
	plot(roc, size = [3600,3300]);
}

bitmap(file = `${dirname(@script)}/data2/violin.png`) {
	volinPlot(y, 
		 margin = "padding: 300px 150px 200px 300px;",
		 size   = "2700,2700",
		 x.lab   = "X",
		 ylab   = ylab,
		 legend.anchor = [1900.0, 1450.0],
		 legendBgFill = "white",
		 showLegend = FALSE,
		 title = `volin plot of ${ylab}`,
		 labelAngle = 0,
		 colorSet = "#94cac1"
	);
}
	
