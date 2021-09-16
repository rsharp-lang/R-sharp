# 经典三旋转：

# 旋转数组经典算法就是三旋转先整体旋转之后在局部旋转
# 需要注意 求余运算，超过数组长度后要取余数后在旋转
# 如：[1,2,3,4,5,6,7]  3
# > [7,6,5,4,3,2,1]  整体旋转
# > [5,6,7,4,3,2,1]  [..k] 旋转
# > [5,6,7,1,2,3,4]  [k..] 旋转

# let k: usize = k as usize % nums.len();
# nums.reverse();
# nums[..k].reverse();
# nums[k..].reverse();

# let len = nums.len();
# nums.rotate_right(k as usize  % len)
# %也可以使用rem_euclid(i32) 这个函数代替。

# 
# let i=nums.len() as i32;
# ums.rotate_right(k.rem_euclid(i) as usize)

print(all(rotate_right([1,2,3,4,5,6,7], 3) == [5,6,7,1,2,3,4]));
# 4 5 6 7 8 9 1 2 3
print(all(rotate_left([1,2,3,4,5,6,7,8,9], 3) == [4, 5, 6, 7, 8, 9, 1, 2, 3]));
# 0 15 8 8 2 4 6 11 
print(all(rotate_left([8, 2, 4, 6, 11, 0 ,15, 8], 5) == [0, 15, 8, 8, 2, 4, 6, 11]));

const x = [1,2,3,4,5,6,7,8,9];

print("Old vector: ");
print(x);
print("New vector after left rotation 3 times: ");
# 4 5 6 7 8 9 1 2 3
print(x |> rotate_left(3));

cat("\n\n\n");

print("Old vector: ");
print(x);
print("New vector after right rotation 4 times: ");
# 6 7 8 9 1 2 3 4 5
print(x |> rotate_right(4));
