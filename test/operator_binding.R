var me <- list() with {
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

var out as string <- me + "abc";

str(out);
# string [1:2] "xyz" "abc"

me$display();
# xieguigang has 2 families.
