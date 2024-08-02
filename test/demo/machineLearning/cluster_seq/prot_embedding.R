require(GCModeller);

imports "dataset" from "MLkit";
imports "bioseq.fasta" from "seqtoolkit";

let seqs = NULL;

for(let file in ["./cluster1.txt" "./cluster2.txt" "./cluster3.txt"]) {
    let fa = read.fasta(file);
    fa = as.data.frame(fa);
    print(fa);
    rownames(fa) = `${basename(file)}-${fa$id}`;
    seqs = rbind(seqs, fa);
}

write.csv(seqs, file = file.path(@dir, "seqs.csv"));

let sgt = SGT(alphabets = );
