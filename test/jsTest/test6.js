if (sname != null) {
        $("#kendo_sname").kendoGrid({
            dataSource: {
                data: sname,
                pageSize: 10,
            },
            pageable: true,
            sortable: true,
            filterable: true,
            resizable: true,
            columnMenu: true,
            columns: [{ field: "molecule_synonyms", title: "Molecule synonyms" }
            ]
        });
    } else { $("#kendo_sname").html("N/A") };