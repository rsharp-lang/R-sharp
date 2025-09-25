let test_func = function(ligand_pdbqt) {
    print(`check processed ligand model: ${ligand_pdbqt}`);

    if (!file.exists(ligand_pdbqt)) {
        stop("Failed to generate ligand PDBQT file: ", ligand_pdbqt);
    }
}

test_func("./file_not_exists.file");