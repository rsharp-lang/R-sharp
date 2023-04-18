$("#kendo_adme").kendoGrid({
    dataSource: {
        data: mol,
    },
    resizable: true,
    columns: [{ field: "mw", title: "MW", width: "60px", type: "number", format: "{0:n}" },
    { field: "alogp", title: "AlogP", width: "60px", type: "number", format: "{0:n}" },
    { field: "hdon", title: "Hdon", width: "53px", type: "number" },
    { field: "hacc", title: "Hacc", width: "50px", type: "number" },
    { field: "ob", title: "OB (%)", width: "65px", type: "number", format: "{0:n}" },
    { field: "caco2", title: "Caco-2", width: "70px", type: "number", format: "{0:n}" },
    { field: "bbb", title: "BBB", width: "55px", type: "number", format: "{0:n}" },
    { field: "dl", title: "DL", width: "50px", type: "number", format: "{0:n}" },
    { field: "FASA", width: "65px", title: "FASA-", type: "number", format: "{0:n}" },
    { field: "tpsa", title: "TPSA", width: "65px", type: "number", format: "{0:n}" },
    { field: "rbn", title: "RBN", width: "50px", type: "number" },
    { field: "halflife", title: "HL", width: "55px", type: "number", format: "{0:n}" }
    ]
});