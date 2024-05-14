let names = ["258_Herniarin_[M+H2O+H]+" "993_Geranyl acetate_[M+H2O+H]+" "3229_Glycerol_[M+NH4]+" "398587_Cytidine_[M+H]+" "398007_Adenosine_[M+H]+"];
let grep = "$(\d+)_$(\[\d*M.*\]\d*[+-])";

print(text_grep(grep, names));