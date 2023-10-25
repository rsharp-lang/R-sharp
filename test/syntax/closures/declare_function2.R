const .term_maps = function(type = ["genes","disease","compounds"], x) {
	const map = {
		"genes": Article -> list(query = [Article]::ChemicalName, partner = [Article]::GeneSymbolName, type = type),
		"disease": Article -> list(query = [Article]::ChemicalName, partner = [Article]::DiseaseName, type = type),
		"compounds": Article -> list(query = [Article]::ChemicalName_1, partner = [Article]::ChemicalName_2, type = type)
	};
	const f = map[[type]];
	
	f(x);
}