require(GCModeller);

imports "dataset" from "MLkit";
imports "bioseq.fasta" from "seqtoolkit";

let seqs = [];

for(let file in ["./cluster1.txt" "./cluster2.txt" "./cluster3.txt"]) {
    let fa = read.fasta(file);
    
}


