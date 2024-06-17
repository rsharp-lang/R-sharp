require(charts);

let data = list(
    A = 55,
    B = 32,
    C = 99,
    D = 112
);

bitmap(file = file.path(@dir, "demo_pie.png")) {
    pie(data);
}

bitmap(file = file.path(@dir, "demo_pie3d.png")) {
    pie(data, d3 = TRUE);
}