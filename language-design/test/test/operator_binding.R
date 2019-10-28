imports "Microsoft.VisualBasic.dll";

let me = list() with {
    $name <- "xieguigang";
    $family <- {"xyz"};
    $display <- function() {
       return $name & " has {$family:Length} families.";
    }
    
    %+% <- function($, name as string) {
        $family.append(name);
        return $family;
    }
}

let out as string = me + "abc";

str(out);
# string [1:2] "xyz" "abc"

me$display();
# xieguigang has 2 families.

library("Microsoft.VisualBasic.Strings");

var index.string <- list() with {
    $string <- "abcdefg";
    
    %in% <- function(str, $) {
        return InStr($string, str);
    }
}

"abcd" in index.string;
# [1] 1

"xyz" in index.string;
# [1] 0
