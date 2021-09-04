imports "xgboost" from "MLkit";

setwd(@dir);

# training phase
const ftrain = read.csv( "./train.csv", row.names = NULL, check.names = FALSE);
const fval = read.csv( "./test.csv", row.names = NULL, check.names = FALSE);
const params = list('categorical_features'= ["PRI_jet_num"],
          'early_stopping_rounds'= 10,
          'maximize'= TRUE,
          'eval_metric'= 'auc',
          'loss'= 'logloss',
          'eta'= 0.3,
          'num_boost_round'= 20,
          'max_depth'= 7,
          'scale_pos_weight'=1.,
          'subsample'= 0.8,
          'colsample'= 0.8,
          'min_child_weight'= 1.,
          'min_sample_split'= 5,
          'reg_lambda'= 1.,
          'gamma'= 0.,
          'num_thread'= -1
          );

print("parameters:");
str(params);

const model = xgboost(
	xgb.DMatrix(ftrain[, 1:(ncol(ftrain)-1)], label = ftrain[, ncol(ftrain)], categorical_features = ["PRI_jet_num"]), 
	xgb.DMatrix(fval[, 1:(ncol(fval)-1)], label = fval[, ncol(fval)], validate_set = TRUE), 
	params
);

# testing phase
const ftest = read.csv( "./test.csv", row.names = NULL, check.names = FALSE);
const testLabels = ftest[, "Label"];
const foutput = "./test_result.csv";
const foutput2 = "./test_result2.csv";

ftest[, "Label"] = NULL;

model
|> xgboost::predict(xgb.DMatrix(ftest))
|> writeLines(con = foutput)
;

# save the model
model
|> xgboost::serialize
|> writeLines('./tgb.model');

# load model and predict
const result_pred = readLines('./tgb.model')
|> xgboost::parseTree
|> xgboost::predict(xgb.DMatrix(ftest))
;

data.frame(predict = result_pred, label = testLabels)
|> write.csv(file = foutput2, row.names = FALSE)
;
