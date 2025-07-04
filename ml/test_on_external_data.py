import pandas as pd
import joblib
from sklearn.metrics import classification_report, confusion_matrix

# Load external test data
external_df = pd.read_csv('synthetic_external_latency.csv')
X_ext = external_df['latency_ms'].values.reshape(-1, 1)
y_ext = external_df['anomaly'].values

# Load the best model
model = joblib.load('isolation_forest_latency_best.pkl')

# Predict
y_pred = model.predict(X_ext)
y_pred = (y_pred == -1).astype(int)

# Print results
print('Classification report (external data):')
print(classification_report(y_ext, y_pred, digits=3))
print('\nConfusion matrix (external data):')
print(confusion_matrix(y_ext, y_pred)) 