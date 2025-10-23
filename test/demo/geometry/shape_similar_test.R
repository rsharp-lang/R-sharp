imports "dataframe" from "base";

require(ggplot);

setwd(@dir);

let a = read.arff("triangles.arff");

print(a);

a = as.list(a, byrow=TRUE) |> lapply(function(t, i) {
    data.frame(x = c(t$x1,t$x2,t$x3),y= c(t$y1,t$y2,t$y3), class = rep( t$cls, times = 3), triangle_id = rep(`triangle_${i}`, times = 3));
}) |> bind_rows()
;

print(a);

bitmap(file = "triangle_shapes.png") {
    ggplot(a, aes(x = "x", y = "y"))
    + geom_polygon(aes(group = "triangle_id", fill = "class"),  # 按三角形分组并按cls填充颜色
               color = "black",    # 多边形边框颜色
               alpha = 0.6,        # 设置透明度（可选）
               size = 0.5);
}