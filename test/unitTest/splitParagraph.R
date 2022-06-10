imports "Html" from "webKit";

labels = "1,2-dipalmitoyl-3'-<i>O</i>-(L-lysyl)-phosphatid... [M-H2O+NH4]+ 851.634";

print(labels);

labels = splitParagraph(Html::plainText(labels), 24);

print(labels);