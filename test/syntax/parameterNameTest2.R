dam = data.frame(A= 1, row.names = "A");
subset = list(

list(
name = 1,
precursor_type = 2,
        chebi          = 3,
        drugbank       = 4,
        hmdb           = 5,
        kegg           = 6,
        knapsack       = 7,
        lipidmaps      = 8,
        pubchem        = 9,
        MeSH           = 10
)
	
);

x= cbind(dam, data.frame(
        name           = subset@name,
        precursor_type = subset@precursor_type,
        chebi          = subset@chebi,
        drugbank       = subset@drugbank,
        hmdb           = subset@hmdb,
        kegg           = subset@kegg,
        knapsack       = subset@knapsack,
        lipidmaps      = subset@lipidmaps,
        pubchem        = subset@pubchem,
        MeSH           = subset@MeSH,
        row.names      = rownames(dam)
    ));
	
	print(x);