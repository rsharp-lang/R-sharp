require(JSON);

let zhcn = "['/mnt/smb3/\u9879\u76ee\t\u4ee5\u5916\u5185\u5bb9/2024/中文字符名称']";

print(JSON::json_decode(zhcn));