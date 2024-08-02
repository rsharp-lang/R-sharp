require(GCModeller);

imports "dataset" from "MLkit";
imports "bioseq.fasta" from "seqtoolkit";

let seqs = NULL;

for(let file in ["./cluster1.txt" "./cluster2.txt" "./cluster3.txt"]) {
    let fa = read.fasta(file);
    fa = as.data.frame(fa);
    print(fa);
    rownames(fa) = `${basename(file)}-${fa$id}`;
    fa[,"label"] = basename(file);
    seqs = rbind(seqs, fa);
}

print(seqs);

write.csv(seqs, file = file.path(@dir, "seqs.csv"));

let sgt = SGT(alphabets = bioseq.fasta::chars("Protein"));

seqs[,"name"] = rownames(seqs);

let vec = list();

for(let seq in as.list( seqs, byrow = TRUE)) {
    vec[[seq$name]] = as.list(fit(sgt, sequence = seq$seq));
}

# str(vec);

vec = as.data.frame(vec, t= TRUE);
rownames(vec) = seqs$name;

print(vec);

write.csv(vec, file = file.path(@dir, "vector.csv"));