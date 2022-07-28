imports "Html" from "webKit";

labels = "1,2-dipalmitoyl-3'-<i>O</i>-(L-lysyl)-phosphatid... [M-H2O+NH4]+ 851.634";

print(labels);

labels = splitParagraph(Html::plainText(labels), 24);

print(labels);

labels = "The coverage cutoff value for measure the RGB ion combination score. if there is no ms-imaging result in the triple ions 
        ms-imaging folder, then it means this parameter cutoff value 
        may be too high for filter ions list, try to make this threshold cutoff value smaller.";
		
		print("the raw text:");
		cat(labels);
		cat("\n\n\n");
		
labels = splitParagraph(Html::plainText(labels), 64);

print("the text split result:");

print(labels);