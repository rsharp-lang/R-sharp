require(REnv);

data(Mall_Customers);

print(Mall_Customers, max.print = 6);

# table summary of the gender

print(table(Mall_Customers$Gender));
