import pandas as pd
import joblib


df = pd.read_csv('synthetic_latency.csv')

# Remove rows with missing latency values
df_clean = df.dropna(subset=['latency_ms'])

upper_limit = df_clean['latency_ms'].quantile(0.99)
df_clean = df_clean[df_clean['latency_ms'] <= upper_limit]

# Save cleaned data for reference
df_clean.to_csv('synthetic_latency_clean.csv', index=False)
print(f"Cleaned dataset saved as synthetic_latency_clean.csv with {len(df_clean)} rows.")

# Load the trained model
model = joblib.load('isolation_forest_latency.pkl')

# Test on the first 10 latency values
print('Testing model on first 10 latency values:')
predictions = model.predict(latencies[:10])
for i, (lat, pred) in enumerate(zip(latencies[:10].flatten(), predictions)):
    print(f'Latency: {lat}, Anomaly: {int(pred == -1)}') 