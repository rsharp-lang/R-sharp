$(function () {
    var sname = [{ "molecule_synonyms": "BENZENE-1,2,3-TRIOL" }, { "molecule_synonyms": "Pyrogallol" }, { "molecule_synonyms": "87-66-1" }, { "molecule_synonyms": "C.I.  Oxidation Base 32" }, { "molecule_synonyms": "c0025" }, { "molecule_synonyms": "1,2,3-Trihydroxybenzen" }, { "molecule_synonyms": "Benzene, 1,2,3-trihydroxy-" }, { "molecule_synonyms": "C.I. 76515" }, { "molecule_synonyms": "C.I. Oxidation Base 32" }, { "molecule_synonyms": "Fouramine Brown AP" }, { "molecule_synonyms": "Fourrine 85" }, { "molecule_synonyms": "Fourrine PG" }, { "molecule_synonyms": "NSC5035" }, { "molecule_synonyms": "WLN: QR BQ CQ" }, { "molecule_synonyms": "ZINC00330141" }, { "molecule_synonyms": "Pyrogallol polymer" }, { "molecule_synonyms": "1,2,3-Trihydroxybenzen [Czech]" }, { "molecule_synonyms": "4-06-00-07327 (Beilstein Handbook Reference)" }, { "molecule_synonyms": "AI3-00709" }, { "molecule_synonyms": "BRN 0907431" }, { "molecule_synonyms": "CCRIS 1940" }, { "molecule_synonyms": "CI 76515" }, { "molecule_synonyms": "CI Oxidation Base 32" }, { "molecule_synonyms": "EINECS 201-762-9" }, { "molecule_synonyms": "HSDB 794" }, { "molecule_synonyms": "NSC 5035" }, { "molecule_synonyms": "Pyro" }, { "molecule_synonyms": "GMN" }, { "molecule_synonyms": "2,2',2\"-[1,2,3-BENZENE-TRIYLTRIS(OXY)]TRIS[N,N,N-TRIETHYLETHANAMINIUM]" }, { "molecule_synonyms": "CHEBI:16164" }, { "molecule_synonyms": "PYG" }, { "molecule_synonyms": "254002_SIAL" }, { "molecule_synonyms": "Piral" }, { "molecule_synonyms": "AB-131\/40221933" }, { "molecule_synonyms": "NCGC00091507-01" }, { "molecule_synonyms": "fouramine base ap" }, { "molecule_synonyms": "83135_FLUKA" }, { "molecule_synonyms": "Pyrogallol solution" }, { "molecule_synonyms": "16040_RIEDEL" }, { "molecule_synonyms": "MLS001066376" }, { "molecule_synonyms": "SMR000471842" }, { "molecule_synonyms": "AIDS002956" }, { "molecule_synonyms": "P0381_SIGMA" }, { "molecule_synonyms": "AIDS-082397" }, { "molecule_synonyms": "AIDS082397" }, { "molecule_synonyms": "PYROP" }, { "molecule_synonyms": "1,2,3-TRIHYDROXY-BENZENE" }, { "molecule_synonyms": "InChI=1\/C6H6O3\/c7-4-2-1-3-5(8)6(4)9\/h1-3,7-9" }, { "molecule_synonyms": "1,2,3-Benzenetriol" }, { "molecule_synonyms": "1,2,3-Trihydroxybenzene" }, { "molecule_synonyms": "C01108" }, { "molecule_synonyms": "Pyrogallic acid" }, { "molecule_synonyms": "AIDS-002956" }];
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

    var mol = [{ "pubchem_cid": "1057", "MOL_ID": "MOL000106", "molecule_ID": "106", "molecule_name": "PYG", "tpsa": "60.69", "rbn": "0", "inchikey": "WQGWDDDVZFFDIG-UHFFFAOYSA-N", "ob": "22.9778822728", "dl": "0.022766", "bbb": "0.78572", "caco2": "0.69054", "mw": "126.120", "hdon": "3", "hacc": "3", "alogp": "1.028", "halflife": "", "FASA": "0.39706758" }];
    if (mol != null) {
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
    } else { $("#kendo_adme").html("N/A") };

    var herb = [{ "herb_cn_name": "\u767d\u828d", "herb_en_name": "Paeoniae Radix Alba" }, { "herb_cn_name": "\u8349\u679c", "herb_en_name": "Amomum Tsao-Ko Crevostet" }, { "herb_cn_name": "\u8d64\u828d", "herb_en_name": "Radix Paeoniae Rubra" }, { "herb_cn_name": "\u675c\u4ef2", "herb_en_name": "Eucommiae Cortex" }, { "herb_cn_name": "\u864e\u8033\u8349", "herb_en_name": "Saxifraga Stolonifera" }, { "herb_cn_name": "\u5343\u91d1\u5b50", "herb_en_name": "Semen Euphorbiae" }, { "herb_cn_name": "\u9752\u679c", "herb_en_name": "Canarii Fructus" }, { "herb_cn_name": "\u82ab\u837d", "herb_en_name": "Coriandri Sativi Herba" }, { "herb_cn_name": "\u6cfd\u6f06", "herb_en_name": "Euphorbiae Helioscopiae Herba" }, { "herb_cn_name": "\u84bf\u672c", "herb_en_name": "Radix et Rhizoma Ligustici Chinesis" }];
    if (herb != null) {
        $("#kendo_herb").kendoGrid({
            dataSource: {
                data: herb,
                pageSize: 10,
            },
            pageable: true,
            sortable: true,
            filterable: true,
            resizable: true,
            columnMenu: true,
            columns: [{ field: "herb_cn_name", title: "Chinese name", width: "130px" },
            { field: "herb_en_name", title: "Latin name", template: "<a href='tcmspsearch.php?qr=${herb_en_name}&qsr=\"herb_en_name\"&token=f7e531143289682332639b198066b092'>${herb_en_name}</a>" },
            ]
        });
    } else { $("#kendo_herb").html("N/A") };

    var tar = null;
    if (tar != null) {
        $("#kendo_target").kendoGrid({
            dataSource: {
                data: tar,
                pageSize: 10,
            },
            pageable: true,
            sortable: true,
            filterable: true,
            resizable: true,
            columnMenu: true,
            columns: [
                // { field:"target_ID",title:"Target ID",width:"130px" },
                { field: "target_name", title: "Target name", template: "<a  href='target.php?qt=${target_ID}'>${target_name}</a>" },
                { field: "drugbank_ID", title: "Source", width: "100px", template: "#= (drugbank_ID!='h001') ? \"<a href='http://v3.drugbank.ca/molecules/\"+drugbank_ID+\"'><span class=\'label label-success\'>DrugBank</span></a>\" : 'N/A'#" }
            ]
        });
    } else { $("#kendo_target").html("N/A") };

    var dis = null;
    if (dis != null) {
        $("#kendo_disease").kendoGrid({
            dataSource: {
                data: dis,
                pageSize: 10,
            },
            pageable: true,
            sortable: true,
            filterable: true,
            resizable: true,
            columnMenu: true,
            columns: [
                // { field:"disease_ID",title:"Disease ID",width:"130px"},
                { field: "disease_name", title: "Disease name", template: "<a href='disease.php?qd=${disease_ID}'>${disease_name}</a>" }
            ]
        });
    } else { $("#kendo_disease").html("N/A") };
})