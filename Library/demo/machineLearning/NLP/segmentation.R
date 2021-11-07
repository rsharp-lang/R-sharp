imports "NLP" from "MLkit";

options(strict = FALSE);

data = NLP::segmentation(readText("E:\GCModeller\src\runtime\sciBASIC#\Data\Trinity\alice30.txt"));

print(data);