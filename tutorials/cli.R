# A demo for run external cli command in R# environment
#
# Add a @ symbol prefix to a string expression means invoke
# a cli command
# such syntax returns the std_output text data after the cli executation
# success
#
# if the cli command executation failure, or command not found
# then code will be stopped if the suppress keyword is not set

let app as string = 'eggHTS';
let APP_HOME = "D:/GCModeller/GCModeller/bin";
let std_out <- @`${APP_HOME}/${app}.exe`;

print(std_out);