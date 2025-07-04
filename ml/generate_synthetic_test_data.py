import numpy as np
import pandas as pd

np.random.seed(42)


normal_latencies = np.random.normal(loc=120, scale=15, size=1000)
normal_labels = np.zeros(1000, dtype=int)


anomaly_latencies = np.random.normal(loc=300, scale=20, size=20)
anomaly_labels = np.ones(20, dtype=int)


latencies = np.concatenate([normal_latencies, anomaly_latencies])
labels = np.concatenate([normal_labels, anomaly_labels])

data = pd.DataFrame({
    'latency_ms': latencies,
    'anomaly': labels
})

data = data.sample(frac=1, random_state=42).reset_index(drop=True)

data.to_csv('synthetic_external_latency.csv', index=False)
print('Synthetic external test dataset saved as synthetic_external_latency.csv') 