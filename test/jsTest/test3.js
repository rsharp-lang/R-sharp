$(function () {
    var sname = [{ "molecule_synonyms": "[(1R)-1-(5,8-dihydroxy-1,4-dioxonaphthalen-2-yl)-4-methylpent-3-enyl] 3-hydroxy-3-methylbutanoate" }, { "molecule_synonyms": "[(1R)-1-(5,8-dihydroxy-1,4-dioxo-2-naphthyl)-4-methyl-pent-3-enyl] 3-hydroxy-3-methyl-butanoate" }, { "molecule_synonyms": "3-hydroxy-3-methylbutanoic acid [(1R)-1-(5,8-dihydroxy-1,4-dioxo-2-naphthyl)-4-methylpent-3-enyl] ester" }, { "molecule_synonyms": "3-hydroxy-3-methyl-butyric acid [(1R)-1-(5,8-dihydroxy-1,4-diketo-2-naphthyl)-4-methyl-pent-3-enyl] ester" }, { "molecule_synonyms": "[(1R)-1-(5,8-dihydroxy-1,4-dioxo-naphthalen-2-yl)-4-methyl-pent-3-enyl] 3-hydroxy-3-methyl-butanoate" }];
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

    var mol = [{ "pubchem_cid": "479502", "MOL_ID": "MOL007739", "molecule_ID": "7739", "molecule_name": "[(1R)-1-(5,8-dihydroxy-1,4-dioxo-2-naphthyl)-4-methyl-pent-3-enyl] 3-hydroxy-3-methyl-butanoate", "tpsa": "121.13", "rbn": "7", "inchikey": "MXANJRGHSFELEJ-MRXNPFEDSA-N", "ob": "7.92590750811", "dl": "0.38869", "bbb": "-0.76089", "caco2": "-0.24206", "mw": "388.450", "hdon": "3", "hacc": "7", "alogp": "2.932", "halflife": "", "FASA": "0.35677525" }];
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

    var herb = [{ "herb_cn_name": "\u7d2b\u8349", "herb_en_name": "Lithospermum Erythrorhizon" }];
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

    var tar = [{ "target_ID": "46", "target_name": "Estrogen receptor", "drugbank_ID": "136", "SVM_score": "0.95264", "RF_score": "0.756" }, { "target_ID": "48", "target_name": "Androgen receptor", "drugbank_ID": "146", "SVM_score": "0.90088", "RF_score": "0.764" }, { "target_ID": "79", "target_name": "Coagulation factor Xa", "drugbank_ID": "239", "SVM_score": "0.89726", "RF_score": "0.732" }, { "target_ID": "94", "target_name": "Prostaglandin G\/H synthase 2", "drugbank_ID": "290", "SVM_score": "0.9269", "RF_score": "0.748" }, { "target_ID": "117", "target_name": "Carbonic anhydrase II", "drugbank_ID": "357", "SVM_score": "0.85796", "RF_score": "0.768" }, { "target_ID": "287", "target_name": "DNA topoisomerase II", "drugbank_ID": "817", "SVM_score": "0.88883", "RF_score": "0.716" }, { "target_ID": "444", "target_name": "Heat shock protein HSP 90", "drugbank_ID": "1939", "SVM_score": "0.97264", "RF_score": "0.706" }, { "target_ID": "2966", "target_name": "Proto-oncogene serine\/threonine-protein kinase Pim-1", "drugbank_ID": "2347", "SVM_score": "0.83512", "RF_score": "0.7" }];
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

    var dis = [{ "disease_name": "Brain injury", "disease_ID": "113" }, { "disease_name": "Breast cancer", "disease_ID": "117" }, { "disease_name": "Cardiovascular disease, unspecified", "disease_ID": "144" }, { "disease_name": "Coronary atherosclerosis", "disease_ID": "206" }, { "disease_name": "Endocrine independent cancer", "disease_ID": "270" }, { "disease_name": "Neurodegenerative diseases", "disease_ID": "550" }, { "disease_name": "Osteoporosis, unspecified", "disease_ID": "595" }, { "disease_name": "Postmenopausal symptoms", "disease_ID": "647" }, { "disease_name": "Osteoporosis", "disease_ID": "592" }, { "disease_name": "Myocardial Infarction", "disease_ID": "533" }, { "disease_name": "Migraine", "disease_ID": "517" }, { "disease_name": "Cardiovascular disease", "disease_ID": "143" }, { "disease_name": "Prostate cancer", "disease_ID": "659" }, { "disease_name": "Spinal and bulbar muscular atrophy", "disease_ID": "747" }, { "disease_name": "XY disorders of sex development (Other)", "disease_ID": "1006" }, { "disease_name": "XY disorders of sex development (Disorders in androgen synthesis or action)", "disease_ID": "1005" }, { "disease_name": "Spinal and bulbar muscular atrophy of Kennedy", "disease_ID": "1004" }, { "disease_name": "Hypospadias 1, X-linked", "disease_ID": "1003" }, { "disease_name": "Androgen insensitivity", "disease_ID": "1002" }, { "disease_name": "Atrial fibrillation and flutter", "disease_ID": "83" }, { "disease_name": "Coagulative disorders", "disease_ID": "192" }, { "disease_name": "Thromboembolic disorders", "disease_ID": "770" }, { "disease_name": "Thromboembolism", "disease_ID": "771" }, { "disease_name": "Thrombosis", "disease_ID": "772" }, { "disease_name": "Thrombotic disease", "disease_ID": "773" }, { "disease_name": "Abdominal aortic aneurysm", "disease_ID": "1" }, { "disease_name": "Adenomatous polyposis", "disease_ID": "24" }, { "disease_name": "Alzheimer's Disease", "disease_ID": "51" }, { "disease_name": "Analgesics", "disease_ID": "55" }, { "disease_name": "Arthritis", "disease_ID": "75" }, { "disease_name": "Bladder cancer", "disease_ID": "103" }, { "disease_name": "Cancer, unspecific", "disease_ID": "130" }, { "disease_name": "Carcinoma in situ, unspecified", "disease_ID": "134" }, { "disease_name": "Carpal tunnel syndrome", "disease_ID": "147" }, { "disease_name": "Colorectal cancer", "disease_ID": "200" }, { "disease_name": "Dysmenorrhea, unspecified", "disease_ID": "264" }, { "disease_name": "Endometriosis", "disease_ID": "273" }, { "disease_name": "Genitourinary tumors", "disease_ID": "325" }, { "disease_name": "Gestational hypertension", "disease_ID": "327" }, { "disease_name": "Inflammation", "disease_ID": "416" }, { "disease_name": "Inflammatory diseases", "disease_ID": "418" }, { "disease_name": "Lung Cancer", "disease_ID": "471" }, { "disease_name": "Malignant mesothelioma", "disease_ID": "496" }, { "disease_name": "Meningioma", "disease_ID": "508" }, { "disease_name": "Oropharyngeal squamous cell carcinoma", "disease_ID": "585" }, { "disease_name": "Osteoarthritis", "disease_ID": "586" }, { "disease_name": "Pain, unspecified", "disease_ID": "611" }, { "disease_name": "Pathological angiogenesis", "disease_ID": "622" }, { "disease_name": "Peutz-Jeghers syndrome", "disease_ID": "633" }, { "disease_name": "Pyresis", "disease_ID": "676" }, { "disease_name": "Renal Cell Carcinoma", "disease_ID": "689" }, { "disease_name": "Rheumatoid arthritis, unspecified", "disease_ID": "703" }, { "disease_name": "Stroke", "disease_ID": "754" }, { "disease_name": "Vascular lesion regression", "disease_ID": "814" }, { "disease_name": "Glaucoma", "disease_ID": "330" }, { "disease_name": "Pancreatic Cancer", "disease_ID": "612" }, { "disease_name": "Renal failure", "disease_ID": "692" }, { "disease_name": "Acute promyelocytic leukemia", "disease_ID": "19" }, { "disease_name": "Bacterial Infections", "disease_ID": "93" }, { "disease_name": "Fungal diseases", "disease_ID": "304" }, { "disease_name": "Herpes virus infection", "disease_ID": "372" }, { "disease_name": "Leishmania Infections", "disease_ID": "454" }, { "disease_name": "Malaria", "disease_ID": "489" }, { "disease_name": "Trichomoniasis", "disease_ID": "790" }, { "disease_name": "Chronic Myelogenous Leukemia (CML)", "disease_ID": "178" }, { "disease_name": "Gastrointestinal Stromal Tumors (GIST)", "disease_ID": "321" }, { "disease_name": "Hematological Malignancies", "disease_ID": "357" }, { "disease_name": "HER2-positive Metastatic Breast Cancer", "disease_ID": "367" }, { "disease_name": "Melanoma", "disease_ID": "505" }, { "disease_name": "Multiple Myeloma", "disease_ID": "527" }, { "disease_name": "Non-small Cell Lung Cancer", "disease_ID": "569" }, { "disease_name": "Ovarian cancer", "disease_ID": "597" }, { "disease_name": "Refractory Hematological Malignancies", "disease_ID": "683" }, { "disease_name": "Solid tumors", "disease_ID": "744" }];
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