# in test each element in x collection
# is exact match in collection b?
#
# each x in b? 
print([1, 2, 2.5, 3, 4, 5] in [2, 3]);
# result:
# [1] FALSE TRUE FALSE TRUE FALSE FALSE

# between test each element in x collection
# is in a given value range?
#
# each x between a given value range?
print([1, 2, 2.5, 3, 4, 5] between [2, 3]);
# result:
# [1] FALSE TRUE TRUE TRUE FALSE FALSE
