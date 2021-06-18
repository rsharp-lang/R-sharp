imports "sqlite" from "devkit";

using xcc as sqlite::open(`${dirname(@script)}/xcc.db`) {
    xcc
    |> sqlite::ls
    |> print()
    ;

	for(name in ["Pathways", "Orthologs", "Genes"]) {
		print("view data contents of table:");
		print(name);
		
		xcc
		|> sqlite::load(name)
		|> head
		|> print
		;
	}    
}