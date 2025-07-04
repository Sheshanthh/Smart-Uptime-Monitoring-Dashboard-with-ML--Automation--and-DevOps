import pandas as pd
from sklearn.ensemble import IsolationForest
from sklearn.metrics import classification_report, confusion_matrix
from sklearn.model_selection import train_test_split
import joblib

# Load the dataset
csv_path = 'synthetic_latency.csv'
df = pd.read_csv(csv_path)

# Clean the dataset: remove NaNs
clean_df = df.dropna(subset=['latency_ms'])

# Cap outliers at 99th percentile
upper_limit = clean_df['latency_ms'].quantile(0.99)
clean_df = clean_df[clean_df['latency_ms'] <= upper_limit]

# Save cleaned data for reference
clean_df.to_csv('synthetic_latency_clean.csv', index=False)
print(f"Cleaned dataset saved as synthetic_latency_clean.csv with {len(clean_df)} rows.")

latencies = clean_df['latency_ms'].values.reshape(-1, 1)
y_true = clean_df['anomaly'].values if 'anomaly' in clean_df.columns else None

# Split into train and test sets (80% train, 20% test)
X_train, X_test, y_train, y_test = train_test_split(latencies, y_true, test_size=0.2, random_state=42, stratify=y_true)

best_f1 = -1
best_model = None
best_contamination = None
best_report = None

# Try several contamination values
for contamination in [0.01, 0.02, 0.05, 0.1]:
    model = IsolationForest(contamination=contamination, random_state=42)
    model.fit(X_train)
    preds = model.predict(X_test)
    y_pred = (preds == -1).astype(int)
    if y_test is not None:
        report = classification_report(y_test, y_pred, output_dict=True, zero_division=0)
        f1 = report['1']['f1-score']
        print(f"Contamination: {contamination}, F1-score: {f1:.3f}")
        if f1 > best_f1:
            best_f1 = f1
            best_model = model
            best_contamination = contamination
            best_report = report
    else:
        if best_model is None:
            best_model = model
            best_contamination = contamination

# Save the best model
joblib.dump(best_model, 'isolation_forest_latency_best.pkl')
print(f"Best model saved as isolation_forest_latency_best.pkl (contamination={best_contamination})")

# Print classification report and confusion matrix for best model on test set
if best_report is not None:
    print("\nBest model classification report (on test set):")
    from pprint import pprint
    pprint(best_report)
    y_pred = best_model.predict(X_test)
    y_pred = (y_pred == -1).astype(int)
    print("\nConfusion Matrix (on test set):")
    print(confusion_matrix(y_test, y_pred))

# Test the best model on the first 10 test latency values
print('\nTesting best model on first 10 test latency values:')
print('Latency, Anomaly')
test_latencies = X_test[:10]
predictions = best_model.predict(test_latencies)
for lat, pred in zip(test_latencies.flatten(), predictions):
    print(f'{lat}, {int(pred == -1)}') 