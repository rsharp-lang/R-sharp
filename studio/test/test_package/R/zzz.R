imports "visualPlot" from "visualkit";
imports "geneExpression" from "phenotype_kit";
imports ["dataset", "umap", "clustering"] from "MLkit";

let .onLoad as function() {
	cat("\n\n");

	print("Welcome to the bionovogene lipidsearch analysis toolkit!");
	print("if any question about this inhouse package, please contact the author:");
	print("  xieguigang <gg.xie@bionovogene.com>");
	
	cat("\n\n");
}