# CNN

feed-forward phase of deep Convolutional Neural Networks

+ [n_threads](CNN/n_threads.1) get/set of the CNN parallel thread number
+ [cnn](CNN/cnn.1) Create a new CNN model
+ [input_layer](CNN/input_layer.1) The input layer is a simple layer that will pass the data though and
+ [regression_layer](CNN/regression_layer.1) 
+ [conv_layer](CNN/conv_layer.1) This layer uses different filters to find attributes of the data that
+ [conv_transpose_layer](CNN/conv_transpose_layer.1) 
+ [lrn_layer](CNN/lrn_layer.1) This layer is useful when we are dealing with ReLU neurons. Why is that?
+ [tanh_layer](CNN/tanh_layer.1) Implements Tanh nonlinearity elementwise x to tanh(x)
+ [softmax_layer](CNN/softmax_layer.1) [*loss_layers] This layer will squash the result of the activations in the fully
+ [relu_layer](CNN/relu_layer.1) This is a layer of neurons that applies the non-saturating activation
+ [leaky_relu_layer](CNN/leaky_relu_layer.1) 
+ [maxout_layer](CNN/maxout_layer.1) Implements Maxout nonlinearity that computes x to max(x)
+ [sigmoid_layer](CNN/sigmoid_layer.1) Implements Sigmoid nonlinearity elementwise x to 1/(1+e^(-x))
+ [pool_layer](CNN/pool_layer.1) This layer will reduce the dataset by creating a smaller zoomed out
+ [dropout_layer](CNN/dropout_layer.1) This layer will remove some random activations in order to
+ [full_connected_layer](CNN/full_connected_layer.1) Neurons in a fully connected layer have full connections to all
+ [gaussian_layer](CNN/gaussian_layer.1) 
+ [sample_dataset](CNN/sample_dataset.1) 
+ [sample_dataset.image](CNN/sample_dataset.image.1) 
+ [auto_encoder](CNN/auto_encoder.1) 
+ [training](CNN/training.1) Do CNN network model training
+ [ada_delta](CNN/ada_delta.1) Adaptive delta will look at the differences between the expected result and the current result to train the network.
+ [ada_grad](CNN/ada_grad.1) The adaptive gradient trainer will over time sum up the square of
+ [adam](CNN/adam.1) Adaptive Moment Estimation is an update to RMSProp optimizer. In this running average of both the
+ [nesterov](CNN/nesterov.1) Another extension of gradient descent is due to Yurii Nesterov from 1983,[7] and has been subsequently generalized
+ [sgd](CNN/sgd.1) Stochastic gradient descent (often shortened in SGD), also known as incremental gradient descent, is a
+ [window_grad](CNN/window_grad.1) This is AdaGrad but with a moving window weighted average
+ [predict](CNN/predict.1) 
+ [CeNiN](CNN/CeNiN.1) load a CNN model from file
+ [detectObject](CNN/detectObject.1) classify a object from a given image data
+ [saveModel](CNN/saveModel.1) save the CNN model into a binary data file
