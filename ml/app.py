from flask import Flask, request, jsonify
import joblib
import numpy as np

app = Flask(__name__)
model = joblib.load('isolation_forest_latency_best.pkl')

@app.route('/predict', methods=['POST'])
def predict():
    data = request.get_json()
    if not data or 'latency_ms' not in data:
        return jsonify({'error': 'Missing latency_ms in request'}), 400
    try:
        latency = float(data['latency_ms'])
    except Exception:
        return jsonify({'error': 'latency_ms must be a number'}), 400
    pred = model.predict(np.array([[latency]]))
    is_anomaly = int(pred[0] == -1)
    return jsonify({'anomaly': is_anomaly})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000) 