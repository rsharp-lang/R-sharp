let data = {"name": "Alice", "scores": [95, 87, 92], "active": True};

str(data);

pickle.dumps(data, file = relative_work("data.pkl"));

str(pickle.load(relative_work("data.pkl")));
